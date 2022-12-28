using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class ColorRenderer : OutputRendererBase
    {
        public override Type[] SupportedTypes { get; } = { typeof(Color) };

        public override void DrawGUI(object content)
        {
            var color = (Color)content;
            var hdr = color.maxColorComponent > 1.0f;
            EditorGUILayout.ColorField(GUIContent.none, color, false, true, hdr, GUILayout.Width(100), GUILayout.Height(100));
            GUILayout.Label($"RGBA {color.r:0.00}, {color.g:0.00}, {color.b:0.00}, {color.a:0.00} • Hex {ColorUtility.ToHtmlStringRGBA(color)}");
        }
    }
}