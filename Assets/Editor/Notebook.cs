using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Serialization;
using MG.MDV;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;

// https://ipython.org/ipython-doc/3/notebook/nbformat.html

[ScriptedImporter(1, "ipynb")]
public class NotebookImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var json = System.IO.File.ReadAllText(ctx.assetPath);
        var notebook = ScriptableObject.CreateInstance<Notebook>();
        notebook.PopulateFromJson(json);
        
        ctx.AddObjectToAsset("main", notebook);
        ctx.SetMainObject(notebook);
    }
}

public class Notebook : ScriptableObject
{
    public int format;
    public int formatMinor;
    public Metadata metadata;
    public List<Cell> cells = new();

    public static void Create(string path)
    {
        var notebook = CreateInstance<Notebook>();
        notebook.metadata = new Metadata();
        notebook.format = 4;
        notebook.formatMinor = 0;
        notebook.cells = new List<Cell>();
        notebook.Save(path);
    }

    public void Save(string path)
    {
        var assetPath = AssetDatabase.GetAssetPath(this);
        var json = ConvertToJson();
        System.IO.File.WriteAllText(assetPath, json);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    public void PopulateFromJson(string json)
    {
        var nb = JObject.Parse(json);
        format = (nb["nbformat"] ?? 4).Value<int>();
        formatMinor = (nb["nbformat_minor"] ?? 0).Value<int>();
        
        // TODO parse file metadata
        // metadata = nb["metadata"] != null ? nb["metadata"].ToObject<Metadata>() : new Metadata();

        foreach (var rawCell in nb["cells"]?.ToObject<JArray>())
        {
            var cell = rawCell.ToObject<Cell>();
            cells.Add(cell);
        }
    }

    public string ConvertToJson()
    {
        var nb = new JObject
        {
            ["nbformat"] = format,
            ["nbformat_minor"] = formatMinor,
            ["metadata"] = JsonConvert.SerializeObject(metadata),
            ["cells"] = new JArray(cells.Select(JsonConvert.SerializeObject))
        };
        return nb.ToString();
    }
    
    [Serializable]
    public class Metadata
    {
        public string signature;
        public string kernel_info;
        public string language_info;
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
        
        // UI rendering
        [NonSerialized] public MarkdownViewer markdownViewer;
        [NonSerialized] public TextBlock textBlock;
    }
    
    public enum AutoScroll { True, False, Auto }
    
    [Serializable]
    [JsonConverter(typeof(CellMetadataConverter))]
    public class CellMetadata
    {
        public bool collapsed;
        public AutoScroll autoscroll;
        public bool deletable;
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

