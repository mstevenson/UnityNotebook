using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityNotebook
{
    [Serializable]
    [JsonConverter(typeof(CellOutputDisplayDataConverter))]
    public class CellOutputDisplayData : CellOutput
    {
        public CellOutputDisplayData() => outputType = OutputType.DisplayData;

        [JsonIgnore]
        public List<ValueWrapper> values = new(); // mime-type -> data, often text/plain, image/png, application/json

        // public List<CellOutputMetadataEntry> metadata = new(); // mime-type -> metadata
    }
    
    public class CellOutputDisplayDataConverter : JsonConverter<CellOutputDisplayData>
    {
        public override CellOutputDisplayData ReadJson(JsonReader reader, Type objectType, CellOutputDisplayData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (!obj.HasValues)
            {
                return new CellOutputDisplayData();
            }
            CellOutputDisplayData output = hasExistingValue ? existingValue : new CellOutputDisplayData();
            output.outputType = obj["output_type"].ToObject<OutputType>();
            
            foreach (var jToken in obj["data"])
            {
                var item = (JProperty) jToken;
                var mimeType = item.Name;
                var value = item.Value;

                object result;
                switch (mimeType)
                {
                    // Strings
                    case "text/plain":
                    {
                        var list = obj["data"][mimeType].ToObject<List<string>>();
                        result = string.Concat(list);
                        break;
                    }
                    // Unity Object json data
                    case var _ when UnityMimeTypes.GetType(mimeType) != null:
                    {
                        var type = UnityMimeTypes.GetType(mimeType);
                        var json = obj["data"][mimeType].ToString();
                        result = JsonConvert.DeserializeObject(json, type);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException($"Mime type {mimeType} is not supported");
                    }
                }
                output.values.Add(new ValueWrapper(result));
            }

            return output;
        }
        
        public override void WriteJson(JsonWriter writer, CellOutputDisplayData displayData, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(displayData.outputType),
            };
            
            // Collect assets
            foreach (var value in displayData.values)
            {
                var jEntry = new JObject();
                
                // String
                if (value.Object is string str)
                {
                    var list = str.Split('\n');
                    jEntry["text/plain"] = JArray.FromObject(list);
                }
                // Unity type
                else
                {
                    var mimeType = UnityMimeTypes.GetMimeType(value.Object.GetType());
                    jEntry[mimeType] = JObject.FromObject(value.Object);
                }
                
                output["data"] = jEntry;
            }
            
            // Collect metadata
            // value.metadata.Clear();
            // foreach (var tex in tempTextures)
            // {
            //     var meta = new JObject
            //     {
            //         ["mime_type"] = "image/png",
            //         ["width"] = tex.width,
            //         ["height"] = tex.height
            //     };
            //
            //     output["metadata"] = meta;
            // }
            
            output.WriteTo(writer);
        }
    }
}