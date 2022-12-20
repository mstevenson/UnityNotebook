using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Reflection;
using Editor;
using UnityEngine;

// https://github.com/dotnet/roslyn/blob/315c2e149ba7889b0937d872274c33fcbfe9af5f/docs/wiki/Scripting-API-Samples.md
// https://gsferreira.com/archive/2016/02/the-shining-new-csharp-scripting-api/

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
            var imports = new[] { "RuntimeMethods", "UnityEngine", "UnityEditor", "System", "System.Collections", "System.Collections.Generic" };
            _options = ScriptOptions.Default.WithReferences(Assemblies).WithImports(imports);
        }
    }

    public static void Execute(Notebook notebook, Notebook.Cell cell)
    {
        notebook.IsRunning = true;
        cell.executionCount += 1;
        // TODO can't call Unity APIs from a thread other than the main thread
        // Task.Run(() => ExecuteInternal(notebook, cell));
        ExecuteInternal(notebook, cell);
    }

    private static async void ExecuteInternal(Notebook notebook, Notebook.Cell cell)
    {
        // cancel the current token from notebook.cancellationTokenSource if it exists
        
        Init();
        cell.outputs.Clear();
        try
        {
            var code = string.Concat(cell.source);
            
            // turn into a coroutine if there's a yield statement
            // TODO use roslyn to analyze this?
            if (code.Contains("yield "))
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
                cell.outputs.Add(NotebookUtils.Exception(notebook.scriptState.Exception));
            }
            else
            {
                switch (notebook.scriptState.ReturnValue)
                {
                    case null:
                        break;
                    case string str:
                        cell.outputs.Add(NotebookUtils.DisplayData(str));
                        break;
                    case Texture2D tex:
                        cell.outputs.Add(NotebookUtils.DisplayData(tex));
                        break;
                    default:
                        cell.outputs.Add(NotebookUtils.DisplayData(notebook.scriptState.ReturnValue));
                        break;
                }
            }
        }
        finally
        {
            notebook.IsRunning = false;
        }
    }

    public static void Stop(Notebook notebook)
    {
        NotebookCoroutine.StopAll();
    }
}
