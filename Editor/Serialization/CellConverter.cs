using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static UnityNotebook.Notebook.CellType;

namespace UnityNotebook
{
    public class CellConverter : JsonConverter<Notebook.Cell>
    {
        public override void WriteJson(JsonWriter writer, Notebook.Cell value, JsonSerializer serializer)
        {
            var cell = new JObject
            {
                ["cell_type"] = JToken.FromObject(value.cellType),
                // TODO metadata
                ["metadata"] = new JObject(),
                ["source"] = JArray.FromObject(value.source)
            };
            if (value.cellType == Code)
            {
                cell["execution_count"] = value.executionCount;
                cell["outputs"] = value.outputs is {Count: > 0} ? JArray.FromObject(value.outputs) : new JArray();
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

            cell.cellType = obj["cell_type"]?.ToObject<Notebook.CellType>() ?? Code;
            // TODO metadata
            // cell.metadata = obj["metadata"]?.ToObject<List<Notebook.CellMetadataEntry>>() ?? new List<Notebook.CellMetadataEntry>();
            cell.source = obj["source"]?.ToObject<string[]>() ?? Array.Empty<string>();
            if (cell.cellType == Code)
            {
                var outputsList = obj["outputs"];
                if (outputsList is {HasValues: true})
                {
                    cell.outputs = outputsList.ToObject<List<Notebook.CellOutput>>() ?? new List<Notebook.CellOutput>();
                }
            }

            return cell;
        }
    }
}