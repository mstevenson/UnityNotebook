using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class TextRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { "text/plain", "text/markdown", "application/json" };
        public override Type[] SupportedTypes { get; } = { typeof(string) }; // also is a fallback to support any type by calling ToString()

        public override void DrawGUI(Notebook.CellOutputDataEntry content)
        {
            if (content.backingValue.Object is string str)
            {
                GUILayout.Label(str);
            }
        }

        public override Notebook.CellOutput CreateCellOutputData(object obj)
        {
            var output = new Notebook.CellOutputDisplayData();
            output.data.Add(new Notebook.CellOutputDataEntry
            {
                mimeType = "text/plain",
                backingValue = new ValueWrapper(obj.ToString())
            });
            return output;
        }
    }
}