using System;
using System.Collections.Generic;
using MG.MDV;
using UnityEditor;
using UnityEngine;

public class NotebookWindow : EditorWindow
{
    [MenuItem("Window/Notebook")]
    public static void ShowExample()
    {
        var wnd = GetWindow<NotebookWindow>();
        wnd.titleContent = new GUIContent("Notebook");
    }
    
    // references are set in this script's inspector
    public GUISkin MarkdownSkinLight;
    public GUISkin MarkdownSkinDark;

    private static GUIStyle _textStyle;
    private static GUIStyle _textStyleNoBackground;
    private static GUIStyle _codeStyle;
    private static GUIStyle _codeStyleNoBackground;
    private static bool _tabPressed;
    private static int _caretPos;
    
    private Notebook _notebook;

    private static Vector2 _scroll;

    private void OnGUI()
    {
        if (_textStyle == null)
        {
            _textStyle = new GUIStyle(EditorStyles.textField)
            {
                richText = true,
                wordWrap = true,
                stretchHeight = false,
                stretchWidth = true,
                padding = new RectOffset(8, 8, 8, 8)
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
                wordWrap = _textStyle.wordWrap,
                clipping = _textStyle.clipping,
                stretchHeight = _textStyle.stretchHeight,
                stretchWidth = _textStyle.stretchWidth,
                font = _textStyle.font,
                richText = _textStyle.richText
            };
        }
        if (_codeStyle == null)
        {
            _codeStyle = new GUIStyle(GUI.skin.textArea)
            {
                padding = new RectOffset(8, 8, 8, 8),
                wordWrap = false,
                clipping = TextClipping.Clip,
                stretchHeight = false,
                stretchWidth = true,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Menlo-Regular.ttf"),
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
                wordWrap = _codeStyle.wordWrap,
                clipping = _codeStyle.clipping,
                stretchHeight = _codeStyle.stretchHeight,
                stretchWidth = _codeStyle.stretchWidth,
                font = _codeStyle.font,
                richText = _codeStyle.richText
            };
        }
        
        DrawToolbar();
        
        if (_notebook == null)
        {
            DrawCreateNotebook();
        }
        else
        {
            DrawNotebook(_notebook);
        }
    }

    private void DrawCreateNotebook()
    {
        var buttonWidth = 200;
        var buttonHeight = 50;
        var buttonRect = new Rect(
            position.width / 2 - buttonWidth / 2,
            (position.height / 2 - buttonHeight / 2) - 20,
            buttonWidth,
            buttonHeight
        );
        if (GUI.Button(buttonRect, "Create Notebook"))
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Notebook", "Notebook", "ipynb", "Create Notebook");
            if (!path.EndsWith(".ipynb"))
            {
                path += ".ipynb";
            }
            Debug.Log(path);
        }
        buttonRect.y += buttonHeight + 10;
        if (GUI.Button(buttonRect, "Open Notebook"))
        {
            var path = EditorUtility.OpenFilePanel("Open Notebook", Application.dataPath, "ipynb");
            if (!string.IsNullOrEmpty(path))
            {
                // Get assets-relative path
                path = path.Replace(Application.dataPath, "Assets");
                _notebook = AssetDatabase.LoadAssetAtPath<Notebook>(path);
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
        }
        
        var index = EditorGUI.Popup(rect, "Notebooks", -1, notebookNames);
        if (index >= 0)
        {
            _notebook = notebooks[index];
        }
    }

