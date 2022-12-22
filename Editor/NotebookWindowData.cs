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
        [SerializeField] private bool isEditMode;

        public static Notebook OpenedNotebook
        {
            get => instance.openedNotebook;
            set
            {
                if (instance.openedNotebook == value) return;
                instance.openedNotebook = value;
                instance.Save(true);
            }
        }
        
        public static Vector2 Scroll
        {
            get => instance.scroll;
            set
            {
                if (instance.scroll == value) return;
                instance.scroll = value;
                instance.Save(true);
            }
        }
        
        public static int SelectedCell
        {
            get => instance.selectedCell;
            set
            {
                if (instance.selectedCell == value) return;
                instance.selectedCell = value;
                instance.Save(true);
            }
        }
        
        public static int RunningCell
        {
            get => instance.runningCell;
            set
            {
                if (instance.runningCell == value) return;
                instance.runningCell = value;
                instance.Save(true);
            }
        }
        
        public static bool IsEditMode
        {
            get => instance.isEditMode;
            set
            {
                if (instance.isEditMode == value) return;
                instance.isEditMode = value;
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