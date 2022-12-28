using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    public class CellOutputStreamConverter : JsonConverter<CellOutputStream>
    {
        public override CellOutputStream ReadJson(JsonReader reader, Type objectType, CellOutputStream existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new CellOutputStream();
            }
            var output = hasExistingValue ? existingValue : new CellOutputStream();
            output.outputType = obj["output_type"].ToObject<OutputType>();
            output.name = obj["name"]?.Value<string>();
            output.text = obj["text"]?.ToObject<List<string>>();
            return output;
        }

        public override void WriteJson(JsonWriter writer, CellOutputStream value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
                ["name"] = value.name,
                ["text"] = JArray.FromObject(value.text)
            };
            output.WriteTo(writer);
        }
    }
}