using System;
using System.Collections.Generic;
using System.Text;
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
        var notebook = Notebook.Load(ctx.assetPath);
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
    
    public static Notebook Load(string assetPath)
    {
        var json = System.IO.File.ReadAllText(assetPath);
        var notebook = CreateInstance<Notebook>();
        notebook.PopulateFromJson(json);
        return notebook;
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
            var cell = Cell.Parse(rawCell);
            cells.Add(cell);
        }
    }

    public string ConvertToJson()
    {
        throw new NotImplementedException();
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
    public class Cell
    {
        // common
        public CellType cellType; // markdown, code
        public CellMetadata metadata; // empty object if its a markdown cell
        public List<string> source = new(); // could be a single string or a list of strings
        
        // code
        public List<CellOutput> outputs = new();

        public static Cell Parse(JToken rawCell)
        {
            var cell = new Cell();
            cell.cellType = rawCell["cell_type"]?.Value<string>() switch
            {
                "code" => CellType.Code,
                "markdown" => CellType.Markdown,
                "raw" => CellType.Raw,
                _ => CellType.Code
            };
            cell.metadata = CellMetadata.Parse(rawCell["metadata"]);
            cell.source = rawCell["source"].Type == JTokenType.String
                ? new List<string> {rawCell["source"].Value<string>()}
                : rawCell["source"].ToObject<List<string>>();
            if (cell.cellType == CellType.Code)
            {
                foreach (var rawOutput in rawCell["outputs"].ToObject<JArray>())
                {
                    var output = CellOutput.Parse(rawOutput);
                    cell.outputs.Add(output);
                }
            }
            return cell;
        }
    }
    
    public enum AutoScroll { True, False, Auto }
    
    [Serializable]
    public class CellMetadata
    {
        public bool collapsed;
        public AutoScroll autoscroll;
        public bool deletable;
        // TODO support Raw NBConvert Cell
        public string format; // The mime-type of a Raw NBConvert Cell
        public string name;
        public List<string> tags;

        public static CellMetadata Parse(JToken metadata) =>
            new()
            {
                collapsed = metadata?["collapsed"]?.Value<bool>() ?? false,
                autoscroll = metadata?["autoscroll"]?.Type == JTokenType.Boolean
                    ? metadata["autoscroll"].Value<bool>() ? AutoScroll.True : AutoScroll.False
                    : AutoScroll.Auto,
                deletable = metadata?["deletable"]?.Value<bool>() ?? true,
                format = metadata?["format"]?.Value<string>(),
                name = metadata?["name"]?.Value<string>(),
                tags = metadata?["tags"]?.ToObject<List<string>>()
            };
    }
    
    public enum OutputType { Stream, DisplayData, ExecuteResult, Error }
    
    [Serializable]
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

        public static CellOutput Parse(JToken rawOutput)
        {
            var output = new CellOutput();
            output.outputType = rawOutput["output_type"]?.Value<string>() switch
            {
                "execute_result" => OutputType.ExecuteResult,
                "display_data" => OutputType.DisplayData,
                "stream" => OutputType.Stream,
                "error" => OutputType.Error
            };
            switch (output.outputType)
            {
                case OutputType.Stream:
                    output.name = rawOutput["name"]?.Value<string>();
                    output.text = rawOutput["text"]?.ToObject<List<string>>();
                    break;
                case OutputType.ExecuteResult:
                    output.executionCount = rawOutput["execution_count"].Value<int>();
                    break;
                case OutputType.Error:
                    output.ename = rawOutput["ename"]?.Value<string>();
                    output.evalue = rawOutput["evalue"]?.Value<string>();
                    output.traceback = rawOutput["traceback"]?.ToObject<List<string>>();
                    break;
            }
            if (output.outputType is OutputType.DisplayData or OutputType.ExecuteResult)
            {
                foreach (var rawOutputData in rawOutput["data"].ToObject<JObject>())
                {
                    var entry = CellOutputDataEntry.Parse(rawOutputData);
                    output.data.Add(entry);
                }
                foreach (var rawOutputMetadata in rawOutput["metadata"].ToObject<JObject>())
                {
                    var entry = CellOutputMetadataEntry.Parse(rawOutputMetadata);
                    output.metadata.Add(entry);
                }
            }
            return output;
        }
    }
    
    [Serializable]
    public class CellOutputDataEntry
    {
        public string mimeType;
        public List<string> stringData = new();
        public Texture2D imageData;

        public static CellOutputDataEntry Parse(KeyValuePair<string, JToken> rawOutputData)
        {
            var entry = new CellOutputDataEntry();
            entry.mimeType = rawOutputData.Key;
            switch (entry.mimeType)
            {
                case "text/plain":
                case "text/html":
                case "text/markdown":
                    entry.stringData = rawOutputData.Value.ToObject<List<string>>();
                    break;
                case "image/png":
                case "image/jpeg":
                {
                    var b64 =  new StringBuilder();
                    foreach (var line in rawOutputData.Value.ToObject<List<string>>())
                    {
                        b64.Append(line);
                    }
                    var bytes = Convert.FromBase64String(b64.ToString());
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(bytes);
                    entry.imageData = tex;
                    break;
                }
                case "application/json":
                    entry.stringData.Add(rawOutputData.Value.ToString());
                    break;
                default:
                    Debug.LogError($"Unsupported output MIME type '{entry.mimeType}'");
                    break;
            }
            return entry;
        }
    }
    
    [Serializable]
    public class CellOutputMetadataEntry
    {
        // TODO add additional fields
        public string mimeType;

        // TODO parse output metadata
        public static CellOutputMetadataEntry Parse(KeyValuePair<string, JToken> rawOutputMetadata)
        {
            var entry = new CellOutputMetadataEntry();
            // entry.mimeType = rawOutputMetadata["metadata"]?.Value<string>();
            return entry;
        }
    }
}

