using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CellMetadataConverter: JsonConverter<Notebook.CellMetadata>
{
    public override void WriteJson(JsonWriter writer, Notebook.CellMetadata value, JsonSerializer serializer)
    {
        // var metadata = new JObject
        // {
        //     ["collapsed"] = value.collapsed,
        //     ["autoscroll"] = value.autoscroll.ToString().ToLower(),
        //     ["deletable"] = value.deletable
        // };
        // if (value.format != null) metadata["format"] = value.format;
        // if (value.name != null) metadata["name"] = value.name;
        // if (value.tags != null) metadata["tags"] = JArray.FromObject(value.tags);
        // metadata.WriteTo(writer);        
    }

    public override Notebook.CellMetadata ReadJson(JsonReader reader, Type objectType, Notebook.CellMetadata existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        
        var meta = hasExistingValue ? existingValue : new Notebook.CellMetadata();

        meta.collapsed = obj?["collapsed"]?.Value<bool>() ?? false;
        meta.autoscroll = obj?["autoscroll"]?.Type == JTokenType.Boolean
            ? obj["autoscroll"].Value<bool>() ? Notebook.AutoScroll.True : Notebook.AutoScroll.False
            : Notebook.AutoScroll.Auto;
        meta.deletable = obj?["deletable"]?.Value<bool>() ?? true;
        meta.format = obj?["format"]?.Value<string>();
        meta.name = obj?["name"]?.Value<string>();
        meta.tags = obj?["tags"]?.ToObject<List<string>>();

        return meta;
    }
}
