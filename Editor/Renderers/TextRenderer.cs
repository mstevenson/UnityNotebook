using System;
using System.Collections.Generic;
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
            if (value is string str)
            {
                GUILayout.Label(str);
            }
        }
    }
}