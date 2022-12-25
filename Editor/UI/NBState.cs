using System;
using Microsoft.CodeAnalysis.Scripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityNotebook
{
    // Singleton state object for the Notebook window
    [FilePath("UserSettings/NotebookWindow.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NBState : ScriptableSingleton<NBState>
    {
        [SerializeField] private Notebook openedNotebook;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private int selectedCell;
        [SerializeField] private int runningCell;
        [SerializeField] private bool isEditMode;
        [SerializeField] private bool isJsonOutOfDate;
        
        [NonSerialized] public ScriptState scriptState;

        // HACK
        [NonSerialized] public bool forceSyntaxRefresh;

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

        public static bool IsJsonOutOfDate
        {
            get => instance.isJsonOutOfDate;
            set
            {
                instance.isJsonOutOfDate = value;
                instance.Save(true);
            }
        }

        public static void CloseNotebook()
        {
            instance.openedNotebook = null;
            instance.scroll = default;
            instance.selectedCell = -1;
            instance.runningCell = -1;
            instance.isJsonOutOfDate = false;
            SaveScriptableObject();
            SaveJson();
            instance.Save(true);
        }

        public static void ClearOutput()
        {
            var nb = instance.openedNotebook;
            if (nb == null)
            {
                return;
            }
            nb.ClearOutputs();
            SaveScriptableObject();
        }

        public static void SetDirty()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                EditorUtility.SetDirty(nb);
                IsJsonOutOfDate = true;
            }
        }
        
        public static void SaveJson()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                nb.SaveJson();
            }
            IsJsonOutOfDate = false;
        }
        
        public static void SaveScriptableObject()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                nb.SaveScriptableObject();
            }
        }
        
        // Update the cell's source lines stored in json from the raw text used by the UI
        public static void CopyRawTextToSourceLines(Notebook.Cell cell)
        {
            cell.source = cell.rawText.Split('\n');
            // add stripped newline char back onto each line
            for (var i = 0; i < cell.source.Length; i++)
            {
                if (i < cell.source.Length - 1)
                {
                    cell.source[i] += '\n';
                }
            }
        }
    }
}