namespace UnityNotebook
{
    public class TextRenderer : NotebookRenderer
    {
        public override string[] MimeTypes { get; } = { "text/plain", "text/markdown", "application/json" };
        
        public override void Render(Notebook.CellOutput content)
        {
            
        }
    }
}