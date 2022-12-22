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
            EditorGUILayout.ColorField((Color)content.value, GUILayout.Width(100), GUILayout.Height(100));
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