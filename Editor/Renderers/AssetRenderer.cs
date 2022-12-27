using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class AssetRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { $"{UnityMimePrefix}texture", $"{UnityMimePrefix}material", $"{UnityMimePrefix}mesh", $"{UnityMimePrefix}gameobject" };
        public override Type[] SupportedTypes { get; } = { typeof(Texture), typeof(Material), typeof(Mesh), typeof(GameObject) };
        
        public override void DrawGUI(Notebook.CellOutputDataEntry content)
        {
            // var asset = content.obj;
            // var cachedPreview = GetAssetImage(asset, content.Id);
            // var rect = GUILayoutUtility.GetRect(cachedPreview.width, cachedPreview.height, GUILayout.ExpandWidth(false));
            // EditorGUI.DrawPreviewTexture(rect, cachedPreview);
            // GUILayout.Label(label);
        }
        
        public override Notebook.CellOutput CreateCellOutputData(object obj)
        {
            var unityObject = (Object) obj;
            var output = new Notebook.CellOutputDisplayData();
            
            var tempPreview = AssetPreview.GetAssetPreview(unityObject);
            if (tempPreview != null)
            {
                var tex = new Texture2D(tempPreview.width, tempPreview.height, tempPreview.format, false);
                Graphics.CopyTexture(tempPreview, tex);
                // We have to destroy the preview texture, otherwise the GUI system will reuse a stale
                // texture during subsequence GetAssetPreview calls for the same asset.
                Object.DestroyImmediate(tempPreview);
                
                // Image
                output.data.Add(new Notebook.CellOutputDataEntry
                {
                    mimeType = "image/png",
                    backingValue = new ValueWrapper(tex),
                });
            }
            
            // Info
            output.data.Add(new Notebook.CellOutputDataEntry
            {
                mimeType = "text/plain",
                data = new List<string> { GetInfoString(unityObject) }
            });
            
            // Metadata will be added by the json converter during serialization

            return output;
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