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
        [SerializeField] private int previewImageSize = 200;

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
        
        public static int RunningCell
        {
            get => instance.runningCell;
            set
            {
                instance.runningCell = value;
                instance.Save(true);
            }
        }
        
        public static int PreviewImageSize
        {
            get => instance.previewImageSize;
            set
            {
                instance.previewImageSize = value;
                instance.Save(true);
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