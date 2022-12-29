using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    // Singleton state object for the Notebook window
    [FilePath("UserSettings/NotebookState.asset", FilePathAttribute.Location.ProjectFolder)]
    public class NBState : ScriptableSingleton<NBState>
    {
        [SerializeField] private Notebook openedNotebook;
        [SerializeField] private Vector2 scroll;
        [SerializeField] private int selectedCell;
        [SerializeField] private int runningCell;
        [SerializeField] private bool isEditMode;
        [SerializeField] private bool isJsonOutOfDate;
        [SerializeField] private List<int> texHashes = new();
        [SerializeField] private List<Texture2D> texCache = new();
        
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
            DiscardAllTextures();
            SaveScriptableObject();
            SaveJson();
            instance.Save(true);
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

        private static int GetTextureHash(byte[] bytes)
        {
            var hash = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                hash = (hash * 397) ^ bytes[i];
            }
            return hash;
        }

        public static Texture2D GetTexture(int hash)
        {
            if (hash == 0)
            {
                return null;
            }
            for (int i = 0; i < instance.texHashes.Count; i++)
            {
                if (instance.texHashes[i] == hash)
                {
                    var tex = instance.texCache[i];
                    return tex;
                }
            }
            return null;
        }
        
        public static int CacheTexture(string b64)
        {
            if (string.IsNullOrEmpty(b64))
            {
                return 0;
            }
            var bytes = Convert.FromBase64String(string.Concat(b64));
            int hash = CacheTexture(bytes);
            return hash;
        }

        public static int CacheTexture(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return 0;
            }
            var hash = GetTextureHash(bytes);
            var index = instance.texHashes.IndexOf(hash);
            if (index != -1)
            {
                return hash;
            }
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            instance.texHashes.Add(hash);
            instance.texCache.Add(tex);
            return hash;
        }
        
        public static void DiscardTexture(int hash)
        {
            if (hash == 0)
            {
                return;
            }
            var index = instance.texHashes.IndexOf(hash);
            if (index == -1)
            {
                return;
            }
            var tex = instance.texCache[index];
            DestroyImmediate(tex);
            instance.texHashes.RemoveAt(index);
            instance.texCache.RemoveAt(index);
        }

        public static void DiscardAllTextures()
        {
            // reverse loop
            for (int i = instance.texCache.Count - 1; i >= 0; i--)
            {
                var tex = instance.texCache[i];
                DestroyImmediate(tex);
            }
            instance.texHashes.Clear();
            instance.texCache.Clear();
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
            DiscardAllTextures();
            instance.Save(true);
        }
    }
}