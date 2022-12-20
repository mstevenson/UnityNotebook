namespace UnityNotebook
{
    public class ImageRenderer : NotebookRenderer
    {
        public override string[] MimeTypes { get; } = { "image/png", "image/jpeg" };
        
        public override void Render(Notebook.CellOutput content)
        {
            
        }
    }
}