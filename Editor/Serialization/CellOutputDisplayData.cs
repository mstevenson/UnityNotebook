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
                
                switch (mimeType)
                {
                    case "text/plain":
                        var list = obj["data"][mimeType].ToObject<List<string>>();
                        output.values.Add(new ValueWrapper(string.Concat(list)));
                        break;
                    case "image/png":
                    {
                        // TODO parse image data? Is it required to reconstruct the texture?
                        
                        var b64 =  new StringBuilder();
                        var lines = obj["data"].ToObject<List<string>>();
                        foreach (var line in lines)
                        {
                            b64.Append(line);
                        }
                        var bytes = Convert.FromBase64String(b64.ToString());
                        var tex = new Texture2D(2, 2);
                        tex.LoadImage(bytes);
                        output.values.Add(new ValueWrapper(tex));
                        break;
                    }
                    // Unity object type
                    default:
                    {
                        var type = UnityMimeTypes.GetType(mimeType);
                        var unityObj = obj["data"][mimeType].ToObject(type);
                        output.values.Add(new ValueWrapper(unityObj));
                        break;
                    }
                }
            }

            return output;
        }
        
        public override void WriteJson(JsonWriter writer, CellOutputDisplayData displayData, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(displayData.outputType),
            };
            
            var tempTextures = new List<Texture2D>();
            
            // Collect assets
            foreach (var value in displayData.values)
            {
                var jEntry = new JObject();
                
                // Texture
                if (value.Object is Texture2D tex)
                {
                    tempTextures.Add(tex);
                    var bytes = tex.EncodeToPNG();
                    var b64 = Convert.ToBase64String(bytes);
                    var lines = new[] { b64 };
                    jEntry["image/png"] = JArray.FromObject(lines);
                }
                // String
                else if (value.Object is string str)
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