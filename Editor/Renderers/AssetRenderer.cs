using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class AssetRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { $"{UnityMimePrefix}texture", $"{UnityMimePrefix}material", $"{UnityMimePrefix}mesh", $"{UnityMimePrefix}gameobject" };
        public override Type[] SupportedTypes { get; } = { typeof(Texture), typeof(Material), typeof(Mesh), typeof(GameObject) };

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            var asset = content.asset as UnityEngine.Object;
            var preview = AssetPreview.GetAssetPreview(asset);
            // var size = NotebookWindowData.PreviewImageSize;
            var rect = GUILayoutUtility.GetRect(preview.width, preview.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, preview);
            
            var assetName = (string.IsNullOrEmpty(asset.name) ? "Unnamed" : asset.name) + $" ({asset.GetType().Name})";
            var label = $"{assetName} • " + content.asset switch
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
            var mimeType = obj switch
            {
                Texture2D _ => $"{UnityMimePrefix}texture",
                Material _ => $"{UnityMimePrefix}material",
                Mesh _ => $"{UnityMimePrefix}mesh",
                GameObject _ => $"{UnityMimePrefix}gameobject",
                _ => throw new ArgumentException($"Object type {obj.GetType()} is not supported by this renderer")
            };
            
            return new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new()
                    {
                        mimeType = mimeType,
                        asset = obj
                    }
                }
            };
        }
    }
}