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
    public class Evaluator
    {
        private static readonly List<Assembly> Assemblies = new();
        private static ScriptOptions _options;

        public static void Init()
        {
            if (Assemblies.Count == 0)
            {
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
        }

        public static void Execute(Notebook notebook, int cell)
        {
            NotebookWindowData.instance.runningCell = cell;
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

        private static EditorCoroutine _sequenceCoroutine;

        private static IEnumerator ExecuteSequence(Notebook notebook)
        {
            for (var i = 0; i < notebook.cells.Count; i++)
            {
                if (notebook.cells[i].cellType != Notebook.CellType.Code)
                {
                    continue;
                }

                Execute(notebook, i);
                while (NotebookWindowData.instance.runningCell != -1)
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
                if (isCoroutine)
                {
                    code = $"IEnumerator EvaluateCoroutine() {{ {code} }} NotebookCoroutine.Run(EvaluateCoroutine());";
                }

                if (notebook.scriptState == null)
                {
                    notebook.scriptState = await CSharpScript.RunAsync(code, _options);
                }
                else
                {
                    notebook.scriptState = await notebook.scriptState.ContinueWithAsync(code, _options);
                }

                if (notebook.scriptState.Exception != null)
                {
                    Debug.LogError(notebook.scriptState.Exception.Message);
                    notebook.cells[cell].outputs.Add(NotebookUtils.Exception(notebook.scriptState.Exception));
                }
                else
                {
                    switch (notebook.scriptState.ReturnValue)
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
                            notebook.cells[cell].outputs
                                .Add(NotebookUtils.DisplayData(notebook.scriptState.ReturnValue));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                notebook.cells[cell].outputs.Add(NotebookUtils.Exception(e));
                NotebookWindowData.instance.runningCell = -1;
            }
            finally
            {
                if (!isCoroutine)
                {
                    NotebookWindowData.instance.runningCell = -1;
                }
            }
        }

        public static void Stop()
        {
            NotebookCoroutine.StopAll();
        }
    }
}