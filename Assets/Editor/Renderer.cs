namespace Editor
{
    public abstract class Renderer
    {
        public abstract string[] MimeTypes { get; }
        
        public abstract void Render(Notebook.CellOutput content);
    }
}