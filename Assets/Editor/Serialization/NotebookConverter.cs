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
                ["cells"] = JArray.FromObject(value.cells)
            };
            nb.WriteTo(writer);
        }

        public override Notebook ReadJson(JsonReader reader, Type objectType, Notebook existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return ScriptableObject.CreateInstance<Notebook>();
            }
            var nb = hasExistingValue ? existingValue : ScriptableObject.CreateInstance<Notebook>();;
            
            nb.format = (obj["nbformat"] ?? 4).Value<int>();
            nb.formatMinor = (obj["nbformat_minor"] ?? 2).Value<int>();
            // TODO don't require all fields
            if (obj["metadata"] != null) nb.metadata = obj["metadata"].ToObject<Notebook.Metadata>();
            var cells = obj["cells"];
            if (cells != null)
            {
                nb.cells.AddRange(cells.ToObject<Notebook.Cell[]>());
            }
            
            return nb;
        }
    }
}