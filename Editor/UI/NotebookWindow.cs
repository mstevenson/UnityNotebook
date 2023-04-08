using System;
using System.Collections.Generic;
using System.IO;
using AICommand;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using static UnityNotebook.Styles;

namespace UnityNotebook
{
    public class NotebookWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Notebook")]
        public static void Init()
        {
            var wnd = GetWindow<NotebookWindow>();
            wnd.titleContent = new GUIContent("Notebook");
        }
        
        // Automatically called by the editor to build a context menu for the window
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Split Cell"), false, () => Commands.SplitCell());
            menu.AddItem(new GUIContent("Merge Cell Below"), false, () => Commands.MergeCellBelow());
            menu.AddItem(new GUIContent("Merge Cell Above"), false, () => Commands.MergeCellAbove());
            menu.AddItem(new GUIContent("Convert to Markdown"), false, Commands.ConvertCellToMarkdown);
            menu.AddItem(new GUIContent("Convert to Code"), false, Commands.ConvertCellToCode);
            menu.AddSeparator(null);
            menu.AddItem(new GUIContent("Reset Tool State"), false, NBState.Reset);
        }
        
        private static bool _openExternally;
        private static double _cooldownStartTime = double.PositiveInfinity;
        private const float SaveCooldownDuration = 1.5f;
        private static bool _runningSaveCooldown;

        private static bool IsRunning => NBState.RunningCell >= 0;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (_openExternally)
            {
                return false;
            }

            var target = EditorUtility.InstanceIDToObject(instanceID);
            if (target is not Notebook)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(instanceID);
            if (Path.GetExtension(path) != ".ipynb")
            {
                return false;
            }

            var notebook = AssetDatabase.LoadAssetAtPath<Notebook>(path);
            Init();
            NBState.OpenedNotebook = notebook;
            return true;
        }
        
        [MenuItem("Assets/Create/Notebook", false, 80)]
        public static void CreateNotebookAssetMenu()
        {
            // Get the path to the currently selected folder
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            // Create asset
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/New Notebook.ipynb");
            var asset = Commands.CreateNotebookAsset(assetPath);
            Selection.activeObject = asset;
        }

        private static void ChangeNotebook(Notebook notebook)
        {
            if (notebook == NBState.OpenedNotebook)
            {
                return;
            }
            NBState.SaveJson();
            NBState.CloseNotebook();
            NBState.OpenedNotebook = notebook;
        }
        
        private void OnEnable()
        {
            ChangeNotebook(NBState.OpenedNotebook);
            EditorApplication.update += OnUpdate;
            Undo.undoRedoPerformed += OnUpdate;
        }
        
        private void OnDisable()
        {
            Evaluator.Stop();
            EditorApplication.update -= OnUpdate;
            Undo.undoRedoPerformed -= OnUpdate;
        }
        
        private void OnDestroy()
        {
            NBState.SaveJson();
            NBState.Reset();
        }

        private int _lastKeyboardControl;

        private void OnGUI()
        {
            Styles.Init();

            if (Event.current.isKey && NBState.OpenedNotebook != null)
            {
                // TODO undo doesn't work with this, need to use TextEditor's undo?
                if (!_runningSaveCooldown)
                {
                //     Debug.Log("record undo");
                //     Undo.RecordObject(NBState.OpenedNotebook, "Notebook Cell Text");
                }
                _runningSaveCooldown = true;
                _cooldownStartTime = EditorApplication.timeSinceStartup;
            }
            
            // Save the asset when moving between fields
            if (GUIUtility.keyboardControl != _lastKeyboardControl)
            {
                _lastKeyboardControl = GUIUtility.keyboardControl;
                NBState.SaveScriptableObject();
            }

            DrawWindowToolbar();

            if (NBState.OpenedNotebook == null)
            {
                DrawCreateNotebook();
            }
            else
            {
                DrawNotebook(NBState.OpenedNotebook);
            }

            // HACK
            NBState.instance.forceSyntaxRefresh = false;
        }

        private void DrawCreateNotebook()
        {
            const int buttonWidth = 200;
            const int buttonHeight = 50;
            var buttonRect = new Rect(
                position.width / 2 - buttonWidth / 2,
                (position.height / 2 - buttonHeight / 2) - 35,
                buttonWidth,
                buttonHeight
            );
            
            if (GUI.Button(buttonRect, "Create Notebook"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Create Notebook", "Notebook", "ipynb",
                    "Create Notebook");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.EndsWith(".ipynb"))
                    {
                        path += ".ipynb";
                    }

                    var nb = Commands.CreateNotebookAsset(path);
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(nb);
                    nb.cells.Add(new Cell {cellType = CellType.Code});
                    NBState.OpenedNotebook = nb;
                }
            }

            buttonRect.y += buttonHeight + 10;
            if (GUI.Button(buttonRect, "Open Notebook"))
            {
                var path = EditorUtility.OpenFilePanel("Open Notebook", Application.dataPath, "ipynb");
                if (!string.IsNullOrEmpty(path))
                {
                    // Get assets-relative path
                    path = path.Replace(Application.dataPath, "Assets");
                    var nb = AssetDatabase.LoadAssetAtPath<Notebook>(path);
                    ChangeNotebook(nb);
                }
            }

            buttonRect.y += buttonHeight + 10;

            DrawNotebookAssetsPopup(buttonRect);
        }

        private static void DrawNotebookAssetsPopup(Rect rect)
        {
            var guids = AssetDatabase.FindAssets("t:Notebook");
            var notebooks = new List<Notebook>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                notebooks.Add(AssetDatabase.LoadAssetAtPath<Notebook>(path));
            }

            if (notebooks.Count == 0)
            {
                return;
            }

            notebooks.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            var notebookNames = new string[notebooks.Count];
            for (var i = 0; i < notebooks.Count; i++)
            {
                notebookNames[i] = AssetDatabase.GetAssetPath(notebooks[i]);
                // Print forward slashes instead of using them as submenu separators.
                // Replaces the slash with division character.
                notebookNames[i] = notebookNames[i].Replace("/", " \u2215 ");
            }

            var index = EditorGUI.Popup(rect, "Notebooks", -1, notebookNames);
            if (index >= 0)
            {
                ChangeNotebook(notebooks[index]);
            }
        }

        private void DrawWindowToolbar()
        {
            using var h = new EditorGUILayout.HorizontalScope(EditorStyles.toolbar);
            
            var nb = NBState.OpenedNotebook;
            using (new EditorGUI.DisabledScope(nb == null))
            {
                if (nb != null && IsRunning)
                {
                    if (GUILayout.Button("Stop", EditorStyles.toolbarButton, GUILayout.Width(40)))
                    {
                        Evaluator.Stop();
                    }
                }
                else
                {
                    if (GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Width(40)))
                    {
                        GUI.FocusControl(null);
                        // TODO repaint callback doesn't work reliably, the final cell output often isn't drawn
                        Evaluator.ExecuteAllCells(nb, () =>
                        {
                            NBState.SetNotebookDirty();
                            Repaint();
                        });
                    }
                }

                using (new EditorGUI.DisabledScope(nb == null || IsRunning))
                {
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                    {
                        Commands.ClearAllOutputs();
                    }

                    using (new EditorGUI.DisabledScope(nb != null && NBState.instance.scriptState == null))
                    {
                        if (GUILayout.Button("Restart", EditorStyles.toolbarButton))
                        {
                            NBState.instance.scriptState = null;
                        }
                    }
                    
                    EditorGUILayout.Space();

                    using (new EditorGUI.DisabledScope(!NBState.IsJsonOutOfDate))
                    {
                        if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                        {
                            NBState.SaveJson();
                        }
                    }
                    // Revert unsaved ScriptableObject changes by reimporting the json file
                    if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
                    {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(nb));
                        NBState.SaveScriptableObject();
                        NBState.IsJsonOutOfDate = false;
                    }
                    if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                    {
                        _openExternally = true;
                        AssetDatabase.OpenAsset(nb);
                        _openExternally = false;
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            
            // Notebook object field
            using (new EditorGUI.DisabledScope(IsRunning))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var newNotebook = EditorGUILayout.ObjectField(NBState.OpenedNotebook, typeof(Notebook), true) as Notebook;
                    if (check.changed)
                    {
                        ChangeNotebook(newNotebook);
                    }
                }
            }
        }

        private void OnUpdate()
        {
            if (NBState.OpenedNotebook != null)
            {
                if (EditorUtility.IsDirty(NBState.OpenedNotebook))
                {
                    NBState.IsJsonOutOfDate = true;
                }
            }
            
            // subtract editor deltatime
            if (_runningSaveCooldown && EditorApplication.timeSinceStartup - _cooldownStartTime > SaveCooldownDuration)
            {
                _runningSaveCooldown = false;
                NBState.SetNotebookDirty();
                // NBState.SaveScriptableObject();
                Repaint();
            }
            
            if (NBState.RunningCell != -1)
            {
                Repaint();
            }
        }

        private void DrawNotebook(Notebook notebook)
        {
            if (notebook == null)
            {
                return;
            }
            
            if (Shortcuts.ConsumeReturnKey && Event.current.type == EventType.KeyUp)
            {
                Shortcuts.ConsumeReturnKey = false;
            }
            if (Event.current.isKey && Shortcuts.ConsumeReturnKey && Event.current.character == '\n')
            {
                Event.current.Use();
            }

            // Keyboard shortcuts
            if (Shortcuts.HandleKeyboardShortcuts(notebook))
            {
                NBState.SetNotebookDirty();
                Event.current.Use();
                Repaint();
            }

            using var scrollView = new EditorGUILayout.ScrollViewScope(NBState.Scroll, false, false);
            NBState.Scroll = scrollView.scrollPosition;
            
            var cellCount = notebook.cells.Count;
            var cellIndex = 0;
            do
            {
                if (DrawAddCellButtons(notebook, cellIndex, out var headerRect))
                {
                    // added a cell, loop invalidated
                    break;
                }

                if (cellCount > 0 && cellIndex < cellCount)
                {
                    DrawCell(notebook, cellIndex);
                    
                    // called after drawing the cell so it displays in front of the cell
                    if (DrawCellToolbar(notebook, cellIndex, headerRect))
                    {
                        // cells were modified, break out of the draw loop
                        break;
                    }
                    
                    // detect a click inside of the box
                    var cellRect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        NBState.SelectedCell = cellIndex;
                        GUI.FocusControl(null);
                        Event.current.Use();
                        Repaint();
                    }
                }

                cellIndex++;
            } while (cellIndex < cellCount + 1);
        }
        
        private static bool DrawAddCellButtons(Notebook notebook, int cellIndex, out Rect rect)
        {
            using var _ = new EditorGUI.DisabledScope(IsRunning);
            
            // Add cell buttons
            rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            var buttonRect = new Rect(rect.x + rect.width / 2 - 60, rect.y, 60, 20);
                
            bool AddButton(string title, CellType type)
            {
                if (!GUI.Button(buttonRect, title, EditorStyles.toolbarButton))
                {
                    return false;
                }
                Commands.AddCell(type, notebook, cellIndex);
                Event.current.Use();
                return true;
            }

            var addCode = AddButton("+ Code", CellType.Code);
            buttonRect.x += 60;
            var addText = AddButton("+ Text", CellType.Markdown);
                
            return addCode || addText;
        }

        private static bool DrawCellToolbar(Notebook notebook, int cellIndex, Rect rect)
        {
            using var _ = new EditorGUI.DisabledScope(IsRunning);
            
            // Cell toolbar
            const int buttonWidth = 22;
            rect = new Rect(rect.x + rect.width - (buttonWidth * 3) - 10, rect.y + 13, buttonWidth, 16);
            using (new EditorGUI.DisabledScope(cellIndex == 0))
            {
                if (GUI.Button(rect, "▲", EditorStyles.miniButtonLeft))
                {
                    Commands.MoveCellUp(cellIndex);
                    Event.current.Use();
                    return true;
                }
            }

            rect.x += buttonWidth;
            using (new EditorGUI.DisabledScope(cellIndex == notebook.cells.Count - 1))
            {
                if (GUI.Button(rect, "▼", EditorStyles.miniButtonMid))
                {
                    Commands.MoveCellDown(cellIndex);
                    Event.current.Use();
                    return true;
                }
            }

            rect.x += buttonWidth;
            if (GUI.Button(rect, "✕", EditorStyles.miniButtonRight))
            {
                Commands.DeleteCell(cellIndex);
                Event.current.Use();
                return true;
            }

            return false;
        }

        private static void DrawCell(Notebook notebook, int cell)
        {
            if (cell >= notebook.cells.Count)
            {
                return;
            }
            
            switch (notebook.cells[cell].cellType)
            {
                case CellType.Code:
                    using (new GUILayout.VerticalScope(NBState.SelectedCell == cell
                               ? NBState.IsEditMode ? CodeCellBoxSelectedEditStyle : CodeCellBoxSelectedStyle
                               : CodeCellBoxStyle))
                    {
                        DrawCodeCell(notebook, cell);
                        DrawCodeCellOutput(notebook, cell);
                    }
                    break;
                case CellType.Markdown:
                    using (new GUILayout.VerticalScope(NBState.SelectedCell == cell
                               ? NBState.IsEditMode ? CellBoxSelectedEditStyle : CellBoxSelectedStyle
                               : CellBoxStyle))
                    {
                        DrawTextCell(notebook, cell);
                    }
                    break;
                case CellType.Raw:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawTextCell(Notebook notebook, int cell)
        {
            using var _ = new EditorGUILayout.HorizontalScope();
            
            // Run button
            if (IsRunning && NBState.RunningCell == cell)
            {
                if (GUILayout.Button("■", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    // TODO cancel web request?
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(NBState.RunningCell != -1))
                {
                    if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        if (!OpenAIUtil.ValidateApiKey)
                        {
                            Debug.LogError(OpenAIUtil.ApiKeyErrorText);
                        }
                        else
                        {
                            GUI.FocusControl(null);
                            var instructions = string.Concat(notebook.cells[cell].source);
                            var prompt = OpenAIUtil.GetPrompt(instructions);
                            var code = OpenAIUtil.InvokeChat(prompt);
                            code = OpenAIUtil.SanitizeResult(code);
                            Debug.Log(code);
                            var output = Evaluator.ExecuteCodeAsync(code).Result;
                            if (output != null)
                            {
                                notebook.cells[cell].outputs.Add(output);
                            }
                        }
                    }
                }
            }
            
            if (!NBState.IsEditMode || NBState.SelectedCell != cell)
            {
                Markdown.Draw(notebook.cells[cell].source);
                return;
            }
            
            notebook.cells[cell].rawText = notebook.cells[cell].source == null ? string.Empty : string.Concat(notebook.cells[cell].source);
            GUI.SetNextControlName(CellInputControlName);
            notebook.cells[cell].rawText = EditorGUILayout.TextArea(notebook.cells[cell].rawText, TextStyle);
            UpdateFocusAndMode(cell);
            TryUpdateCellSource(notebook.cells[cell]);
        }

        private static void DrawCodeCell(Notebook notebook, int cell)
        {
            using var _ = new EditorGUILayout.HorizontalScope();
            
            // Run button
            if (IsRunning && NBState.RunningCell == cell)
            {
                if (GUILayout.Button("■", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Evaluator.Stop();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(NBState.RunningCell != -1))
                {
                    if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        GUI.FocusControl(null);
                        Evaluator.ExecuteCell(notebook, cell);
                    }
                }
            }
            
            DrawCodeCellTextArea(notebook, cell);
        }

        private static void DrawCodeCellTextArea(Notebook notebook, int cell)
        {
            using var _ = new EditorGUILayout.VerticalScope();
            
            var c = notebook.cells[cell];
            c.rawText ??= string.Concat(c.source);
            var syntaxTheme = EditorGUIUtility.isProSkin ? SyntaxHighlighting.Theme.Dark : SyntaxHighlighting.Theme.Light;
            // a horizontal scroll view
            var height = (c.source.Length == 0 ? 1 : c.source.Length) * 14 + 28;
            c.scroll = GUILayout.BeginScrollView(c.scroll, false, false, GUILayout.Height(height));
            GUI.SetNextControlName(CellInputControlName);
            CodeArea.Draw(ref c.rawText, ref c.highlightedText, syntaxTheme, CodeStyle);

            UpdateFocusAndMode(cell);

            GUILayout.EndScrollView();
            // split code area's text into separate lines to store in scriptable object
            TryUpdateCellSource(c);
        }

        private static void DrawCodeCellOutput(Notebook notebook, int cell)
        {
            if (notebook.cells[cell].outputs.Count <= 0)
            {
                return;
            }
            using var _ = new EditorGUILayout.HorizontalScope();
            if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(20)))
            {
                Commands.ClearCellOutput(notebook.cells[cell]);
                NBState.SaveScriptableObject();
            }
            DrawOutput(notebook.cells[cell]);
        }

        private static void DrawOutput(Cell cell)
        {
            using var _ = new EditorGUILayout.VerticalScope();
            
            foreach (var output in cell.outputs)
            {
                switch (output)
                {
                    case CellOutputStream stream:
                        EditorGUILayout.TextArea(string.Concat(stream.text), TextNoBackgroundStyle);
                        break;
                    case CellOutputExecuteResults results:
                        var resultStr = results.backingValue.Object as string;
                        EditorGUILayout.TextArea(resultStr, TextNoBackgroundStyle);
                        break;
                    case CellOutputDisplayData display:
                        foreach (var value in display.values)
                        {
                            var type = value.Object.GetType();
                            var renderer = Renderers.GetRendererForType(type);
                            renderer.DrawGUI(value.Object);
                        }
                        break;
                    // TODO parse terminal control codes, set colors
                    case CellOutputError error:
                        var c = GUI.color;
                        GUI.color = new Color(1f, 0.68f, 0f);
                        GUILayout.Label(error.ename);
                        GUILayout.Label(error.evalue);
                        GUI.color = c;
                        // TODO convert ANSI escape sequences to HTML tags
                        var str = string.Join("\n", error.traceback);
                        output.scroll = GUILayout.BeginScrollView(output.scroll);
                        EditorGUILayout.TextArea(str, CodeNoBackgroundStyle, GUILayout.ExpandHeight(false));
                        GUILayout.EndScrollView();
                        break;
                    default:
                        Debug.LogWarning($"Unknown output type: {output.GetType()}");
                        break;
                }
            }
        }

        private const string CellInputControlName = "CellInputField";
        
        private static void UpdateFocusAndMode(int cell)
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }
            
            if (cell == NBState.SelectedCell && NBState.IsEditMode && NBState.instance.forceFocusCodeArea)
            {
                GUI.FocusControl(CellInputControlName);
                NBState.instance.forceFocusCodeArea = false;
            }
            else if (GUI.GetNameOfFocusedControl() == CellInputControlName)
            {
                NBState.IsEditMode = true;
                NBState.SelectedCell = cell;
            }
        }

        // When the cell text is changed via UI, update the cell's source lines
        private static void TryUpdateCellSource(Cell cell)
        {
            if (!GUI.changed)
            {
                return;
            }
            NBState.CopyRawTextToSourceLines(cell);
            GUI.changed = false;
        }
    }
}