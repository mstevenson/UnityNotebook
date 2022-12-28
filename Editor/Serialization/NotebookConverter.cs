using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityNotebook
{
    public class NotebookConverter : JsonConverter<Notebook>
    {
        public override Notebook ReadJson(JsonReader reader, Type objectType, Notebook existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);
            if (!obj.HasValues)
            {
                return ScriptableObject.CreateInstance<Notebook>();
            }
            var nb = hasExistingValue ? existingValue : ScriptableObject.CreateInstance<Notebook>();
            nb.format = obj["nbformat"]?.Value<int>() ?? 4;
            nb.formatMinor = obj["nbformat_minor"]?.Value<int>() ?? 2;
            var cellsList = obj["cells"];
            if (cellsList is {HasValues: true})
            {
                nb.cells = cellsList.ToObject<List<Cell>>();
            }
            return nb;
        }

        public override void WriteJson(JsonWriter writer, Notebook value, JsonSerializer serializer)
        {
            var nb = new JObject
            {
                ["nbformat"] = value.format,
                ["nbformat_minor"] = value.formatMinor,
                ["metadata"] = new JObject
                {
                    ["kernelspec"] = new JObject
                    {
                        ["display_name"] = ".Net (C#)",
                        ["language"] = "C#",
                        ["name"] = ".net-csharp"
                    }
                },
                ["cells"] = JArray.FromObject(value.cells)
            };
            nb.WriteTo(writer);
        }
    }
}