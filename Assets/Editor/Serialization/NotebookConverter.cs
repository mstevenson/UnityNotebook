using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Editor.Serialization
{
    public class NotebookConverter : JsonConverter<Notebook>
    {
        public override void WriteJson(JsonWriter writer, Notebook value, JsonSerializer serializer)
        {
            var nb = new JObject
            {
                ["nbformat"] = value.format,
                ["nbformat_minor"] = value.formatMinor,
                ["metadata"] = value.metadata != null ? JObject.FromObject(value.metadata) : null,
                ["cells"] = new JArray(value.cells.Select(JsonConvert.SerializeObject))
            };
            nb.WriteTo(writer);
        }

        public override Notebook ReadJson(JsonReader reader, Type objectType, Notebook existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var nb = hasExistingValue ? existingValue : ScriptableObject.CreateInstance<Notebook>();;
            
            nb.format = (obj["nbformat"] ?? 4).Value<int>();
            nb.formatMinor = (obj["nbformat_minor"] ?? 0).Value<int>();
            
            // TODO parse file metadata
            // metadata = nb["metadata"] != null ? nb["metadata"].ToObject<Metadata>() : new Metadata();
            
            foreach (var c in obj["cells"]?.ToObject<JArray>())
            {
                var cell = c.ToObject<Notebook.Cell>();
                nb.cells.Add(cell);
            }
            
            return nb;
        }
    }
}