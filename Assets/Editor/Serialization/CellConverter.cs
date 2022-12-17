using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static Notebook.CellType;

public class CellConverter : JsonConverter<Notebook.Cell>
{
    public override void WriteJson(JsonWriter writer, Notebook.Cell value, JsonSerializer serializer)
    {
        var cell = new JObject
        {
            ["cell_type"] = value.cellType.ToString().ToLower(),
            // TODO metadata
            // ["metadata"] = JsonConvert.SerializeObject(value.metadata),
            ["source"] = JArray.FromObject(value.source)
        };
        if (value.cellType == Code)
        {
            var outputs = new JArray();
            foreach (var output in value.outputs)
            {
                outputs.Add(JsonConvert.SerializeObject(output));
            }
            cell["outputs"] = outputs;
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
        
        // TODO metadata
        // cell.metadata = obj["metadata"]?.ToObject<Notebook.CellMetadata>();
        cell.source = obj["source"]?.ToObject<string[]>();
        if (cell.cellType == Code)
        {
            var outputs = obj["outputs"];
            if (outputs != null)
            {
                cell.outputs.AddRange(outputs.ToObject<List<Notebook.CellOutput>>());
            }
        }
        return cell;
    }
}
