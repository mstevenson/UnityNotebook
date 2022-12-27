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

        public override void DrawGUI(Notebook.CellOutputDataEntry content)
        {
            EditorGUILayout.CurveField(content.curve, GUILayout.Width(250), GUILayout.Height(120));
        }
        
        public override Notebook.CellOutput CreateCellOutputData(object obj)
        {
            var output = new Notebook.CellOutputDisplayData();
            output.data.Add(new Notebook.CellOutputDataEntry
            {
                mimeType = MimeTypes[0],
                curve = (AnimationCurve) obj
            });
            return output;
        }
    }
}