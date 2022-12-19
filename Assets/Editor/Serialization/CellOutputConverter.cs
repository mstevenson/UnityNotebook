using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Notebook.OutputType;

public class CellOutputConverter: JsonConverter<Notebook.CellOutput>
{
    public override void WriteJson(JsonWriter writer, Notebook.CellOutput value, JsonSerializer serializer)
    {
        var output = new JObject();
        output["output_type"] = value.ToString().ToLower();
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

    public override Notebook.CellOutput ReadJson(JsonReader reader, Type objectType, Notebook.CellOutput existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JToken.Load(reader);
        if (!obj.HasValues)
        {
            return new Notebook.CellOutput();
        }
        var output = hasExistingValue ? existingValue : new Notebook.CellOutput();
        
        output.outputType = obj["output_type"]?.Value<string>() switch
        {
            "execute_result" => ExecuteResult,
            "display_data" => DisplayData,
            "stream" => Stream,
            "error" => Error,
            _ => throw new ArgumentOutOfRangeException()
        };
        switch (output.outputType)
        {
            case Stream:
                output.name = obj["name"]?.Value<string>();
                output.text = obj["text"]?.ToObject<List<string>>();
                break;
            case ExecuteResult:
                output.executionCount = (obj["execution_count"] ?? 0).Value<int>();
                break;
            case Error:
                output.ename = obj["ename"]?.Value<string>();
                output.evalue = obj["evalue"]?.Value<string>();
                output.traceback = obj["traceback"]?.ToObject<List<string>>();
                break;
        }
        if (output.outputType is DisplayData or ExecuteResult)
        {
            foreach (var (key, value) in obj["data"]?.ToObject<JObject>())
            {
                // TODO
                // var entry = Notebook.CellOutputDataEntry.Parse(kvp);
                // output.data.Add(entry);
            }
            foreach (var rawOutputMetadata in obj["metadata"]?.ToObject<JObject>())
            {
                // TODO
                // var entry = Notebook.CellOutputMetadataEntry.Parse(rawOutputMetadata);
                // output.metadata.Add(entry);
            }
        }
        return output;
    }
}