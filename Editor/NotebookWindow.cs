using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UnityNotebook
{
    public class NotebookWindow : EditorWindow
    {
        [MenuItem("Window/Notebook")]
        public static NotebookWindow Init()
        {
            var wnd = GetWindow<NotebookWindow>();
            wnd.titleContent = new GUIContent("Notebook");
            return wnd;
        }

        private static string PackagePath => UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).assetPath;
        
        private static bool _openExternally;

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
            var wnd = Init();
            NBState.OpenedNotebook = notebook;
            return true;
        }

        private static void ChangeNotebook(Notebook notebook)
        {
            if (notebook == NBState.OpenedNotebook)
            {
                return;
            }
            SaveJson();
            NBState.instance.Clear();
            NBState.OpenedNotebook = notebook;
        }

        private static void SaveScriptableObject()
        {
            var nb = NBState.OpenedNotebook;
            if (nb != null)
            {
                nb.SaveScriptableObject();
            }
        }
        
        private static void SaveJson()
        {
            var nb = NBState.OpenedNotebook;
            if (nb != null)
            {
                nb.SaveJson();
            }
        }

        private void OnEnable()
        {
            ChangeNotebook(NBState.OpenedNotebook);
            EditorApplication.update += DoRepaint;
        }

        private void OnDisable()
        {
            Evaluator.Stop();
            EditorApplication.update -= DoRepaint;
            SaveJson();
        }

        private int _lastKeyboardControl;

        private static GUIStyle _textStyle;
        private static GUIStyle _textStyleNoBackground;
        private static GUIStyle _codeStyle;
        private static GUIStyle _codeStyleNoBackground;
        private static GUIStyle _cellBox;
        private static GUIStyle _cellBoxSelected;
        private static GUIStyle _codeCellBox;
        private static GUIStyle _codeCellBoxSelected;
        
        private static void BuildStyles()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(EditorStyles.textField)
                {
                    richText = true,
                    wordWrap = true,
                    stretchHeight = false,
                    stretchWidth = true,
                    padding = new RectOffset(4, 4, 4, 4),
                };
            }

            if (_textStyleNoBackground == null)
            {
                _textStyleNoBackground = new GUIStyle()
                {
                    fontStyle = _textStyle.fontStyle,
                    fontSize = _textStyle.fontSize,
                    normal = _textStyle.normal,
                    active = _textStyle.active,
                    focused = _textStyle.focused,
                    hover = _textStyle.hover,
                    padding = _textStyle.padding,
                    margin = _textStyle.margin,
                    wordWrap = _textStyle.wordWrap,
                    clipping = _textStyle.clipping,
                    stretchHeight = _textStyle.stretchHeight,
                    stretchWidth = _textStyle.stretchWidth,
                    font = _textStyle.font,
                    richText = true
                };
            }

            if (_codeStyle == null)
            {
                var fontAsset = AssetDatabase.LoadAssetAtPath<Font>($"{PackagePath}/Assets/Menlo-Regular.ttf");
                if (fontAsset == null)
                {
                    Debug.LogError("Failed to load code editor font");
                }

                _codeStyle = new GUIStyle(GUI.skin.textArea)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    stretchHeight = false,
                    stretchWidth = true,
                    font = fontAsset,
                    richText = true
                };
            }

            if (_codeStyleNoBackground == null)
            {
                _codeStyleNoBackground = new GUIStyle()
                {
                    fontStyle = _codeStyle.fontStyle,
                    fontSize = _codeStyle.fontSize,
                    normal = _codeStyle.normal,
                    active = _codeStyle.active,
                    focused = _codeStyle.focused,
                    hover = _codeStyle.hover,
                    padding = _codeStyle.padding,
                    margin = _codeStyle.margin,
                    wordWrap = _codeStyle.wordWrap,
                    clipping = _codeStyle.clipping,
                    stretchHeight = _codeStyle.stretchHeight,
                    stretchWidth = _codeStyle.stretchWidth,
                    font = _codeStyle.font,
                    richText = true
                };
            }
            
            if (_cellBox == null)
            {
                _cellBox = new GUIStyle("box");
                _cellBox.padding = new RectOffset(8, 8, 8, 8);
            }

            if (_cellBoxSelected == null)
            {
                Texture2D BuildTexture(Color color)
                {
                    const int width = 64;
                    const int height = 64;
                    var pixels = new Color[width * height];
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = color;
                    }
                    var result = new Texture2D(width, height);
                    result.SetPixels(pixels);
                    result.Apply();
                    return result;
                }
                _cellBoxSelected = new GUIStyle(_cellBox)
                {
                    normal = new GUIStyleState()
                    {
                        background = BuildTexture(new Color(0.23f, 0.29f, 0.37f)),
                    }
                };
            }
            
            if (_codeCellBox == null)
            {
                _codeCellBox = new GUIStyle(_cellBox);
                _codeCellBox.padding = new RectOffset(8, 8, 8, 1);
            }
            if (_codeCellBoxSelected == null)
            {
                _codeCellBoxSelected = new GUIStyle(_cellBoxSelected);
                _codeCellBoxSelected.padding = new RectOffset(8, 8, 8, 1);
            }
        }

        private void OnGUI()
        {
            // Save the asset when moving between fields
            if (GUIUtility.keyboardControl != _lastKeyboardControl)
            {
                _lastKeyboardControl = GUIUtility.keyboardControl;
                SaveScriptableObject();
            }
            
            BuildStyles();

            DrawToolbar();

            if (NBState.OpenedNotebook == null)
            {
                DrawCreateNotebook();
            }
            else
            {
                DrawNotebook(NBState.OpenedNotebook);
            }
        }

        private void DrawCreateNotebook()
        {
            var buttonWidth = 200;
            var buttonHeight = 50;
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

                    var nb = Notebook.CreateAsset(path);
                    AssetDatabase.Refresh();
                    EditorGUIUtility.PingObject(nb);
                    nb.cells.Add(new Notebook.Cell {cellType = Notebook.CellType.Code});
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

        private void DrawNotebookAssetsPopup(Rect rect)
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

        private void DrawNotebookSelector()
        {
            var nb = NBState.OpenedNotebook;
            EditorGUI.BeginChangeCheck();
            var newNotebook = EditorGUILayout.ObjectField(nb, typeof(Notebook), true) as Notebook;
            if (EditorGUI.EndChangeCheck())
            {
                ChangeNotebook(newNotebook);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

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
                        Evaluator.ExecuteAllCells(nb, Repaint);
                    }
                }

                using (new EditorGUI.DisabledScope(nb == null || IsRunning))
                {
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                    {
                        Undo.RecordObject(nb, "Clear All Output");
                        nb.ClearOutputs();
                        SaveScriptableObject();
                    }

                    using (new EditorGUI.DisabledScope(nb != null && NBState.instance.scriptState == null))
                    {
                        if (GUILayout.Button("Restart", EditorStyles.toolbarButton))
                        {
                            NBState.instance.scriptState = null;
                        }
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                    {
                        SaveJson();
                    }
                    if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                    {
                        _openExternally = true;
                        AssetDatabase.OpenAsset(nb);
                        _openExternally = false;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(nb != null && IsRunning))
            {
                GUILayout.FlexibleSpace();
                DrawNotebookSelector();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DoRepaint()
        {
            if (NBState.RunningCell == -1)
            {
                return;
            }

            Repaint();
        }
        
        static bool consumeReturnKey;

        private void DrawNotebook(Notebook notebook)
        {
            if (consumeReturnKey && Event.current.type == EventType.KeyUp)
            {
                consumeReturnKey = false;
            }
            if (Event.current.isKey && consumeReturnKey && Event.current.character == '\n')
            {
                Event.current.Use();
            }

            if (notebook == null)
            {
                return;
            }
            
            // Keyboard shortcuts
            if (Event.current.type == EventType.KeyDown)
            {
                var selectedCell = NBState.SelectedCell;
                var isEditMode = NBState.IsEditMode;
                bool flag = false;
                
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                        // run cell
                        if (Event.current.control)
                        {
                            Evaluator.ExecuteCell(notebook, selectedCell);
                        }
                        // execute cell, select next
                        if (Event.current.shift)
                        {
                            Evaluator.ExecuteCell(notebook, selectedCell);
                            if (selectedCell < notebook.cells.Count - 1)
                            {
                                NBState.SelectedCell = selectedCell + 1;
                            }
                            flag = true;
                            // consumeReturnKey = true;
                        }
                        // execute and add cell
                        else if (Event.current.alt)
                        {
                            GUI.FocusControl(null);
                            Evaluator.ExecuteCell(notebook, selectedCell);
                            var newCell = new Notebook.Cell { cellType = notebook.cells[selectedCell].cellType };
                            notebook.cells.Insert(selectedCell + 1, newCell);
                            NBState.SelectedCell = selectedCell + 1;
                            flag = true;
                            // consumeReturnKey = true;
                        }
                        // enter edit mode
                        else if (!NBState.IsEditMode)
                        {
                            NBState.IsEditMode = true;
                            consumeReturnKey = true;
                        }
                        break;
                    // enter edit mode
                    case KeyCode.Q when !isEditMode:
                    case KeyCode.Escape when !isEditMode:
                        NBState.IsEditMode = true;
                        flag = true;
                        break;
                    // enter command mode
                    case KeyCode.Escape:
                    case KeyCode.M when Event.current.control && isEditMode:
                        GUI.FocusControl(null);
                        NBState.IsEditMode = false;
                        flag = true;
                        break;
                    // select next cell
                    case KeyCode.J when !isEditMode && selectedCell < notebook.cells.Count - 1:
                    case KeyCode.DownArrow when !isEditMode && selectedCell < notebook.cells.Count - 1:
                        NBState.SelectedCell += 1;
                        flag = true;
                        break;
                    // select previous cell
                    case KeyCode.K when !isEditMode && selectedCell > 0:
                    case KeyCode.UpArrow when !isEditMode && selectedCell > 0:
                        NBState.SelectedCell -= 1;
                        flag = true;
                        break;
                    // delete current empty cell
                    case KeyCode.Backspace when isEditMode && (notebook.cells[selectedCell].source.Length == 0 || notebook.cells[selectedCell].source[0].Length == 0):
                    case KeyCode.Delete when !isEditMode:
                        Undo.RecordObject(notebook, "Delete Cell");
                        notebook.cells.RemoveAt(selectedCell);
                        NBState.SelectedCell = Mathf.Max(0, selectedCell - 1);
                        flag = true;
                        break;
                    // add a cell below
                    case KeyCode.B when !isEditMode:
                        Undo.RecordObject(notebook, "Add Cell Below");
                        var c = new Notebook.Cell { cellType = Notebook.CellType.Code };
                        notebook.cells.Insert(selectedCell + 1, c);
                        NBState.SelectedCell = selectedCell + 1;
                        flag = true;
                        break;
                    // add cell above
                    case KeyCode.A when !isEditMode:
                        Undo.RecordObject(notebook, "Add Cell Above");
                        var c2 = new Notebook.Cell { cellType = Notebook.CellType.Code };
                        notebook.cells.Insert(selectedCell, c2);
                        flag = true;
                        break;
                    // merge cell below
                    case KeyCode.M when Event.current.shift && !isEditMode:
                        if (selectedCell < notebook.cells.Count - 1)
                        {
                            Undo.RecordObject(notebook, "Merge Cell Below");
                            // add newline to last line of current cell
                            var count = notebook.cells[selectedCell].source.Length - 1;
                            var lastLine = notebook.cells[selectedCell].source[count];
                            if (lastLine.Length > 0 && lastLine[^1] != '\n')
                            {
                                notebook.cells[selectedCell].source[count] += "\n";
                            }
                            // merge the cells
                            notebook.cells[selectedCell].source = notebook.cells[selectedCell].source.Concat(notebook.cells[selectedCell + 1].source).ToArray();
                            notebook.cells[selectedCell].rawText = string.Join("", notebook.cells[selectedCell].source);
                            notebook.cells.RemoveAt(selectedCell + 1);
                            flag = true;
                        }
                        break;
                    // split cell
                    case KeyCode.Minus when Event.current.control && Event.current.shift && isEditMode:
                        // Undo.RecordObject(notebook, "Split Cell");
                        var cell = notebook.cells[selectedCell];
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        var cursorIndex = editor.selectIndex;
                        // TODO copy the raw text after the cursorIndex to a new cell and delete the previous characters from the current cell
                        // then update the source array to match the rawText
                        Debug.Log("split not implemented");
                        break;
                    // set header
                    case KeyCode.Alpha1 when !isEditMode:
                        SetTextCellHeaderLevel(1);
                        flag = true;
                        break;
                    case KeyCode.Alpha2 when !isEditMode:
                        SetTextCellHeaderLevel(2);
                        flag = true;
                        break;
                    case KeyCode.Alpha3 when !isEditMode:
                        SetTextCellHeaderLevel(3);
                        flag = true;
                        break;
                    case KeyCode.Alpha4 when !isEditMode:
                        SetTextCellHeaderLevel(4);
                        flag = true;
                        break;
                    case KeyCode.Alpha5 when !isEditMode:
                        SetTextCellHeaderLevel(5);
                        flag = true;
                        break;

                    case KeyCode.M when !isEditMode:
                        Undo.RecordObject(notebook, "Change Cell Type");
                        notebook.cells[selectedCell].cellType = Notebook.CellType.Markdown;
                        flag = true;
                        break;
                    case KeyCode.Y when !isEditMode:
                        Undo.RecordObject(notebook, "Change Cell Type");
                        notebook.cells[selectedCell].cellType = Notebook.CellType.Code;
                        flag = true;
                        break;
                }
                if (flag)
                {
                    Event.current.Use();
                    Repaint();
                }
            }

            NBState.Scroll = EditorGUILayout.BeginScrollView(NBState.Scroll, false, false);

            EditorGUILayout.Space(2);

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
                    if (DrawCellToolbar(notebook, cellIndex, headerRect))
                    {
                        // cells were modified, break out of the draw loop
                        break;
                    }

                    DrawCell(notebook, cellIndex);
                }

                cellIndex++;
            } while (cellIndex < cellCount + 1);

            EditorGUILayout.EndScrollView();
        }

        private static void SetTextCellHeaderLevel(int level)
        {
            var notebook = NBState.OpenedNotebook;
            var cell = NBState.SelectedCell;
            if (notebook.cells[cell].cellType != Notebook.CellType.Markdown)
            {
                return;
            }
            var lines = notebook.cells[cell].source;
            if (lines.Length == 0)
            {
                return;
            }
            var newFirstLine = Regex.Replace(lines[0], @"^#{1,5}\s*", "");
            lines[0] = $"{new string('#', level)} {newFirstLine}";
        }

        private static bool DrawAddCellButtons(Notebook notebook, int cellIndex, out Rect rect)
        {
            using (new EditorGUI.DisabledScope(IsRunning))
            {
                // Add cell buttons
                rect = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
                var buttonRect = new Rect(rect.x + rect.width / 2 - 60, rect.y - 2, 60, 16);
                if (GUI.Button(buttonRect, "+ Code", EditorStyles.toolbarButton))
                {
                    Undo.RecordObject(notebook, "Add Code Cell");
                    notebook.cells.Insert(cellIndex, new Notebook.Cell {cellType = Notebook.CellType.Code});
                    return true;
                }

                buttonRect.x += 60;
                if (GUI.Button(buttonRect, "+ Text", EditorStyles.toolbarButton))
                {
                    Undo.RecordObject(notebook, "Add Text Cell");
                    notebook.cells.Insert(cellIndex, new Notebook.Cell {cellType = Notebook.CellType.Markdown});
                    return true;
                }

                return false;
            }
        }

        private static bool DrawCellToolbar(Notebook notebook, int cellIndex, Rect rect)
        {
            using (new EditorGUI.DisabledScope(IsRunning))
            {
                // Cell toolbar
                var toolbarRect = new Rect(rect.x + rect.width - 94, rect.y + 2, 90, 20);
                toolbarRect.width = 30;
                using (new EditorGUI.DisabledScope(cellIndex == 0))
                {
                    if (GUI.Button(toolbarRect, "▲", EditorStyles.miniButtonLeft))
                    {
                        Undo.RecordObject(notebook, "Move Cell Up");
                        notebook.cells.Insert(cellIndex - 1, notebook.cells[cellIndex]);
                        notebook.cells.RemoveAt(cellIndex + 1);
                        return true;
                    }
                }

                toolbarRect.x += 30;
                using (new EditorGUI.DisabledScope(cellIndex == notebook.cells.Count - 1))
                {
                    if (GUI.Button(toolbarRect, "▼", EditorStyles.miniButtonMid))
                    {
                        Undo.RecordObject(notebook, "Move Cell Down");
                        notebook.cells.Insert(cellIndex + 2, notebook.cells[cellIndex]);
                        notebook.cells.RemoveAt(cellIndex);
                        return true;
                    }
                }

                toolbarRect.x += 30;
                if (GUI.Button(toolbarRect, "✕", EditorStyles.miniButtonRight))
                {
                    Undo.RecordObject(notebook, "Delete Cell");
                    notebook.cells.RemoveAt(cellIndex);
                    return true;
                }

                return false;
            }
        }

        private void DrawCell(Notebook notebook, int cell)
        {
            if (cell >= notebook.cells.Count)
            {
                return;
            }
            
            switch (notebook.cells[cell].cellType)
            {
                case Notebook.CellType.Code:
                    GUILayout.BeginVertical(NBState.SelectedCell == cell ? _codeCellBoxSelected : _codeCellBox);
                    DrawCodeCell(notebook, cell);
                    GUILayout.EndVertical();
                    break;
                case Notebook.CellType.Markdown:
                    GUILayout.BeginVertical(NBState.SelectedCell == cell ? _cellBoxSelected : _cellBox);
                    DrawTextCell(notebook, cell);
                    GUILayout.EndVertical();
                    break;
            }

            // detect a click inside of the box
            var cellRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
            {
                NBState.SelectedCell = cell;
                GUI.FocusControl(null);
                Repaint();
            }
        }

        private static void DrawTextCell(Notebook notebook, int cell)
        {
            if (!NBState.IsEditMode || NBState.SelectedCell != cell)
            {
                Markdown.Draw(notebook.cells[cell].source);
                return;
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            notebook.cells[cell].rawText = notebook.cells[cell].source == null ? string.Empty : string.Concat(notebook.cells[cell].source);
            GUI.SetNextControlName(CellInputControlName);
            notebook.cells[cell].rawText = EditorGUILayout.TextArea(notebook.cells[cell].rawText, _textStyle);
            UpdateFocusAndMode(cell);
            TryUpdateCellSource(notebook.cells[cell]);
            GUILayout.EndHorizontal();
        }

        private static void DrawCodeCell(Notebook notebook, int cell)
        {
            if (cell >= notebook.cells.Count)
            {
                return;
            }
            
            GUILayout.BeginHorizontal();
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

            GUILayout.BeginVertical();

            var c = notebook.cells[cell];
            c.rawText ??= string.Concat(c.source);
            var syntaxTheme = EditorGUIUtility.isProSkin ? SyntaxHighlighting.Theme.Dark : SyntaxHighlighting.Theme.Light;
            // a horizontal scroll view
            var height = (c.source.Length == 0 ? 1 : c.source.Length) * 14 + 28;
            c.scroll = GUILayout.BeginScrollView(c.scroll, false, false, GUILayout.Height(height));
            GUI.SetNextControlName(CellInputControlName);
            CodeArea.Draw(ref c.rawText, ref c.highlightedText, syntaxTheme, _codeStyle);

            UpdateFocusAndMode(cell);

            GUILayout.EndScrollView();
            // split code area's text into separate lines to store in scriptable object
            TryUpdateCellSource(c);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (c.outputs.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Undo.RecordObject(notebook, "Clear Output");
                    c.outputs.Clear();
                }

                GUILayout.BeginVertical();
                DrawOutput(c);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private const string CellInputControlName = "CellInputField";
        private static void UpdateFocusAndMode(int cell)
        {
            if (cell == NBState.SelectedCell && NBState.IsEditMode)
            {
                GUI.FocusControl(CellInputControlName);
            }
            else if (GUI.GetNameOfFocusedControl() == CellInputControlName)
            {
                NBState.IsEditMode = true;
                NBState.SelectedCell = cell;
            }
        }

        private static void TryUpdateCellSource(Notebook.Cell c)
        {
            // split code area's text into separate lines to store in scriptable object
            if (GUI.changed)
            {
                c.source = c.rawText.Split('\n');

                // add stripped newline char back onto each line
                for (var i = 0; i < c.source.Length; i++)
                {
                    if (i < c.source.Length - 1)
                    {
                        c.source[i] += '\n';
                    }
                }

                GUI.changed = false;
            }
        }

        private static void DrawOutput(Notebook.Cell cell)
        {
            foreach (var output in cell.outputs)
            {
                switch (output.outputType)
                {
                    case Notebook.OutputType.Stream:
                        EditorGUILayout.TextArea(string.Concat(output.text), _textStyleNoBackground);
                        break;
                    case Notebook.OutputType.ExecuteResult:
                        foreach (var data in output.data)
                        {
                            EditorGUILayout.TextArea(string.Concat(data.stringData), _textStyleNoBackground);
                        }
                        break;
                    case Notebook.OutputType.DisplayData:
                        foreach (var data in output.data)
                        {
                            var renderer = Renderers.GetRendererForMimeType(data.mimeType);
                            renderer.Render(data);
                        }
                        break;
                    // TODO parse terminal control codes, set colors
                    case Notebook.OutputType.Error:
                        var c = GUI.color;
                        GUI.color = Color.red;
                        GUILayout.Label(output.ename);
                        GUILayout.Label(output.evalue);
                        GUI.color = c;
                        // TODO convert ANSI escape sequences to HTML tags
                        var str = string.Join("\n", output.traceback);
                        output.scroll = GUILayout.BeginScrollView(output.scroll);
                        EditorGUILayout.TextArea(str, _codeStyleNoBackground, GUILayout.ExpandHeight(false));
                        GUILayout.EndScrollView();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}