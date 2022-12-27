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
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputDisplayData();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputDisplayData();
            output.outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            
            var dict = obj["data"].ToObject<Dictionary<string, List<string>>>();
            foreach (var (mimeType, dataList) in dict)
            {
                var entry = new Notebook.CellOutputDataEntry
                {
                    mimeType = mimeType,
                    data = dataList
                };
                output.data.Add(entry);
            }
            
            // TODO parse metadata from raw fields (see WriteJson below), use to construct Texture2Ds?
            
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
                var data = entry.GetData();
                if (data is Texture2D tex)
                {
                    tempTextures.Add(tex);
                }
                var dataArray = new[] {data};
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