using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [FilePath("UserSettings/NotebookWindow.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NotebookWindowData : ScriptableSingleton<NotebookWindowData>
    {
        public Notebook openedNotebook;
        public Vector2 scroll;
        public int runningCell;

        public void Save() => base.Save(true);

        public void Clear()
        {
            openedNotebook = null;
            scroll = default;
            runningCell = -1;
            EditorUtility.SetDirty(this);
        }
    }
}