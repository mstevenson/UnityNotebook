using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MG.MDV;
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

        // references are set in this script's inspector
        public GUISkin MarkdownSkinLight;
        public GUISkin MarkdownSkinDark;

        private static bool _openExternally;

        private static bool IsRunning => NotebookWindowData.RunningCell >= 0;

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
            NotebookWindowData.OpenedNotebook = notebook;
            return true;
        }

        private static void ChangeNotebook(Notebook notebook)
        {
            if (notebook == NotebookWindowData.OpenedNotebook)
            {
                return;
            }
            SaveNotebookAsset();
            NotebookWindowData.instance.Clear();
            NotebookWindowData.OpenedNotebook = notebook;
        }

        private static void SaveNotebookAsset()
        {
            var nb = NotebookWindowData.OpenedNotebook;
            if (nb != null)
            {
                nb.SaveAsset();
            }
        }

        private void OnEnable()
        {
            ChangeNotebook(NotebookWindowData.OpenedNotebook);
            EditorApplication.update += DoRepaint;
        }

        private void OnDisable()
        {
            Evaluator.Stop();
            EditorApplication.update -= DoRepaint;
            SaveNotebookAsset();
        }

        private int _lastKeyboardControl;

        private static GUIStyle _textStyle;
        private static GUIStyle _textStyleNoBackground;
        private static GUIStyle _codeStyle;
        private static GUIStyle _codeStyleNoBackground;
        private static GUIStyle _cellBox;
        private static GUIStyle _cellBoxSelected;
        
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
        }

        private void OnGUI()
        {
            // Save the asset when moving between fields
            if (GUIUtility.keyboardControl != _lastKeyboardControl)
            {
                _lastKeyboardControl = GUIUtility.keyboardControl;
                SaveNotebookAsset();
            }
            
            BuildStyles();

            DrawToolbar();

            if (NotebookWindowData.OpenedNotebook == null)
            {
                DrawCreateNotebook();
            }
            else
            {
                DrawNotebook(NotebookWindowData.OpenedNotebook);
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
                    NotebookWindowData.OpenedNotebook = nb;
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
            var nb = NotebookWindowData.OpenedNotebook;
            EditorGUI.BeginChangeCheck();
            var newNotebook = EditorGUILayout.ObjectField(nb, typeof(Notebook), true) as Notebook;
            if (EditorGUI.EndChangeCheck())
            {
                ChangeNotebook(newNotebook);
                // _caretPos = 0;
                if (nb == null)
                {
                    return;
                }

                // get asset path
                var path = AssetDatabase.GetAssetPath(nb);
                foreach (var cell in nb.cells)
                {
                    // TODO markdown
                    // if (cell.cellType == Notebook.CellType.Markdown)
                    // {
                    //     cell.markdownViewer = new MarkdownViewer(EditorGUIUtility.isProSkin ? MarkdownSkinDark : MarkdownSkinLight, path, cell.textBlock);
                    // }
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var nb = NotebookWindowData.OpenedNotebook;
            
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
                        Evaluator.ExecuteAll(nb);
                    }
                }

                using (new EditorGUI.DisabledScope(nb == null || IsRunning))
                {
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                    {
                        Undo.RecordObject(nb, "Clear All Output");
                        nb.ClearOutputs();
                        SaveNotebookAsset();
                    }

                    using (new EditorGUI.DisabledScope(nb != null && nb.scriptState == null))
                    {
                        if (GUILayout.Button("Restart", EditorStyles.toolbarButton))
                        {
                            nb.scriptState = null;
                        }
                    }

                    EditorGUILayout.Space();

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
            if (NotebookWindowData.RunningCell == -1)
            {
                return;
            }

            Repaint();
        }

        private void DrawNotebook(Notebook notebook)
        {
            if (notebook == null)
            {
                return;
            }

            NotebookWindowData.Scroll = EditorGUILayout.BeginScrollView(NotebookWindowData.Scroll, false, false);

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
            
            GUILayout.BeginVertical(NotebookWindowData.SelectedCell == cell ? _cellBoxSelected : _cellBox);
            switch (notebook.cells[cell].cellType)
            {
                case Notebook.CellType.Code:
                    DrawCodeCell(notebook, cell);
                    break;
                case Notebook.CellType.Markdown:
                    DrawTextCell(notebook.cells[cell]);
                    break;
            }
            GUILayout.EndVertical();

            // detect a click inside of the box
            var cellRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
            {
                NotebookWindowData.SelectedCell = cell;
                GUI.FocusControl(null);
                Repaint();
            }

            // arrow key navigation
            if (cell == NotebookWindowData.SelectedCell && Event.current.type == EventType.KeyDown)
            {
                bool flag = false;
                switch (Event.current.keyCode)
                {
                    case KeyCode.DownArrow when cell < notebook.cells.Count - 1:
                        NotebookWindowData.SelectedCell += 1;
                        flag = true;
                        break;
                    case KeyCode.UpArrow when cell > 0:
                        NotebookWindowData.SelectedCell -= 1;
                        flag = true;
                        break;
                }
                if (flag)
                {
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        private static void DrawTextCell(Notebook.Cell cell)
        {
            // TODO draw markdown
            // cell.markdownViewer.Draw();
            cell.rawText = cell.source == null ? string.Empty : string.Concat(cell.source);
            cell.rawText = EditorGUILayout.TextArea(cell.rawText, _textStyle);
            // split code area's text into separate lines to store in scriptable object
            TryUpdateCellSource(cell);
        }

        private void DrawCodeCell(Notebook notebook, int cell)
        {
            if (cell >= notebook.cells.Count)
            {
                return;
            }
            
            GUILayout.BeginHorizontal();
            if (IsRunning && NotebookWindowData.RunningCell == cell)
            {
                if (GUILayout.Button("■", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Evaluator.Stop();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(NotebookWindowData.RunningCell != -1))
                {
                    if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        GUI.FocusControl(null);
                        Evaluator.ExecuteCell(notebook, cell);
                    }
                }
            }
            
            if (cell == NotebookWindowData.SelectedCell)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    // if current cell source is empty, delete it
                    if (Event.current.keyCode == KeyCode.Backspace && (notebook.cells[cell].source.Length == 0 || notebook.cells[cell].source[0].Length == 0))
                    {
                        Undo.RecordObject(notebook, "Delete Cell");
                        notebook.cells.RemoveAt(cell);
                        NotebookWindowData.SelectedCell = Mathf.Max(0, cell - 1);
                        Event.current.Use();
                        Repaint();
                    }
                    else if (Event.current.keyCode == KeyCode.Return)
                    {
                        // execute current cell
                        if (Event.current.shift)
                        {
                            Evaluator.ExecuteCell(notebook, cell);
                            Event.current.Use();
                        }
                        // execute and add new cell
                        else if (Event.current.alt)
                        {
                            GUI.FocusControl(null);
                            Evaluator.ExecuteCell(notebook, cell);
                            var newCell = new Notebook.Cell { cellType = notebook.cells[cell].cellType };
                            notebook.cells.Insert(cell + 1, newCell);
                            NotebookWindowData.SelectedCell = cell + 1;
                            Event.current.Use();
                            Repaint();
                        }
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        GUI.FocusControl(null);
                        Event.current.Use();
                    }
                }
            }

            GUILayout.BeginVertical();

            var c = notebook.cells[cell];
            c.rawText ??= string.Concat(c.source);
            var syntaxTheme = EditorGUIUtility.isProSkin ? SyntaxTheme.Dark : SyntaxTheme.Light;
            // a horizontal scroll view
            c.scroll = GUILayout.BeginScrollView(c.scroll, false, false, GUILayout.ExpandHeight(false));
            GUI.SetNextControlName("CodeArea");
            CodeArea.Draw(ref c.rawText, ref c.highlightedText, syntaxTheme, _codeStyle);
            // press enter to switch from command mode to edit mode
            if (cell == NotebookWindowData.SelectedCell &&
                GUI.GetNameOfFocusedControl() != "CodeArea" &&
                Event.current.type == EventType.KeyDown &&
                !(Event.current.shift || Event.current.alt) &&
                Event.current.character == '\n')
            {
                GUI.FocusControl("CodeArea");
                Event.current.Use();
            }
            if (GUI.GetNameOfFocusedControl() == "CodeArea")
            {
                NotebookWindowData.SelectedCell = cell;
            }
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