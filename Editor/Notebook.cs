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
public class Notebook : ScriptableObject
{
    public int format = 4;
    public int formatMinor = 2;
    public Metadata metadata = new();
    public List<Cell> cells = new();
    
    [NonSerialized] public ScriptState scriptState;

    // Saves the current ScriptableObject data back to the underlying json asset file
    public void SaveJson()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(this), json);
        AssetDatabase.Refresh();
    }

    public static Notebook CreateAsset(string path)
    {
        var notebook = CreateInstance<Notebook>();
        var json = JsonConvert.SerializeObject(notebook, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Notebook>(path);
    }

    [MenuItem("Assets/Create/Notebook", false, 80)]
    public static void CreateAssetMenu()
    {
        // Get the path to the currently selected folder
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
        {
            path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        
        // Create asset
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/New Notebook.ipynb");
        var asset = CreateAsset(assetPath);
        Selection.activeObject = asset;
    }

    public void ClearOutputs()
    {
        foreach (var cell in cells)
        {
            cell.executionCount = 0;
            cell.outputs.Clear();
        }
        EditorUtility.SetDirty(this);
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
        public int executionCount;
        public CellMetadata metadata; // empty object if its a markdown cell
        public string[] source = Array.Empty<string>(); // could be a single string or a list of strings
        
        // code
        public List<CellOutput> outputs = new();
        
        // UI
        [NonSerialized] public MarkdownViewer markdownViewer;
        [NonSerialized] public string rawText;
        [NonSerialized] public string highlightedText = "";
        [NonSerialized] public Vector2 scroll;
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

        public static CellOutput DisplayResult(CellOutputDataEntry dataEntry)
        {
            return new CellOutput
            {
                outputType = OutputType.DisplayData,
                data = new List<CellOutputDataEntry> { dataEntry }
            };
        }
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

