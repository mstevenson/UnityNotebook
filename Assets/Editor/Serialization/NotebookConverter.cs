using System;
using System.Collections.Generic;
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
            nb.format = obj["nbformat"]?.Value<int>() ?? 4;
            nb.formatMinor = obj["nbformat_minor"]?.Value<int>() ?? 2;
            nb.metadata = obj["metadata"]?.ToObject<Notebook.Metadata>() ?? new Notebook.Metadata();
            nb.cells = obj["cells"]?.ToObject<List<Notebook.Cell>>() ?? new List<Notebook.Cell>();
            return nb;
        }
    }
}