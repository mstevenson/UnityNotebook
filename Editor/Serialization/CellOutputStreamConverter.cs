using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    public class CellOutputStreamConverter : JsonConverter<Notebook.CellOutputStream>
    {
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputStream value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
                ["name"] = value.name,
                ["text"] = JArray.FromObject(value.text)
            };
            output.WriteTo(writer);
        }

        public override Notebook.CellOutputStream ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputStream existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputStream();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputStream();
            output.outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            output.name = obj["name"]?.Value<string>();
            output.text = obj["text"]?.ToObject<List<string>>();
            return output;
        }
    }
}