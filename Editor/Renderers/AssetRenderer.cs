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
        
        // TODO leaks textures? automatically clean up
        private static readonly Dictionary<int, Texture2D> AssetPreviews = new();

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            var asset = content.obj;

            // Cache asset preview textures so that different outputs can show separate visual states of the same asset 
            if (!AssetPreviews.TryGetValue(content.Id, out var cachedPreview))
            {
                var tempPreview = AssetPreview.GetAssetPreview(asset);
                var newPreview = new Texture2D(tempPreview.width, tempPreview.height, tempPreview.format, false);
                Graphics.CopyTexture(tempPreview, newPreview);
                // We have to destroy the preview texture, otherwise the GUI system will reuse a stale
                // texture during subsequence GetAssetPreview calls for the same asset.
                Object.DestroyImmediate(tempPreview);
                AssetPreviews.Add(content.Id, newPreview);
                cachedPreview = newPreview;
            }
            var rect = GUILayoutUtility.GetRect(cachedPreview.width, cachedPreview.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, cachedPreview);
            
            var assetName = (string.IsNullOrEmpty(asset.name) ? "Unnamed" : asset.name) + $" ({asset.GetType().Name})";
            var label = $"{assetName} • " + asset switch
            {
                Texture tex => $"{tex.width}x{tex.height} • {tex.graphicsFormat}",
                Material mat => $"{mat.shader.name}",
                Mesh mesh => $"{mesh.vertexCount} vertices • {mesh.triangles.Length / 3} triangles",
                GameObject go => $"",
                _ => assetName
            };
            GUILayout.Label(label);
        }

        public override Notebook.CellOutput ObjectToCellOutput(object obj)
        {
            var o = (Object) obj;
            
            var mimeType = o switch
            {
                Texture2D _ => $"{UnityMimePrefix}texture",
                Material _ => $"{UnityMimePrefix}material",
                Mesh _ => $"{UnityMimePrefix}mesh",
                GameObject _ => $"{UnityMimePrefix}gameobject",
                _ => throw new ArgumentException($"Object type {o.GetType()} is not supported by this renderer")
            };
            
            return new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new()
                    {
                        mimeType = mimeType,
                        obj = o
                    }
                }
            };
        }
    }
}