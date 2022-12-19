using System;
using System.Collections.Generic;
using System.Threading;
using Editor.Serialization;
using MG.MDV;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

// https://ipython.org/ipython-doc/3/notebook/nbformat.html

[JsonConverter(typeof(NotebookConverter))]
public class Notebook : ScriptableObject, ISerializationCallbackReceiver
{
    public int format = 4;
    public int formatMinor = 2;
    public Metadata metadata = new();
    public List<Cell> cells = new();
    
    [NonSerialized] public ScriptState scriptState;
    
    public bool IsRunning { get; set; }

    public void OnBeforeSerialize()
    {
        // foreach (var cell in cells)
        // {
        //     if (cell.textBlock != null && cell.textBlock.CharacterCount > 0)
        //     {
        //         cell.source = cell.textBlock.GetLines();
        //     }
        // }
        
        // TODO write to json file?
        // Will this trigger a reimport loop?
        
    }

    public void OnAfterDeserialize()
    {
        // foreach (var cell in cells)
        // {
        //     if (cell.textBlock == null)
        //     {
        //         cell.textBlock = new TextBlock();
        //     }
        //     cell.textBlock.SetText(cell.source);
        //     Debug.Log("Characters: " + cell.textBlock.CharacterCount);
        // }
    }

    public static Notebook CreateAsset(string path)
    {
        var notebook = CreateInstance<Notebook>();
        var json = JsonConvert.SerializeObject(notebook, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Notebook>(path);
    }

    [Serializable]
    public class Metadata
    {
        // TODO preserve existing metadata during deserialization / reserialization
        public Kernelspec kernelspec = new();
    }

    public class Kernelspec
    {
        public string display_name = ".Net (C#)";
        public string language = "C#";
        public string name = ".net-csharp";
    }
    
    [Serializable]
    public class KernelInfo
    {
        public string name;
    }

    public enum CellType { Markdown, Code, Raw }
    
    [Serializable]
    [JsonConverter(typeof(CellConverter))]
    public class Cell
    {
        // common
        public CellType cellType; // markdown, code
        public CellMetadata metadata; // empty object if its a markdown cell
        public string[] source; // could be a single string or a list of strings
        
        // code
        public List<CellOutput> outputs = new();
        
        // UI
        [NonSerialized] public MarkdownViewer markdownViewer;
        [NonSerialized] public string rawText;
        [NonSerialized] public string highlightedText = "";
    }
    
    public enum AutoScroll { True, False, Auto }
    
    [Serializable]
    [JsonConverter(typeof(CellMetadataConverter))]
    public class CellMetadata
    {
        public bool collapsed;
        public AutoScroll autoscroll;
        public bool deletable = true;
        // TODO support Raw NBConvert Cell
        public string format; // The mime-type of a Raw NBConvert Cell
        public string name;
        public List<string> tags;
    }
    
    public enum OutputType { Stream, DisplayData, ExecuteResult, Error }
    
    [Serializable]
    [JsonConverter(typeof(CellOutputConverter))]
    public class CellOutput
    {
        public OutputType outputType;
        
        // stream output
        public string name; // if stream output cell: stdout, stderr
        public List<string> text = new();
        
        // display data output or execute result output
        public List<CellOutputDataEntry> data = new(); // mime-type -> data, often text/plain, image/png, application/json
        public List<CellOutputMetadataEntry> metadata = new(); // mime-type -> metadata
        
        // execute result output
        public int executionCount;
        
        // error output
        public string ename;
        public string evalue;
        public List<string> traceback = new();
    }
    
    [Serializable]
    [JsonConverter(typeof(CellOutputDataEntryConverter))]
    public class CellOutputDataEntry
    {
        public string mimeType;
        public List<string> stringData = new();
        public Texture2D imageData;
    }
    
    [Serializable]
    public class CellOutputMetadataEntry
    {
        public string mimeType;
    }
}

