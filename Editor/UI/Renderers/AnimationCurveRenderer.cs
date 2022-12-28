using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class AnimationCurveRenderer : OutputRendererBase
    {
        public override Type[] SupportedTypes { get; } = { typeof(AnimationCurve) };

        public override void DrawGUI(object value)
        {
            EditorGUILayout.CurveField(value as AnimationCurve, GUILayout.Width(250), GUILayout.Height(120));
        }
    }
}