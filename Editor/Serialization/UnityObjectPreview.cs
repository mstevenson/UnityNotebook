using System;
using System.Collections.Generic;
using System.Text;
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
        [SerializeReference]
        public Texture2D image;
        public string info;
        
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
                preview.image = tex;
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

                var result = new UnityObjectPreview();
                
                // TODO check if image exists
                
                var b64 = obj["image"].ToObject<List<string>>();
                var bytes = Convert.FromBase64String(string.Concat(b64));
                var tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                result.image = tex;
                
                result.info = obj["info"].ToObject<string>();
                
                // TODO the image is being set correctly here, but is lost by the time it's accessed.
                // Probably due to serialization weirdness by using [SerializeReference].
                // Store these in a dictionary and access them by guid?
                Debug.Log("Deserialized image: " + result.image);
                
                return result;
            }

            public override void WriteJson(JsonWriter writer, UnityObjectPreview value, JsonSerializer serializer)
            {
                // TODO check if image exists
                
                var bytes = value.image.EncodeToPNG();
                var b64 = Convert.ToBase64String(bytes);
                var lines = new[] { b64 }; 
                
                var preview = new JObject
                {
                    ["image"] = JArray.FromObject(lines),
                    ["info"] = value.info,
                };
                
                preview.WriteTo(writer);
            }
        }
    }
}