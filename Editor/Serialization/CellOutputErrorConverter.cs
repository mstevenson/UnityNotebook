using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    public class CellOutputErrorConverter : JsonConverter<Notebook.CellOutputError>
    {
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputError value, JsonSerializer serializer)
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
        
        public override Notebook.CellOutputError ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputError existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputError();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputError();
            output.outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            output.ename = obj["ename"]?.Value<string>() ?? string.Empty;
            output.evalue = obj["evalue"]?.Value<string>() ?? string.Empty;
            output.traceback = obj["traceback"]?.ToObject<List<string>>() ?? new List<string>();
            return output;
        }
    }
}