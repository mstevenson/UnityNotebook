using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    // [UsedImplicitly]
    // public class ImageRenderer : OutputRenderer
    // {
    //     public override string[] MimeTypes { get; } = { $"{UnityMimePrefix}texture" };
    //     public override Type[] SupportedTypes { get; } = { typeof(Texture2D) };
    //
    //     public override void Render(Notebook.CellOutputDataEntry content)
    //     {
    //         var tex = content.imageData;
    //         if (tex != null)
    //         {
    //             var rect = GUILayoutUtility.GetRect(tex.width, tex.height, GUILayout.ExpandWidth(false));
    //             EditorGUI.DrawPreviewTexture(rect, tex);
    //             var texName = string.IsNullOrEmpty(tex.name) ? "Unnamed" : tex.name;
    //             GUILayout.Label($"{texName} ({tex.GetType().Name}) • {tex.width}x{tex.height} • {tex.format} • {tex.graphicsFormat}");
    //         }
    //     }
    //
    //     public override Notebook.CellOutput ObjectToCellOutput(object obj)
    //     {
    //         if (!SupportedTypes.Contains(obj.GetType()))
    //         {
    //             throw new ArgumentException($"Object type {obj.GetType()} is not supported by this renderer");
    //         }
    //         
    //         return new Notebook.CellOutput
    //         {
    //             outputType = Notebook.OutputType.DisplayData,
    //             data = new List<Notebook.CellOutputDataEntry>
    //             {
    //                 new()
    //                 {
    //                     mimeType = $"{UnityMimePrefix}texture",
    //                     imageData = (Texture2D)obj
    //                 }
    //             }
    //         };
    //     }
    // }
}