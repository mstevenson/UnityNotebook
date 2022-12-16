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

    private static GUIStyle _codeStyle;
    private static bool _tabPressed;
    private static int _caretPos;
    
    private Notebook _notebook;

    private Vector2 _scroll;

    private void OnGUI()
    {
        if (_codeStyle == null)
        {
            _codeStyle = new GUIStyle(GUI.skin.textArea)
            {
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Menlo-Regular.ttf"),
                richText = true
            };
        }
        
        EditorGUI.BeginChangeCheck();
        _notebook = EditorGUILayout.ObjectField(_notebook, typeof(Notebook), true) as Notebook;
        if (EditorGUI.EndChangeCheck())
        {
            _caretPos = 0;
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
        
        _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false);
        DrawNotebook(_notebook);
        EditorGUILayout.EndScrollView();
    }

    private static void DrawNotebook(Notebook notebook)
    {
        if (notebook == null)
        {
            return;
        }
        foreach (var cell in notebook.cells)
        {
            DrawCell(cell);
        }
    }
    
    private static void DrawCell(Notebook.Cell cell)
    {
        GUILayout.BeginVertical(EditorStyles.textArea);
        // concatenate text lines from cell.source array
        var text = string.Join(null, cell.source);
        if (cell.cellType == Notebook.CellType.Markdown)
        {
            cell.markdownViewer.Draw();
            EditorGUILayout.TextArea(text);
        }
        else
        {
            // TODO split textarea contents into lines to store in Notebook object
            text = DrawCodeEditor(text, ref _caretPos);
        }
        GUILayout.EndVertical();
        GUILayout.Space(20);
    }
    
    
    // https://answers.unity.com/questions/275973/find-cursor-position-in-a-textarea.html
    
    private static string DrawCodeEditor(string text, ref int caretPos)
    {
        // Execute code
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && Event.current.shift)
        {
            Execute(text);
            Event.current.Use();
        }
        
        // Code window
        GUI.SetNextControlName("code");
        text = GUILayout.TextArea(text, _codeStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
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

        return text;
    }

    private static void Execute(string code)
    {
        
    }
}