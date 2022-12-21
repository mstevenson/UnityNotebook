using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static UnityNotebook.Notebook.OutputType;

namespace UnityNotebook
{
    public class CellOutputConverter : JsonConverter<Notebook.CellOutput>
    {
        public override void WriteJson(JsonWriter writer, Notebook.CellOutput value, JsonSerializer serializer)
        {
            var output = new JObject();
            // set the string value for the enum as specified in JsonProperty attribute
            output["output_type"] = JToken.FromObject(value.outputType);
            switch (value.outputType)
            {
                case Stream:
                    output["name"] = value.name;
                    output["text"] = JArray.FromObject(value.text);
                    break;
                case ExecuteResult:
                    output["execution_count"] = value.executionCount;
                    break;
                case Error:
                    output["ename"] = value.ename;
                    output["evalue"] = value.evalue;
                    output["traceback"] = JArray.FromObject(value.traceback);
                    break;
            }

            if (value.outputType is DisplayData or ExecuteResult)
            {
                var data = new JObject();
                foreach (var entry in value.data)
                {
                    // TODO
                    // data[entry.mimeType] = entry.data;
                }

                output["data"] = data;
                var metadata = new JObject();
                foreach (var entry in value.metadata)
                {
                    // TODO
                    // metadata[entry.mimeType] = entry.meta.metadata;
                }

                output["metadata"] = metadata;
            }

            output.WriteTo(writer);
        }

        public override Notebook.CellOutput ReadJson(JsonReader reader, Type objectType,
            Notebook.CellOutput existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Notebook.CellOutput();
            }

            var output = hasExistingValue ? existingValue : new Notebook.CellOutput();

            output.outputType = obj["output_type"]?.ToObject<Notebook.OutputType>() ?? DisplayData;
            switch (output.outputType)
            {
                case Stream:
                    output.name = obj["name"]?.Value<string>();
                    output.text = obj["text"]?.ToObject<List<string>>();
                    break;
                case ExecuteResult:
                    output.executionCount = obj["execution_count"]?.Value<int>() ?? 0;
                    break;
                case Error:
                    output.ename = obj["ename"]?.Value<string>() ?? string.Empty;
                    output.evalue = obj["evalue"]?.Value<string>() ?? string.Empty;
                    output.traceback = obj["traceback"]?.ToObject<List<string>>() ?? new List<string>();
                    break;
            }

            if (output.outputType is DisplayData or ExecuteResult)
            {
                // foreach (var (key, value) in obj["data"]?.ToObject<JObject>())
                // {
                //     // TODO
                //     // var entry = Notebook.CellOutputDataEntry.Parse(kvp);
                //     // output.data.Add(entry);
                // }
                // foreach (var rawOutputMetadata in obj["metadata"]?.ToObject<JObject>())
                // {
                //     // TODO
                //     // var entry = Notebook.CellOutputMetadataEntry.Parse(rawOutputMetadata);
                //     // output.metadata.Add(entry);
                // }
            }

            return output;
        }
    }
}