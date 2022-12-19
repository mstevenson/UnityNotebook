using UnityEngine;

public abstract class NotebookRenderer
{
    public abstract string[] MimeTypes { get; }
    
    public abstract void Render(Notebook.CellOutput content);
}
