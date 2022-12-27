using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    public class CellOutputExecuteResultsConverter : JsonConverter<Notebook.CellOutputExecuteResults>
    {
        public override void WriteJson(JsonWriter writer, Notebook.CellOutputExecuteResults value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
                ["execution_count"] = value.executionCount
            };
            output.WriteTo(writer);
        }

        public override Notebook.CellOutputExecuteResults ReadJson(JsonReader reader, Type objectType, Notebook.CellOutputExecuteResults existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutputExecuteResults();
            }
            var output = hasExistingValue ? existingValue : new Notebook.CellOutputExecuteResults();
            output.outputType = obj["output_type"].ToObject<Notebook.OutputType>();
            output.executionCount = obj["execution_count"]?.Value<int>() ?? 0;
            return output;
        }
    }
}