using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Notebook.CellType;

public class CellConverter : JsonConverter<Notebook.Cell>
{
    public override void WriteJson(JsonWriter writer, Notebook.Cell value, JsonSerializer serializer)
    {
        // var cell = new JObject
        // {
        //     ["cell_type"] = value.cellType.ToString().ToLower(),
        //     ["metadata"] = JsonConvert.SerializeObject(value.metadata),
        //     ["source"] = JArray.FromObject(value.source)
        // };
        // if (value.cellType == Notebook.CellType.Code)
        // {
        //     var outputs = new JArray();
        //     foreach (var output in value.outputs)
        //     {
        //         outputs.Add(JsonConvert.SerializeObject(output));
        //     }
        //     cell["outputs"] = outputs;
        // }
        // cell.WriteTo(writer);
    }

    public override Notebook.Cell ReadJson(JsonReader reader, Type objectType, Notebook.Cell existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        
        var cell = hasExistingValue ? existingValue : new Notebook.Cell();
        cell.cellType = obj["cell_type"]?.Value<string>() switch
        {
            "code" => Code,
            "markdown" => Markdown,
            "raw" => Raw,
            _ => Code
        };
        cell.metadata = obj["metadata"].ToObject<Notebook.CellMetadata>();
        cell.source = obj["source"].ToObject<string[]>();
        if (cell.cellType == Code)
        {
            cell.outputs.AddRange(obj["outputs"].ToObject<List<Notebook.CellOutput>>());
        }
        return cell;
    }
}
