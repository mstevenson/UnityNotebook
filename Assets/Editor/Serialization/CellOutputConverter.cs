using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CellOutputConverter: JsonConverter<Notebook.CellOutput>
{
    public override void WriteJson(JsonWriter writer, Notebook.CellOutput value, JsonSerializer serializer)
    {
        // var output = new JObject();
        // output["output_type"] = value.ToString().ToLower();
        // switch (value.outputType)
        // {
        //     case Notebook.OutputType.Stream:
        //         output["name"] = value.name;
        //         output["text"] = JArray.FromObject(value.text);
        //         break;
        //     case Notebook.OutputType.ExecuteResult:
        //         output["execution_count"] = value.executionCount;
        //         break;
        //     case Notebook.OutputType.Error:
        //         output["ename"] = value.ename;
        //         output["evalue"] = value.evalue;
        //         output["traceback"] = JArray.FromObject(value.traceback);
        //         break;
        // }
        // if (value.outputType is Notebook.OutputType.DisplayData or Notebook.OutputType.ExecuteResult)
        // {
        //     var data = new JObject();
        //     foreach (var entry in value.data)
        //     {
        //         // TODO
        //         // data[entry.mimeType] = entry.data;
        //     }
        //     output["data"] = data;
        //     var metadata = new JObject();
        //     foreach (var entry in value.metadata)
        //     {
        //         // TODO
        //         // metadata[entry.mimeType] = entry.meta.metadata;
        //     }
        //     output["metadata"] = metadata;
        // }
        // output.WriteTo(writer);
    }

    public override Notebook.CellOutput ReadJson(JsonReader reader, Type objectType, Notebook.CellOutput existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        
        var output = new Notebook.CellOutput();
        output.outputType = obj["output_type"]?.Value<string>() switch
        {
            "execute_result" => Notebook.OutputType.ExecuteResult,
            "display_data" => Notebook.OutputType.DisplayData,
            "stream" => Notebook.OutputType.Stream,
            "error" => Notebook.OutputType.Error
        };
        switch (output.outputType)
        {
            case Notebook.OutputType.Stream:
                output.name = obj["name"]?.Value<string>();
                output.text = obj["text"]?.ToObject<List<string>>();
                break;
            case Notebook.OutputType.ExecuteResult:
                output.executionCount = obj["execution_count"].Value<int>();
                break;
            case Notebook.OutputType.Error:
                output.ename = obj["ename"]?.Value<string>();
                output.evalue = obj["evalue"]?.Value<string>();
                output.traceback = obj["traceback"]?.ToObject<List<string>>();
                break;
        }
        if (output.outputType is Notebook.OutputType.DisplayData or Notebook.OutputType.ExecuteResult)
        {
            foreach (var (key, value) in obj["data"].ToObject<JObject>())
            {
                // TODO
                // var entry = Notebook.CellOutputDataEntry.Parse(kvp);
                // output.data.Add(entry);
            }
            foreach (var rawOutputMetadata in obj["metadata"].ToObject<JObject>())
            {
                // TODO
                // var entry = Notebook.CellOutputMetadataEntry.Parse(rawOutputMetadata);
                // output.metadata.Add(entry);
            }
        }
        return output;
    }
}