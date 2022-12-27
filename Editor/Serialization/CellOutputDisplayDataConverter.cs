using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityNotebook
{
    public class CellOutputDisplayDataConverter : JsonConverter<Notebook.CellOutputDisplayData>
    {
        public override Notebook.CellOutputDisplayData ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputDisplayData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputDisplayData();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputDisplayData();
            output.outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            
            // Rebuild json dictionary as a list of objects so CellOutputDataEntry will have its custom JsonConverter called.
            foreach (var jToken in obj["data"])
            {
                var item = (JProperty) jToken;
                var rebuilt = new JObject
                {
                    ["mime_type"] = item.Name,
                    ["data"] = item.Value
                };
                var entry = rebuilt.ToObject<Notebook.CellOutputDataEntry>();
                output.data.Add(entry);
            }
            
            // TODO parse metadata from raw fields (see WriteJson below)
            
            return output;
        }
        
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputDisplayData value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
            };
            
            var tempTextures = new List<Texture2D>();
            
            // Collect assets
            foreach (var entry in value.data)
            {
                var obj = entry.backingValue.Object;
                if (obj is Texture2D tex)
                {
                    tempTextures.Add(tex);
                }
                var dataArray = new[] {obj};
                output["data"] = new JObject
                {
                    [entry.mimeType] = JToken.FromObject(dataArray)
                };
            }
            
            // Collect metadata
            // value.metadata.Clear();
            foreach (var tex in tempTextures)
            {
                var meta = new JObject
                {
                    ["mime_type"] = "image/png",
                    ["width"] = tex.width,
                    ["height"] = tex.height
                };

                output["metadata"] = meta;
            }
            
            output.WriteTo(writer);
        }
    }
}