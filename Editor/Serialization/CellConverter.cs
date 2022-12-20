using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Notebook.CellType;

public class CellConverter : JsonConverter<Notebook.Cell>
{
    public override void WriteJson(JsonWriter writer, Notebook.Cell value, JsonSerializer serializer)
    {
        var cell = new JObject
        {
            ["cell_type"] = value.cellType.ToString().ToLower(),
            ["metadata"] = JObject.FromObject(value.metadata),
            ["source"] = JArray.FromObject(value.source)
        };
        if (value.cellType == Code)
        {
            cell["execution_count"] = value.executionCount;
            cell["outputs"] = JArray.FromObject(value.outputs);
        }
        cell.WriteTo(writer);
    }

    public override Notebook.Cell ReadJson(JsonReader reader, Type objectType, Notebook.Cell existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JToken.Load(reader);
        if (!obj.HasValues)
        {
            return new Notebook.Cell();
        }
        var cell = hasExistingValue ? existingValue : new Notebook.Cell();

        cell.cellType = obj["cell_type"]?.Value<string>() switch
        {
            "code" => Code,
            "markdown" => Markdown,
            "raw" => Raw,
            _ => Code
        };
        cell.metadata = obj["metadata"]?.ToObject<Notebook.CellMetadata>() ?? new Notebook.CellMetadata();
        cell.source = obj["source"]?.ToObject<string[]>() ?? Array.Empty<string>();
        if (cell.cellType == Code)
        {
            cell.outputs = obj["outputs"]?.ToObject<List<Notebook.CellOutput>>() ?? new List<Notebook.CellOutput>();
        }
        return cell;
    }
}
