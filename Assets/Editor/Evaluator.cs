using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Editor;
using UnityEditor;
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
            _options = ScriptOptions.Default.WithReferences(Assemblies).WithImports("UnityEngine", "UnityEditor", "System", "System.Collections", "System.Collections.Generic");
        }
    }
    
    public static async void Execute(Notebook notebook, Notebook.Cell cell)
    {
        // cancel the current token from notebook.cancellationTokenSource if it exists
        notebook.cancellationTokenSource?.Cancel();
        notebook.cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = notebook.cancellationTokenSource.Token;

        Init();
        cell.outputs.Clear();
        try
        {
            if (notebook.scriptState == null)
            {
                notebook.scriptState = await CSharpScript.RunAsync(string.Concat(cell.source), _options,
                    cancellationToken: cancellationToken);
            }
            else
            {
                notebook.scriptState = await notebook.scriptState.ContinueWithAsync(string.Concat(cell.source),
                    _options, cancellationToken: cancellationToken);
            }
            if (notebook.scriptState.Exception != null)
            {
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
            notebook.cancellationTokenSource = null;
        }
    }
    
    public static void Stop(Notebook notebook)
    {
        if (notebook.cancellationTokenSource == null)
        {
            return;
        }
        if (notebook.cancellationTokenSource.Token.CanBeCanceled)
        {
            notebook.cancellationTokenSource.Cancel();
        }
    }
}
