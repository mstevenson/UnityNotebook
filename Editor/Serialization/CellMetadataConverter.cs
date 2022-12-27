using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnityNotebook
{
    public class CellMetadataConverter : JsonConverter<List<Notebook.CellMetadataEntry>>
    {
        public override List<Notebook.CellMetadataEntry> ReadJson(JsonReader reader, Type objectType, List<Notebook.CellMetadataEntry> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var meta = new List<Notebook.CellMetadataEntry>();
            var dict = serializer.Deserialize<Dictionary<string, string>>(reader);
            foreach (var kvp in dict)
            {
                meta.Add(new Notebook.CellMetadataEntry { key = kvp.Key, value = kvp.Value });
            }
            return meta;
        }
        
        public override void WriteJson(JsonWriter writer, List<Notebook.CellMetadataEntry> value, JsonSerializer serializer)
        {
            var dict = new Dictionary<string, string>();
            foreach (var entry in value)
            {
                dict.Add(entry.key, entry.value);
            }
            serializer.Serialize(writer, dict);
        }
    }
}