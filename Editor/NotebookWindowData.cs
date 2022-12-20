using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [FilePath("UserSettings/NotebookWindow.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NotebookWindowData : ScriptableSingleton<NotebookWindowData>
    {
        [SerializeField] private Notebook openedNotebook;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private int runningCell;

        public Notebook OpenedNotebook
        {
            get => openedNotebook;
            set
            {
                openedNotebook = value;
                Save(true);
            }
        }
        
        public Vector2 Scroll
        {
            get => scroll;
            set
            {
                scroll = value;
                Save(true);
            }
        }
        
        public int RunningCell
        {
            get => runningCell;
            set
            {
                runningCell = value;
                Save(true);
            }
        }
        
        public void Clear()
        {
            openedNotebook = null;
            scroll = default;
            runningCell = -1;
            Save(true);
        }
    }
}