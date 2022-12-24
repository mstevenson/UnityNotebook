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
    /// <summary>
    /// Execute arbitrary C# code in the Unity editor
    /// </summary>
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
            NBState.RunningCell = cell;
            notebook.cells[cell].executionCount += 1;
            ExecuteCellAsync(notebook, cell);
        }
        
        public static void ExecuteAllCells(Notebook notebook, Action completionCallback)
        {
            foreach (var cell in notebook.cells)
            {
                cell.outputs.Clear();
            }
            if (_sequenceCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_sequenceCoroutine);
            }
            _sequenceCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteSequenceCoroutine(notebook));
            completionCallback?.Invoke();
        }

        private static IEnumerator ExecuteSequenceCoroutine(Notebook notebook)
        {
            for (var i = 0; i < notebook.cells.Count; i++)
            {
                if (notebook.cells[i].cellType != Notebook.CellType.Code)
                {
                    continue;
                }
                ExecuteCell(notebook, i);
                // Wait for cell to finish executing
                while (NBState.RunningCell != -1)
                {
                    yield return null;
                }
                // Pause a frame to allow the UI to update
                yield return null;
            }
        }

        private static async void ExecuteCellAsync(Notebook notebook, int cell)
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
                if (NBState.instance.scriptState == null)
                {
                    NBState.instance.scriptState = await CSharpScript.RunAsync(code, _options);
                }
                else
                {
                    NBState.instance.scriptState = await NBState.instance.scriptState.ContinueWithAsync(code, _options);
                }

                // Capture return values. Coroutine yielded values call CaptureOutput directly.
                RuntimeMethods.Show(NBState.instance.scriptState.ReturnValue);
            }
            catch (Exception e)
            {
                var output = new Notebook.CellOutput
                {
                    outputType = Notebook.OutputType.Error,
                    ename = e.GetType().Name,
                    evalue = e.Message,
                    traceback = new List<string>(e.StackTrace.Split('\n'))
                };
                notebook.cells[cell].outputs.Add(output);
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
            NBState.RunningCell = -1;
            NBState.OpenedNotebook.SaveScriptableObject();
        }
        
        public static void Stop()
        {
            NotebookCoroutine.StopAll();
        }
    }
}