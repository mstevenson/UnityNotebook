using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static UnityNotebook.Notebook.OutputType;

namespace UnityNotebook
{
    public class CellOutputConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<Notebook.CellOutput>);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = JArray.Load(reader);
            var output = new List<Notebook.CellOutput>();
            if (!list.HasValues)
            {
                return output;
            }

            foreach (var elem in list)
            {
                var item = new Notebook.CellOutput();
                var outputType = elem["output_type"].ToObject<Notebook.OutputType>();
                item = outputType switch
                {
                    Stream => elem.ToObject<Notebook.CellOutputStream>(serializer),
                    ExecuteResult => elem.ToObject<Notebook.CellOutputExecuteResults>(serializer),
                    Error => elem.ToObject<Notebook.CellOutputError>(serializer),
                    DisplayData => elem.ToObject<Notebook.CellOutputDisplayData>(serializer),
                    _ => item
                };
                output.Add(item);
            }
            
            return output;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not List<Notebook.CellOutput> list || list.Count == 0)
            {
                var arr = new JArray();
                arr.WriteTo(writer);
                return;
            }
            
            writer.WriteStartArray();
            foreach (var elem in list)
            {
                var obj = JObject.FromObject(elem);
                obj.WriteTo(writer);
            }
            writer.WriteEndArray();
        }
    }
}