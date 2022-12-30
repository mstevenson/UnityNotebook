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
            var img = NBState.GetTexture(obj.hash);
            if (img == null)
            {
                obj.hash = NBState.CacheTexture(obj.imageB64);
            }

            if (img != null)
            {
                GUILayout.Label(img);
            }
            if (obj.info != null)
            {
                GUILayout.Label(obj.info);
            }
        }
    }
}