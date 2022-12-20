using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityNotebook
{
    public static class NotebookUtils
    {
        public static Notebook.CellOutput Stream(string name, IEnumerable<string> text)
        {
            var output = new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.Stream,
                name = name,
                text = text.ToList()
            };
            return output;
        }

        public static Notebook.CellOutput Exception(Exception exception)
        {
            var output = new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.Error,
                ename = exception.GetType().Name,
                evalue = exception.Message,
                traceback = new List<string>(exception.StackTrace.Split('\n'))
            };
            return output;
        }
        
        public static Notebook.CellOutput DisplayData(string text)
        {
            var output = new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new() { mimeType = "text/plain", stringData = { text } }
                }
            };
            return output;
        }
        
        public static Notebook.CellOutput DisplayData(Texture2D texture)
        {
            var output = new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new() { mimeType = "image/png", imageData = texture }
                }
            };
            return output;
        }

        public static Notebook.CellOutput DisplayData<T>(T value)
        {
            var output = new Notebook.CellOutput
            {
                outputType = Notebook.OutputType.DisplayData,
                data = new List<Notebook.CellOutputDataEntry>
                {
                    new() { mimeType = "text/plain", stringData = { value.ToString() } }
                }
            };
            return output;
        }
    }
}