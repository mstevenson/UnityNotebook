using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    [FilePath("UserSettings/NotebookWindow.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NotebookWindowData : ScriptableSingleton<NotebookWindowData>
    {
        [SerializeField] private Notebook openedNotebook;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private int selectedCell;
        [SerializeField] private int runningCell;

        public static Notebook OpenedNotebook
        {
            get => instance.openedNotebook;
            set
            {
                instance.openedNotebook = value;
                instance.Save(true);
            }
        }
        
        public static Vector2 Scroll
        {
            get => instance.scroll;
            set
            {
                instance.scroll = value;
                instance.Save(true);
            }
        }
        
        public static int SelectedCell
        {
            get => instance.selectedCell;
            set
            {
                instance.selectedCell = value;
                instance.Save(true);
            }
        }
        
        public static int RunningCell
        {
            get => instance.runningCell;
            set
            {
                instance.runningCell = value;
                instance.Save(true);
            }
        }
        
        public void Clear()
        {
            openedNotebook = null;
            scroll = default;
            selectedCell = -1;
            runningCell = -1;
            Save(true);
        }
    }
}