namespace Editor.Renderers
{
    public class TextRenderer : Renderer
    {
        public override string[] MimeTypes { get; } = { "text/plain", "text/markdown", "application/json" };
        
        public override void Render(Notebook.CellOutput content)
        {
            
        }
    }
}