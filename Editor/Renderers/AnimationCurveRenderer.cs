using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class AnimationCurveRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { $"{UnityMimePrefix}animationcurve" };
        public override Type[] SupportedTypes { get; } = { typeof(AnimationCurve) };

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            EditorGUILayout.CurveField(content.curve, GUILayout.Width(250), GUILayout.Height(120));
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
                        curve = (AnimationCurve)obj
                    }
                }
            };
        }
    }
}