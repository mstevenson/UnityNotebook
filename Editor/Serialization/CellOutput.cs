using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static UnityNotebook.OutputType;

namespace UnityNotebook
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutputType
    {
        [EnumMember(Value = "stream")]
        Stream,
        [EnumMember(Value = "display_data")]
        DisplayData,
        [EnumMember(Value = "execute_result")]
        ExecuteResult,
        [EnumMember(Value = "error")]
        Error
    }
    
    [Serializable]
    [JsonConverter(typeof(CellOutputConverter))]
    public class CellOutput
    {
        public OutputType outputType;
        [NonSerialized] public Vector2 scroll;
    }
    
    public class CellOutputConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<CellOutput>);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = JArray.Load(reader);
            var output = new List<CellOutput>();
            if (!list.HasValues)
            {
                return output;
            }

            foreach (var elem in list)
            {
                var item = new CellOutput();
                var outputType = elem["output_type"].ToObject<OutputType>();
                item = outputType switch
                {
                    Stream => elem.ToObject<CellOutputStream>(serializer),
                    ExecuteResult => elem.ToObject<CellOutputExecuteResults>(serializer),
                    Error => elem.ToObject<CellOutputError>(serializer),
                    DisplayData => elem.ToObject<CellOutputDisplayData>(serializer),
                    _ => item
                };
                output.Add(item);
            }
            
            return output;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is not List<CellOutput> list || list.Count == 0)
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