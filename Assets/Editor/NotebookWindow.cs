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
    private static GUIStyle _codeStyle;
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
        if (_notebook == null)
        {
            GUI.enabled = false;
        }
        
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Run", EditorStyles.toolbarButton))
        {
            Debug.Log("Run");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
        {
            Debug.Log("Clear");
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
            notebook.cells.Insert(cellIndex, new Notebook.Cell { cellType = Notebook.CellType.Code });
            return true;
        }
        buttonRect.x += 85;
        if (GUI.Button(buttonRect, "+ Text"))
        {
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
            notebook.cells.Insert(cellIndex + 2, notebook.cells[cellIndex]);
            notebook.cells.RemoveAt(cellIndex);
            return true;
        }
        GUI.enabled = true;
        toolbarRect.x += 30;
        if (GUI.Button(toolbarRect, "✕", EditorStyles.miniButtonRight))
        {
            notebook.cells.RemoveAt(cellIndex);
            return true;
        }
        return false;
    }
    
    private static void DrawCell(Notebook.Cell cell)
    {
        // Code area
        GUILayout.BeginHorizontal();
        if (cell.cellType == Notebook.CellType.Code)
        {
            if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
            {
                Execute(cell);
            }
        }
        GUILayout.BeginVertical();
        if (cell.cellType == Notebook.CellType.Markdown)
        {
            DrawTextCell(cell);
        }
        else
        {
            DrawCodeEditor(cell, ref _caretPos);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private static void DrawTextCell(Notebook.Cell cell)
    {
        // TODO draw markdown
        // cell.markdownViewer.Draw();
        var text = string.Join(null, cell.source);
        EditorGUILayout.TextArea(text, _textStyle);
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