    private void DrawNotebookSelector()
    {
        EditorGUI.BeginChangeCheck();
        _notebook = EditorGUILayout.ObjectField(_notebook, typeof(Notebook), true) as Notebook;
        if (EditorGUI.EndChangeCheck())
        {
            _caretPos = 0;
            if (_notebook == null)
            {
                return;
            }
            // get asset path
            var path = AssetDatabase.GetAssetPath(_notebook);
            foreach (var cell in _notebook.cells)
            {
                if (cell.cellType == Notebook.CellType.Markdown)
                {
                    cell.markdownViewer = new MarkdownViewer(EditorGUIUtility.isProSkin ? MarkdownSkinDark : MarkdownSkinLight, path, string.Join(null, cell.source));
                }
            }
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (Evaluator.IsRunning)
        {
            if (GUILayout.Button("Stop", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                Evaluator.Stop();
            }
        }
        if (_notebook == null)
        {
            GUI.enabled = false;
        }
        if (!Evaluator.IsRunning && GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Width(40)))
        {
            foreach (var cell in _notebook.cells)
            {
                if (cell.cellType == Notebook.CellType.Code)
                {
                    Evaluator.Evaluate(cell);
                }
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
        {
            Undo.RecordObject(_notebook, "Clear Output");
            foreach (var cell in _notebook.cells)
            {
                cell.outputs.Clear();
            }
        }
        if (GUILayout.Button("Restart", EditorStyles.toolbarButton))
        {
            Debug.Log("Restart");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
        {
            // var path = AssetDatabase.GetAssetPath(_notebook);
            AssetDatabase.OpenAsset(_notebook);
        }
        
        GUI.enabled = true;
        
        GUILayout.FlexibleSpace();
        DrawNotebookSelector();
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawNotebook(Notebook notebook)
    {
        if (notebook == null)
        {
            return;
        }
        
        _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false);
        
        var cellCount = notebook.cells.Count;
        var cellIndex = 0;
        do
        {
            if (DrawAddCellButtons(notebook, cellIndex, out var headerRect))
            {
                // added a cell, loop invalidated
                break;
            }
            if (cellCount > 0)
            {
                if (DrawCellToolbar(notebook, cellIndex, headerRect))
                {
                    // cells were modified, break out of the draw loop
                    break;
                }
                DrawCell(notebook.cells[cellIndex]);
            }
            cellIndex++;
        } while (cellIndex < cellCount);
        
        EditorGUILayout.EndScrollView();
    }

    private static bool DrawAddCellButtons(Notebook notebook, int cellIndex, out Rect rect)
    {
        // Add cell buttons
        rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        var buttonRect = new Rect(rect.x + rect.width / 2 - 85, rect.y, 80, 20);
        if (GUI.Button(buttonRect, "+ Code"))
        {
            Undo.RecordObject(notebook, "Add Code Cell");
            notebook.cells.Insert(cellIndex, new Notebook.Cell { cellType = Notebook.CellType.Code });
            return true;
        }
        buttonRect.x += 85;
        if (GUI.Button(buttonRect, "+ Text"))
        {
            Undo.RecordObject(notebook, "Add Text Cell");
            notebook.cells.Insert(cellIndex, new Notebook.Cell { cellType = Notebook.CellType.Markdown });
            return true;
        }
        return false;
    }

    private static bool DrawCellToolbar(Notebook notebook, int cellIndex, Rect rect)
    {
        // Cell toolbar
        var toolbarRect = new Rect(rect.x + rect.width - 95, rect.y+4, 90, 20);
        toolbarRect.width = 30;
        GUI.enabled = cellIndex > 0;
        if (GUI.Button(toolbarRect, "▲", EditorStyles.miniButtonLeft))
        {
            Undo.RecordObject(notebook, "Move Cell Up");
            notebook.cells.Insert(cellIndex - 1, notebook.cells[cellIndex]);
            notebook.cells.RemoveAt(cellIndex + 1);
            return true;
        }
        GUI.enabled = true;
        toolbarRect.x += 30;
        if (cellIndex == notebook.cells.Count - 1)
        {
            GUI.enabled = false;
        }
        if (GUI.Button(toolbarRect, "▼", EditorStyles.miniButtonMid))
        {
            Undo.RecordObject(notebook, "Move Cell Down");
            notebook.cells.Insert(cellIndex + 2, notebook.cells[cellIndex]);
            notebook.cells.RemoveAt(cellIndex);
            return true;
        }
        GUI.enabled = true;
        toolbarRect.x += 30;
        if (GUI.Button(toolbarRect, "✕", EditorStyles.miniButtonRight))
        {
            Undo.RecordObject(notebook, "Delete Cell");
            notebook.cells.RemoveAt(cellIndex);
            return true;
        }
        return false;
    }
    
    private static void DrawCell(Notebook.Cell cell)
    {
        switch (cell.cellType)
        {
            case Notebook.CellType.Code:
                DrawCodeCell(cell);
                break;
            case Notebook.CellType.Markdown:
                DrawTextCell(cell);
                break;
        }
    }

    private static void DrawTextCell(Notebook.Cell cell)
    {
        // TODO draw markdown
        // cell.markdownViewer.Draw();
        var text = string.Join(null, cell.source);
        EditorGUILayout.TextArea(text, _textStyle);
    }
    
    private static void DrawCodeCell(Notebook.Cell cell)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
        {
            Execute(cell);
        }
        GUILayout.BeginVertical();
        DrawCodeEditor(cell, ref _caretPos);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (cell.outputs.Count > 0)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(20)))
            {
                cell.outputs.Clear();
                Execute(cell);
            }
            GUILayout.BeginVertical();
            DrawOutput(cell);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    private static void DrawOutput(Notebook.Cell cell)
    {
        foreach (var output in cell.outputs)
        {
            switch (output.outputType)
            {
                case Notebook.OutputType.Stream:
                    EditorGUILayout.TextArea(string.Join(null, output.text), _textStyleNoBackground);
                    break;
                case Notebook.OutputType.DisplayData:
                case Notebook.OutputType.ExecuteResult:
                    foreach (var data in output.data)
                    {
                        EditorGUILayout.TextArea(string.Join(null, data.stringData), _textStyleNoBackground);
                    }
                    break;
                // TODO parse terminal control codes, set colors
                case Notebook.OutputType.Error:
                    var c = GUI.color;
                    GUI.color = Color.red;
                    GUILayout.Label(output.ename);
                    // GUILayout.Label(output.evalue);
                    GUI.color = c;
                    EditorGUILayout.TextArea(string.Join("\n", output.traceback), _codeStyleNoBackground);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    
    // https://answers.unity.com/questions/275973/find-cursor-position-in-a-textarea.html
    
    private static void DrawCodeEditor(Notebook.Cell cell, ref int caretPos)
    {
        // Execute code
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && Event.current.shift)
        {
            Execute(cell);
            Event.current.Use();
        }
        
        // TODO edit text directly as array of string in cell.source
        var text = string.Join(null, cell.source);
        
        // Code window
        GUI.SetNextControlName("code");
        text = GUILayout.TextArea(text, _codeStyle);
        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        
        // Tab key inserts spaces
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
        {
            text = text.Insert(editor.cursorIndex, "    ");
            caretPos = editor.cursorIndex + 4;
        }
        if (Event.current.keyCode == KeyCode.Tab && Event.current.type == EventType.KeyUp)
        {
            GUI.FocusControl("code");
            Event.current.Use();
            _tabPressed = true;
        }
        if (Event.current.type == EventType.Layout && _tabPressed)
        {
            editor.cursorIndex = caretPos;
            editor.selectIndex = caretPos;
            _tabPressed = false;
        }
    }

    private static void Execute(Notebook.Cell cell)
    {
        
    }
}