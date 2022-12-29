using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityNotebook
{
    [Serializable]
    [JsonConverter(typeof(UnityObjectPreviewConverter))]
    public class UnityObjectPreview
    {
        public string info;
        public string imageB64;
        
        [JsonIgnore]
        public int hash;
        
        public static UnityObjectPreview Create(Object obj)
        {
            var preview = new UnityObjectPreview();
            
            var tempPreview = AssetPreview.GetAssetPreview(obj);
            if (tempPreview != null)
            {
                var tex = new Texture2D(tempPreview.width, tempPreview.height, tempPreview.format, false);
                Graphics.CopyTexture(tempPreview, tex);
                // We have to destroy the preview texture, otherwise the GUI system will reuse a stale
                // texture during subsequence GetAssetPreview calls for the same asset.
                Object.DestroyImmediate(tempPreview);
                
                // Cache the texture
                var bytes = tex.EncodeToPNG();
                preview.imageB64 = Convert.ToBase64String(bytes);
                preview.hash = NBState.CacheTexture(bytes);
            }

            // Info
            var assetName = (string.IsNullOrEmpty(obj.name) ? "Unnamed" : obj.name) + $" ({obj.GetType().Name})";
            preview.info = $"{assetName} • " + obj switch
            {
                Texture tex1 => $"{tex1.width}x{tex1.height} • {tex1.graphicsFormat}",
                Material mat => $"{mat.shader.name}",
                Mesh mesh => $"{mesh.vertexCount} vertices • {mesh.triangles.Length / 3} triangles",
                GameObject go => $"",
                _ => assetName
            };
            
            return preview;
        }

        public class UnityObjectPreviewConverter : JsonConverter<UnityObjectPreview>
        {
            public override UnityObjectPreview ReadJson(JsonReader reader, Type objectType, UnityObjectPreview existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var obj = JToken.Load(reader);
                if (!obj.HasValues)
                {
                    return new UnityObjectPreview();
                }

                var result = new UnityObjectPreview
                {
                    info = obj["info"].ToObject<string>()
                };

                if (obj["image"].HasValues)
                {
                    var b64Array = obj["image"].ToObject<List<string>>();
                    var b64 = string.Concat(b64Array);
                    result.imageB64 = b64;
                    // used by the editor for cached texture lookup
                    result.hash = NBState.CacheTexture(b64);
                }
                
                return result;
            }

            public override void WriteJson(JsonWriter writer, UnityObjectPreview value, JsonSerializer serializer)
            {
                var preview = new JObject
                {
                    ["image"] = string.IsNullOrEmpty(value.imageB64) ? new JArray() : JArray.FromObject(new[] { value.imageB64 }),
                    ["info"] = value.info,
                };

                preview.WriteTo(writer);
            }
        }
    }
}