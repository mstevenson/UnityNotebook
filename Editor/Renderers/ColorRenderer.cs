using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class ColorRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { $"{UnityMimePrefix}color" };
        public override Type[] SupportedTypes { get; } = { typeof(Color) };

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            var c = (Color)content.value;
            var hdr = c.maxColorComponent > 1.0f;
            EditorGUILayout.ColorField(GUIContent.none, c, false, true, hdr, GUILayout.Width(100), GUILayout.Height(100));
            GUILayout.Label($"RGBA {c.r:0.00}, {c.g:0.00}, {c.b:0.00}, {c.a:0.00} • Hex {ColorUtility.ToHtmlStringRGBA(c)}");
        }

        public override Notebook.CellOutput ObjectToCellOutput(object obj)
        {
            return new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new()
                    {
                        mimeType = MimeTypes[0],
                        value = obj
                    }
                }
            };
        }
    }
}