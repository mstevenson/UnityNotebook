using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    public class CellOutputErrorConverter : JsonConverter<CellOutputError>
    {
        public override CellOutputError ReadJson(JsonReader reader, Type objectType, CellOutputError existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new CellOutputError();
            }
            var output = hasExistingValue ? existingValue : new CellOutputError();
            output.outputType = obj["output_type"].ToObject<OutputType>();
            output.ename = obj["ename"]?.Value<string>() ?? string.Empty;
            output.evalue = obj["evalue"]?.Value<string>() ?? string.Empty;
            output.traceback = obj["traceback"]?.ToObject<List<string>>() ?? new List<string>();
            return output;
        }

        public override void WriteJson(JsonWriter writer, CellOutputError value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
                ["ename"] = value.ename,
                ["evalue"] = value.evalue,
                ["traceback"] = JArray.FromObject(value.traceback)
            };
            output.WriteTo(writer);
        }
    }
}