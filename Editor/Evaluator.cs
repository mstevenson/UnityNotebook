using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.EditorCoroutines.Editor;

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
                "System.Collections.Generic", "System.Linq"
            };
            _options = ScriptOptions.Default.WithReferences(Assemblies).WithImports(imports);
        }

        public static void ExecuteCell(Notebook notebook, int cell)
        {
            NotebookWindowData.RunningCell = cell;
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
                while (NotebookWindowData.RunningCell != -1)
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
                OnExecutionEnded();
            }
            finally
            {
                if (!isCoroutine)
                {
                    OnExecutionEnded();
                }
            }
        }

        private static void OnExecutionEnded()
        {
            NotebookWindowData.RunningCell = -1;
            NotebookWindowData.OpenedNotebook.SaveAsset();
        }
        
        public static void CaptureOutput(object obj)
        {
            if (obj == null)
            {
                return;
            }
            var notebook = NotebookWindowData.OpenedNotebook;
            var cell = NotebookWindowData.RunningCell;
            var output = Renderers.GetCellOutputForObject(obj);
            notebook.cells[cell].outputs.Add(output);
        }

        public static void Stop()
        {
            NotebookCoroutine.StopAll();
        }
    }
}