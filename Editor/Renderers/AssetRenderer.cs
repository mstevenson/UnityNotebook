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
        public override Type[] SupportedTypes { get; } = { typeof(Texture), typeof(Material), typeof(Mesh), typeof(GameObject) };
        
        public override void DrawGUI(object value)
        {
            
            // var asset = content.obj;
            // var cachedPreview = GetAssetImage(asset, content.Id);
            // var rect = GUILayoutUtility.GetRect(cachedPreview.width, cachedPreview.height, GUILayout.ExpandWidth(false));
            // EditorGUI.DrawPreviewTexture(rect, cachedPreview);
            // GUILayout.Label(label);
        }
    }
}