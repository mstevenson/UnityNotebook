using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static UnityNotebook.CellType;

namespace UnityNotebook
{
    public class CellConverter : JsonConverter<Cell>
    {
        public override Cell ReadJson(JsonReader reader, Type objectType, Cell existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return new Cell();
            }

            var cell = hasExistingValue ? existingValue : new Cell();

            cell.cellType = obj["cell_type"]?.ToObject<CellType>() ?? Code;
            // TODO metadata
            // cell.metadata = obj["metadata"]?.ToObject<List<Notebook.CellMetadataEntry>>() ?? new List<Notebook.CellMetadataEntry>();
            cell.source = obj["source"]?.ToObject<string[]>() ?? Array.Empty<string>();
            if (cell.cellType == Code)
            {
                var outputsList = obj["outputs"];
                if (outputsList is {HasValues: true})
                {
                    cell.outputs = outputsList.ToObject<List<CellOutput>>() ?? new List<CellOutput>();
                }
            }

            return cell;
        }

        public override void WriteJson(JsonWriter writer, Cell value, JsonSerializer serializer)
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
    }
}