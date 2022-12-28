using System;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

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
        [NonSerialized] public bool forceFocusCodeArea;

        public static Notebook OpenedNotebook
        {
            get => instance.openedNotebook;
            set
            {
                instance.openedNotebook = value;
                if (instance.openedNotebook == value) return;
                instance.Save(true);
            }
        }
        
        public static Vector2 Scroll
        {
            get => instance.scroll;
            set
            {
                instance.scroll = value;
                if (instance.scroll == value) return;
                instance.Save(true);
            }
        }
        
        public static int SelectedCell
        {
            get => instance.selectedCell;
            set
            {
                instance.selectedCell = value;
                if (instance.selectedCell == value) return;
                instance.Save(true);
            }
        }
        
        public static int RunningCell
        {
            get => instance.runningCell;
            set
            {
                instance.runningCell = value;
                if (instance.runningCell == value) return;
                instance.Save(true);
            }
        }
        
        public static bool IsEditMode
        {
            get => instance.isEditMode;
            set
            {
                instance.forceFocusCodeArea = true;
                instance.isEditMode = value;
                if (instance.isEditMode == value) return;
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
            foreach (var cell in nb.cells)
            {
                cell.executionCount = 0;
                cell.outputs.Clear();
            }
            EditorUtility.SetDirty(nb);
            SaveScriptableObject();
        }

        public static void SetNotebookDirty()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                EditorUtility.SetDirty(nb);
                IsJsonOutOfDate = true;
            }
        }
        
        public static Notebook CreateNotebookAsset(string path)
        {
            var notebook = CreateInstance<Notebook>();
            var json = JsonConvert.SerializeObject(notebook, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Notebook>(path);
        }
        
        public static void SaveJson()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                SaveScriptableObject();
                var json = JsonConvert.SerializeObject(nb, Formatting.Indented);
                System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(nb), json);
            }
            IsJsonOutOfDate = false;
        }
        
        public static void SaveScriptableObject()
        {
            var nb = instance.openedNotebook;
            if (nb != null)
            {
                EditorUtility.SetDirty(nb);
                AssetDatabase.SaveAssetIfDirty(nb);
                EditorUtility.ClearDirty(nb);
            }
        }
        
        // Update the cell's source lines stored in json from the raw text used by the UI
        public static void CopyRawTextToSourceLines(Cell cell)
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

        public static void Reset()
        {
            instance.openedNotebook = null;
            instance.scroll = default;
            instance.selectedCell = -1;
            instance.runningCell = -1;
            instance.isJsonOutOfDate = false;
            instance.isEditMode = false;
            instance.scriptState = null;
            instance.forceSyntaxRefresh = false;
            instance.forceFocusCodeArea = false;
            instance.Save(true);
        }
    }
}