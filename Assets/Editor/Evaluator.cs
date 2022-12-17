using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// https://github.com/dotnet/roslyn/blob/315c2e149ba7889b0937d872274c33fcbfe9af5f/docs/wiki/Scripting-API-Samples.md
// https://gsferreira.com/archive/2016/02/the-shining-new-csharp-scripting-api/

public class Evaluator
{
    public static bool IsRunning { get; private set; }
    
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
            _options = ScriptOptions.Default.WithReferences(Assemblies);
        }
    }

    public static void Execute(string code)
    {
        IsRunning = true;
        Init();
        CSharpScript.RunAsync(code, _options);
        IsRunning = false;
    }

    public static async void Execute(Notebook.Cell cell)
    {
        IsRunning = true;
        Init();
        cell.outputs.Clear();
        var script = await CSharpScript.RunAsync(string.Concat(cell.source), _options);
        IsRunning = false;
        if (script.Exception != null)
        {
            cell.outputs.Add(Notebook.CellOutput.Exception(script.Exception));
        }
        else
        {
            switch (script.ReturnValue)
            {
                case null:
                    break;
                case string str:
                    cell.outputs.Add(Notebook.CellOutput.DisplayData(str));
                    break;
                case Texture2D tex:
                    cell.outputs.Add(Notebook.CellOutput.DisplayData(tex));
                    break;
                default:
                    cell.outputs.Add(Notebook.CellOutput.DisplayData(script.ReturnValue));
                    break;
            }
        }
    }
    
    public static void Stop()
    {
    }
}
