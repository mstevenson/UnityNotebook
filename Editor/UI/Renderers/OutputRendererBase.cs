namespace UnityNotebook
{
    public abstract class OutputRendererBase
    {
        public abstract System.Type[] SupportedTypes { get; }

        public abstract void DrawGUI(object value);
    }
}