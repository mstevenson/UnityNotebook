using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityNotebook
{
    [Serializable]
    [JsonConverter(typeof(CellOutputExecuteResultsConverter))]
    public class CellOutputExecuteResults : CellOutput
    {
        public CellOutputExecuteResults() => outputType = OutputType.ExecuteResult;
        public int executionCount;
            
        [JsonIgnore]
        public ValueWrapper backingValue;
    }
    
    public class CellOutputExecuteResultsConverter : JsonConverter<CellOutputExecuteResults>
    {
        public override CellOutputExecuteResults ReadJson(JsonReader reader, Type objectType, CellOutputExecuteResults existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new CellOutputExecuteResults();
            }
            var output = hasExistingValue ? existingValue : new CellOutputExecuteResults();
            output.outputType = obj["output_type"].ToObject<OutputType>();
            output.executionCount = obj["execution_count"]?.Value<int>() ?? 0;
            return output;
        }

        public override void WriteJson(JsonWriter writer, CellOutputExecuteResults value, JsonSerializer serializer)
        {
            var output = new JObject
            {
                ["output_type"] = JToken.FromObject(value.outputType),
                ["execution_count"] = value.executionCount
            };
            output.WriteTo(writer);
        }
    }
}