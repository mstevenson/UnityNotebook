using UnityEditor;
using UnityEngine;

[FilePath("UserSettings/NotebookWindow.asset", FilePathAttribute.Location.ProjectFolder)]
public class NotebookWindowData : ScriptableSingleton<NotebookWindowData>
{
    public Notebook openedNotebook;
    public Vector2 scroll;

    public void Save() => base.Save(true);
}