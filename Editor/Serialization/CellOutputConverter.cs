using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            var obj = JToken.Load(reader);
            var output = new Notebook.CellOutput();
            
            var outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            switch (outputType)
            {
                case Stream:
                    output = obj.ToObject<Notebook.CellOutputStream>(serializer);
                    break;
                case ExecuteResult:
                    output = obj.ToObject<Notebook.CellOutputExecuteResults>(serializer);
                    break;
                case Error:
                    output = obj.ToObject<Notebook.CellOutputError>(serializer);
                    break;
                case DisplayData:
                    output = obj.ToObject<Notebook.CellOutputDisplayData>(serializer);
                    break;
            }
            
            return output;

            return new List<Notebook.CellOutput>();
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var output = (IList) value;
            // var item = new JObject();
            // foreach (var VARIABLE in value)
            // {
            //     
            // }
            // item["output_type"] = output.outputType switch
            // {
            //     Stream => JToken.FromObject(Stream),
            //     ExecuteResult => JToken.FromObject(ExecuteResult),
            //     Error => JToken.FromObject(Error),
            //     DisplayData => JToken.FromObject(DisplayData),
            //     _ => item["output_type"]
            // };
            // item.WriteTo(writer);
        }
    }
}