namespace UnityNotebook
{
    public abstract class NotebookRenderer
    {
        public const string UnityMimePrefix = "application/vnd.unity.";

        public abstract string[] MimeTypes { get; }

        public abstract void Render(Notebook.CellOutput content);
    }
}