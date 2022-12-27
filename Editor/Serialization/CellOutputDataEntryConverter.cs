using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityNotebook
{
    
    // TODO this isn't being called during deserialization because we're manually converting 
    // a <Dictionary<string, List<string>>> to Notebook.CellOutputDataEntry in CellOutputDisplayDataConverter
    
    public class CellOutputDataEntryConverter : JsonConverter<Notebook.CellOutputDataEntry>
    {
        public override Notebook.CellOutputDataEntry ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputDataEntry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Debug.Log("import");
            
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputDataEntry();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputDataEntry();
            output.mimeType = obj["mime_type"].Value<string>();

            switch (output.mimeType)
            {
                case "text/plain":
                    var list = obj["data"].ToObject<List<string>>();
                    output.backingValue = new ValueWrapper(string.Concat(list));
                    Debug.Log(output.backingValue.Object);
                    break;
                case "image/png":
                {
                    var b64 =  new StringBuilder();
                    var lines = obj["data"].ToObject<List<string>>();
                    foreach (var line in lines)
                    {
                        b64.Append(line);
                    }
                    var bytes = Convert.FromBase64String(b64.ToString());
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(bytes);
                    output.backingValue = new ValueWrapper(tex);
                    break;
                }
                case "application/vnd.unity3d.animationcurve":
                {
                    // TODO read curve
                    break;
                }
                case "application/vnd.unity3d.color":
                {
                    // TODO read color
                    break;
                }
                default:
                    throw new NotImplementedException($"Unsupported output data MIME type '{output.mimeType}'");
                    break;
            }
            
            return output;
        }
        
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputDataEntry value, JsonSerializer serializer)
        {
            var output = new JObject();
            var obj = value.backingValue.Object;
            
            if (obj is Texture2D tex)
            {
                var bytes = tex.EncodeToPNG();
                var b64 = Convert.ToBase64String(bytes);
                var lines = new[] { b64 };
                output["mime_type"] = "image/png";
                output["data"] = JArray.FromObject(lines);
            }
            else if (obj is AnimationCurve curve)
            {
                output["mime_type"] = "application/vnd.unity3d.animationcurve";
                // TODO write curve
            }
            else if (obj is Color color)
            {
                output["mime_type"] = "application/vnd.unity3d.color";
                // TODO write color
            }
            else if (obj is string str)
            {
                output["mime_type"] = "text/plain";
                var list = str.Split('\n');
                output["data"] = JArray.FromObject(list);
            }
            else
            {
                throw new NotSupportedException();
            }
            
            output.WriteTo(writer);
        }
    }
}