using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityNotebook
{
    [UsedImplicitly]
    public class ImageRenderer : OutputRenderer
    {
        public override string[] MimeTypes { get; } = { "image/png", "image/jpeg" };
        public override Type[] SupportedTypes { get; } = { typeof(Texture2D) };

        public override void Render(Notebook.CellOutputDataEntry content)
        {
            var img = content.imageData;
            if (img != null)
            {
                GUILayout.BeginVertical();
                GUILayout.Box(img, GUILayout.Width(img.width), GUILayout.Height(img.height));
                GUILayout.EndVertical();
            }
        }

        public override Notebook.CellOutput ObjectToCellOutput(object obj)
        {
            if (!SupportedTypes.Contains(obj.GetType()))
            {
                throw new ArgumentException($"Object type {obj.GetType()} is not supported by this renderer");
            }
            
            return new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new()
                    {
                        mimeType = "image/png",
                        imageData = (Texture2D)obj
                    }
                }
            };
        }
    }
}