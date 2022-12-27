using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    // Static methods that are available to a running notebook cell
    [UsedImplicitly]
    public static class RuntimeMethods
    {
        // TODO render asset preview images
        
        public static void Show(object data)
        {
            if (data == null)
            {
                return;
            }
            var notebook = NBState.OpenedNotebook;
            var cell = NBState.RunningCell;
            var output = new Notebook.CellOutputDisplayData();
            output.values.Add(new ValueWrapper(data));
            notebook.cells[cell].outputs.Add(output);
        }
        
        // TODO call this
        
        private static void GetPreviewImage(UnityEngine.Object unityObject)
        {
            var tempPreview = AssetPreview.GetAssetPreview(unityObject);
            if (tempPreview != null)
            {
                var tex = new Texture2D(tempPreview.width, tempPreview.height, tempPreview.format, false);
                Graphics.CopyTexture(tempPreview, tex);
                // We have to destroy the preview texture, otherwise the GUI system will reuse a stale
                // texture during subsequence GetAssetPreview calls for the same asset.
                Object.DestroyImmediate(tempPreview);
                
                // Image
                // output.values.Add(new Notebook.CellOutputDataEntry
                // {
                //     mimeType = "image/png",
                //     backingValue = new ValueWrapper(tex),
                // });
            }
            
            // Info
            // output.values.Add(new Notebook.CellOutputDataEntry
            // {
            //     mimeType = "text/plain",
            //     data = new List<string> { GetInfoString(unityObject) }
            // });
            
        }
        
        private static string GetInfoString(UnityEngine.Object asset)
        {
            var assetName = (string.IsNullOrEmpty(asset.name) ? "Unnamed" : asset.name) + $" ({asset.GetType().Name})";
            var label = $"{assetName} • " + asset switch
            {
                Texture tex => $"{tex.width}x{tex.height} • {tex.graphicsFormat}",
                Material mat => $"{mat.shader.name}",
                Mesh mesh => $"{mesh.vertexCount} vertices • {mesh.triangles.Length / 3} triangles",
                GameObject go => $"",
                _ => assetName
            };
            return label;
        }
    }
}