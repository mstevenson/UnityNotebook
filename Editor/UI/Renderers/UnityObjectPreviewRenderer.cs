using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class UnityObjectPreviewRenderer : OutputRendererBase
    {
        public override Type[] SupportedTypes { get; } = { typeof(UnityObjectPreview) };
        
        public override void DrawGUI(object value)
        {
            var obj = value as UnityObjectPreview;
            var img = obj.image;
            
            Debug.Log(obj.image);
            // Debug.Log(obj.info);
            
            if (img != null)
            {
                var rect = GUILayoutUtility.GetRect(img.width, img.height, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(rect, img);
            }
            if (obj.info != null)
            {
                GUILayout.Label(obj.info);
            }
        }
    }
}