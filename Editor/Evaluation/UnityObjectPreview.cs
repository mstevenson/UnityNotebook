using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    public class UnityObjectPreview
    {
        public Texture2D image;
        public string info;
        public ValueWrapper value;
        
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
            
            preview.value = new ValueWrapper(obj);

            return preview;
        }
    }
}