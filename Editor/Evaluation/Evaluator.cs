using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;

namespace UnityNotebook
{
    /// <summary>
    /// Execute arbitrary C# code in the Unity editor
    /// </summary>
    public static class Evaluator
    {
        private static readonly List<MetadataReference> Assemblies = new();
        private static ScriptOptions _options;
        private static EditorCoroutine _sequenceCoroutine;
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize()
        {
            Assemblies.Clear();
        }

        private static void Init()
        {
            if (Assemblies.Count != 0)
            {
                return;
            }

#if UNITY_2022_1_OR_NEWER
            //CoreClrShim's AssemblyLoadContext is used for determining if the environment is CoreCLR.
            //However, the newer Unity's mscorlib includes System.Runtime.Loader.AssemblyLoadContext,
            //which is required by AssemblyLoaderContext. As a result, the InteractiveAssemblyLoader
            //that CSharpScript uses to prepare the assembly might mistakenly identify the environment as CoreCLR.
            //This code fixes this problem.

            Type typeofCoreClrShim = typeof(InteractiveAssemblyLoader)
                .Assembly
                .GetType("Microsoft.CodeAnalysis.CoreClrShim");
            
            if (typeofCoreClrShim != null)
            {
                Type typeofAssemblyLoadContext = typeofCoreClrShim
                    .GetNestedType("AssemblyLoadContext", BindingFlags.Static | BindingFlags.NonPublic);

                FieldInfo typeField = typeofAssemblyLoadContext.GetField("Type", BindingFlags.Static | BindingFlags.NonPublic);

                typeField.SetValue(null, null);
            }
            else
            {
                UnityEngine.Debug.LogError($"[UnityNotebook] A {nameof(NotImplementedException)} may be thrown by {nameof(InteractiveAssemblyLoader)}.");
            }
#endif
            
            string workingDir = System.IO.Directory.GetCurrentDirectory();
            var app = AppDomain.CurrentDomain;
            var allAssemblies = app.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                {
                    if (assembly.Location.StartsWith(workingDir))
                    {
                        Assemblies.Add(MetadataReference.CreateFromImage(System.IO.File.ReadAllBytes(assembly.Location)));
                    }
                    else
                    {
                        Assemblies.Add(MetadataReference.CreateFromFile(assembly.Location));
                    }
                }
            }

            var imports = new[]
            {
                "UnityNotebook.RuntimeMethods", "UnityEngine", "UnityEditor", "System.Collections",
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
                if (notebook.cells[i].cellType != CellType.Code)
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

        public static async Task<CellOutput> ExecuteCodeAsync(string code)
        {
            Init();
            
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
                var output = new CellOutputError
                {
                    outputType = OutputType.Error,
                    ename = e.GetType().Name,
                    evalue = e.Message,
                    traceback = new List<string>(e.StackTrace.Split('\n'))
                };
                OnExecutionEnded();
                return output;
            }
            finally
            {
                if (!isCoroutine)
                {
                    OnExecutionEnded();
                }
            }
            return null;
        }

        private static async void ExecuteCellAsync(Notebook notebook, int cell)
        {
            notebook.cells[cell].outputs.Clear();
            var code = string.Concat(notebook.cells[cell].source);
            var output = await ExecuteCodeAsync(code);
            if (output != null)
            {
                notebook.cells[cell].outputs.Add(output);
            }
        }

        private static void OnExecutionEnded()
        {
            NBState.RunningCell = -1;
            NBState.SaveScriptableObject();
            NBState.SetNotebookDirty();
        }
        
        public static void Stop()
        {
            NotebookCoroutine.StopAll();
        }
    }
}