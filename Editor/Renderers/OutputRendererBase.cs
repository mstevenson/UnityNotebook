namespace UnityNotebook
{
    public abstract class OutputRendererBase
    {
        protected const string UnityMimePrefix = "application/vnd.unity.";

        public abstract string[] MimeTypes { get; }
        public abstract System.Type[] SupportedTypes { get; }

        // TODO take metadata as input
        public abstract void Render(Notebook.CellOutputDataEntry content);

        public abstract Notebook.CellOutput ObjectToCellOutput(object obj);
    }
}