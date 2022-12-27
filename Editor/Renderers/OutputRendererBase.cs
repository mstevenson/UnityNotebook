using System.Collections.Generic;

namespace UnityNotebook
{
    public abstract class OutputRendererBase
    {
        protected const string UnityMimePrefix = "application/vnd.unity.";

        public abstract string[] MimeTypes { get; }
        public abstract System.Type[] SupportedTypes { get; }

        public abstract void DrawGUI(Notebook.CellOutputDataEntry content);

        public abstract Notebook.CellOutput CreateCellOutputData(object obj);
    }
}