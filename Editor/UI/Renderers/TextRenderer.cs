using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class TextRenderer : OutputRendererBase
    {
        public override Type[] SupportedTypes { get; } = { typeof(string) }; // also is a fallback to support any type by calling ToString()

        public override void DrawGUI(object value)
        {
            var str = value is string s ? s : value.ToString();
            GUILayout.Label(str);
        }
    }
}