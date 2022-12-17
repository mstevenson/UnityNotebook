using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Editor.Serialization
{
    public class CellOutputDataEntryConverter : JsonConverter<Notebook.CellOutputDataEntry>
    {
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputDataEntry value, JsonSerializer serializer)
        {
            
        }

        public override Notebook.CellOutputDataEntry ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputDataEntry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputDataEntry();
            }
            
            // var str = reader.ReadAsString();
            //
            // // TODO parse as a KeyValuePair<string, JToken>
            //
            // var obj = JObject.Parse(str);
            //
            // var entry = hasExistingValue ? existingValue : new Notebook.CellOutputDataEntry();
            // entry.mimeType = obj.Key;
            // switch (entry.mimeType)
            // {
            //     case "text/plain":
            //     case "text/html":
            //     case "text/markdown":
            //         entry.stringData = obj.Value.ToObject<List<string>>();
            //         break;
            //     case "image/png":
            //     case "image/jpeg":
            //     {
            //         var b64 =  new StringBuilder();
            //         foreach (var line in obj.Value.ToObject<List<string>>())
            //         {
            //             b64.Append(line);
            //         }
            //         var bytes = Convert.FromBase64String(b64.ToString());
            //         var tex = new Texture2D(2, 2);
            //         tex.LoadImage(bytes);
            //         entry.imageData = tex;
            //         break;
            //     }
            //     case "application/json":
            //         entry.stringData.Add(obj.Value.ToString());
            //         break;
            //     default:
            //         Debug.LogError($"Unsupported output MIME type '{entry.mimeType}'");
            //         break;
            // }
            // return entry;

            return new Notebook.CellOutputDataEntry();
        }
    }
}