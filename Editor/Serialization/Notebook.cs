using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace UnityNotebook
{
    // Parses Jupyter Notebook asset files and imports them into Unity's AssetDatabase
    [ScriptedImporter(1, "ipynb")]
    public class NotebookImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var json = System.IO.File.ReadAllText(ctx.assetPath);
            var notebook = JsonConvert.DeserializeObject<Notebook>(json);
            ctx.AddObjectToAsset("main", notebook);
            ctx.SetMainObject(notebook);
        }
    }
    
    // Implementation of the Jupyter Notebook file format
    // https://nbformat.readthedocs.io/en/latest/format_description.html
    // https://github.com/jupyter/nbformat/blob/main/nbformat/v4/nbformat.v4.schema.json
    [JsonConverter(typeof(NotebookConverter))]
    public class Notebook : ScriptableObject
    {
        public int format = 4;
        public int formatMinor = 2;
        public List<Cell> cells = new();
    }
    
    public class NotebookConverter : JsonConverter<Notebook>
    {
        public override Notebook ReadJson(JsonReader reader, System.Type objectType, Notebook existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return ScriptableObject.CreateInstance<Notebook>();
            }
            var nb = hasExistingValue ? existingValue : ScriptableObject.CreateInstance<Notebook>();
            nb.format = obj["nbformat"]?.Value<int>() ?? 4;
            nb.formatMinor = obj["nbformat_minor"]?.Value<int>() ?? 2;
            var cellsList = obj["cells"];
            if (cellsList is {HasValues: true})
            {
                nb.cells = cellsList.ToObject<List<Cell>>();
            }
            return nb;
        }

        public override void WriteJson(JsonWriter writer, Notebook value, JsonSerializer serializer)
        {
            var nb = new JObject
            {
                ["nbformat"] = value.format,
                ["nbformat_minor"] = value.formatMinor,
                ["metadata"] = new JObject
                {
                    ["kernelspec"] = new JObject
                    {
                        ["display_name"] = ".Net (C#)",
                        ["language"] = "C#",
                        ["name"] = ".net-csharp"
                    }
                },
                ["cells"] = JArray.FromObject(value.cells)
            };
            nb.WriteTo(writer);
        }
    }
}
