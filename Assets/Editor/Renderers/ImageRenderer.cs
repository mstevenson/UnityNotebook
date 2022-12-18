namespace Editor.Renderers
{
    public class ImageRenderer : Renderer
    {
        public override string[] MimeTypes { get; } = { "image/png", "image/jpeg" };
        
        public override void Render(Notebook.CellOutput content)
        {
            
        }
    }
}