using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace UnityNotebook
{
    public static class Evaluator
    {
        private static readonly List<Assembly> Assemblies = new();
        private static ScriptOptions _options;
        private static EditorCoroutine _sequenceCoroutine;

        private static void Init()
        {
            if (Assemblies.Count != 0)
            {
                return;
            }
            var app = AppDomain.CurrentDomain;
            var allAssemblies = app.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                {
                    Assemblies.Add(assembly);
                }
            }

            var imports = new[]
            {
                "UnityNotebook.RuntimeMethods", "UnityEngine", "UnityEditor", "System", "System.Collections",
                "System.Collections.Generic"
            };
            _options = ScriptOptions.Default.WithReferences(Assemblies).WithImports(imports);
        }

        public static void ExecuteCell(Notebook notebook, int cell)
        {
            NotebookWindowData.instance.RunningCell = cell;
            notebook.cells[cell].executionCount += 1;
            ExecuteInternal(notebook, cell);
        }

        public static void ExecuteAll(Notebook notebook)
        {
            foreach (var cell in notebook.cells)
            {
                cell.outputs.Clear();
            }
            if (_sequenceCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_sequenceCoroutine);
            }
            _sequenceCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteSequence(notebook));
        }

        private static IEnumerator ExecuteSequence(Notebook notebook)
        {
            for (var i = 0; i < notebook.cells.Count; i++)
            {
                if (notebook.cells[i].cellType != Notebook.CellType.Code)
                {
                    continue;
                }
                ExecuteCell(notebook, i);
                while (NotebookWindowData.instance.RunningCell != -1)
                {
                    yield return null;
                }
            }
        }

        private static async void ExecuteInternal(Notebook notebook, int cell)
        {
            Init();
            notebook.cells[cell].outputs.Clear();
            var code = string.Concat(notebook.cells[cell].source);
            var isCoroutine = code.Contains("yield ");
            try
            {
                // Run the code
                if (isCoroutine)
                {
                    code = $"IEnumerator EvaluateCoroutine() {{ {code} }} UnityNotebook.NotebookCoroutine.Run(EvaluateCoroutine());";
                }
                if (notebook.scriptState == null)
                {
                    notebook.scriptState = await CSharpScript.RunAsync(code, _options);
                }
                else
                {
                    notebook.scriptState = await notebook.scriptState.ContinueWithAsync(code, _options);
                }

                // Capture return values. Coroutine yielded values call CaptureOutput directly.
                CaptureOutput(notebook.scriptState.ReturnValue);
            }
            catch (Exception e)
            {
                notebook.cells[cell].outputs.Add(NotebookUtils.Exception(e));
            }
            finally
            {
                if (!isCoroutine)
                {
                    NotebookWindowData.instance.RunningCell = -1;
                    NotebookWindowData.instance.OpenedNotebook.SaveAsset();
                }
            }
        }
        
        public static void CaptureOutput(object obj)
        {
            var notebook = NotebookWindowData.instance.OpenedNotebook;
            var cell = NotebookWindowData.instance.RunningCell;
            
            switch (obj)
            {
                case null:
                    break;
                case string str:
                    notebook.cells[cell].outputs.Add(NotebookUtils.DisplayData(str));
                    break;
                case Texture2D tex:
                    notebook.cells[cell].outputs.Add(NotebookUtils.DisplayData(tex));
                    break;
                default:
                    notebook.cells[cell].outputs.Add(NotebookUtils.DisplayData(obj));
                    break;
            }
        }

        public static void Stop()
        {
            NotebookCoroutine.StopAll();
        }
    }
}