using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class TextRenderer : OutputRendererBase
    {
        public override string[] MimeTypes { get; } = { "text/plain", "text/markdown", "application/json" };
        public override Type[] SupportedTypes { get; } = { typeof(string) }; // also is a fallback to support any type by calling ToString()

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            foreach (var line in content.stringData)
            {
                GUILayout.Label(line.Replace("\n", ""));
            }
        }

        public override Notebook.CellOutput ObjectToCellOutput(object obj)
        {
            var value = obj as string ?? obj.ToString();
            
            return new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new()
                    {
                        mimeType = "text/plain",
                        stringData = new List<string>(value.Split('\n'))
                    }
                }
            };
        }
    }
}