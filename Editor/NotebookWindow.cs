using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Editor;
using MG.MDV;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

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

    private static GUIStyle _textStyle;
    private static GUIStyle _textStyleNoBackground;
    private static GUIStyle _codeStyle;
    private static GUIStyle _codeStyleNoBackground;
    
    // private static int _caretPos;
    
    private static bool _openExternally;

    private static bool IsRunning => NotebookWindowData.instance.runningCell >= 0;

    private static Notebook OpenedNotebook
    {
        get => NotebookWindowData.instance.openedNotebook;
        set
        {
            NotebookWindowData.instance.openedNotebook = value;
            NotebookWindowData.instance.Save();
        }
    }

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
        OpenedNotebook = notebook;
        return true;
    }

    private static void ChangeNotebook(Notebook notebook)
    {
        NotebookWindowData.instance.Clear();
        OpenedNotebook = notebook;
    }

    private void OnEnable()
    {
        ChangeNotebook(NotebookWindowData.instance.openedNotebook);
        EditorApplication.update += DoRepaint;
    }

    private void OnDisable()
    {
        Evaluator.Stop();
        EditorApplication.update -= DoRepaint;
    }

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
        
        DrawToolbar();
        
        if (OpenedNotebook == null)
        {
            DrawCreateNotebook();
        }
        else
        {
            DrawNotebook(OpenedNotebook);
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
            var path = EditorUtility.SaveFilePanelInProject("Create Notebook", "Notebook", "ipynb", "Create Notebook");
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith(".ipynb"))
                {
                    path += ".ipynb";
                }
                var nb = Notebook.CreateAsset(path);
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(nb);
                nb.cells.Add(new Notebook.Cell { cellType = Notebook.CellType.Code });
                OpenedNotebook = nb;
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
        EditorGUI.BeginChangeCheck();
        var nb = EditorGUILayout.ObjectField(OpenedNotebook, typeof(Notebook), true) as Notebook;
        if (EditorGUI.EndChangeCheck())
        {
            ChangeNotebook(nb);
            // _caretPos = 0;
            if (OpenedNotebook == null)
            {
                return;
            }
            // get asset path
            var path = AssetDatabase.GetAssetPath(OpenedNotebook);
            foreach (var cell in OpenedNotebook.cells)
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

        using (new EditorGUI.DisabledScope(OpenedNotebook == null))
        {
            if (OpenedNotebook != null && IsRunning)
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
                    Evaluator.ExecuteAll(OpenedNotebook);
                }
            }
            using (new EditorGUI.DisabledScope(OpenedNotebook == null || IsRunning))
            {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    Undo.RecordObject(OpenedNotebook, "Clear All Output");
                    OpenedNotebook.ClearOutputs();
                }
                using (new EditorGUI.DisabledScope(OpenedNotebook != null && OpenedNotebook.scriptState == null))
                {
                    if (GUILayout.Button("Restart", EditorStyles.toolbarButton))
                    {
                        OpenedNotebook.scriptState = null;
                    }
                }
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    EditorUtility.SetDirty(OpenedNotebook);
                    OpenedNotebook.SaveJson();
                    AssetDatabase.SaveAssets();
                }
                if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                {
                    _openExternally = true;
                    AssetDatabase.OpenAsset(OpenedNotebook);
                    _openExternally = false;
                }
            }
        }

        using (new EditorGUI.DisabledScope(OpenedNotebook != null && IsRunning))
        {
            GUILayout.FlexibleSpace();
            DrawNotebookSelector();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DoRepaint()
    {
        if (NotebookWindowData.instance.runningCell == -1)
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
        
        NotebookWindowData.instance.scroll = EditorGUILayout.BeginScrollView(NotebookWindowData.instance.scroll, false, false);

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
        switch (notebook.cells[cell].cellType)
        {
            case Notebook.CellType.Code:
                DrawCodeCell(notebook, cell);
                break;
            case Notebook.CellType.Markdown:
                DrawTextCell(notebook.cells[cell]);
                break;
        }
    }

    private static void DrawTextCell(Notebook.Cell cell)
    {
        // TODO draw markdown
        // cell.markdownViewer.Draw();
        var text = cell.source == null ? string.Empty : string.Concat(cell.source);
        EditorGUILayout.TextArea(text, _textStyle);
    }
    
    private void DrawCodeCell(Notebook notebook, int cell)
    {
        GUILayout.BeginHorizontal();
        if (IsRunning && NotebookWindowData.instance.runningCell == cell)
        {
            if (GUILayout.Button("■", GUILayout.Width(20), GUILayout.Height(20)))
            {
                Evaluator.Stop();
            }
        }
        else
        {
            using (new EditorGUI.DisabledScope(NotebookWindowData.instance.runningCell != -1))
            {
                if (GUILayout.Button("▶", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    Evaluator.Execute(notebook, cell);
                }
            }
        }
        GUILayout.BeginVertical();

        var c = notebook.cells[cell];
        c.rawText ??= string.Concat(c.source);
        var syntaxTheme = EditorGUIUtility.isProSkin ? SyntaxTheme.Dark : SyntaxTheme.Light;
        // a horizontal scroll view
        c.scroll = GUILayout.BeginScrollView(c.scroll, false, false, GUILayout.ExpandHeight(false));
        CodeArea.Draw(ref c.rawText, ref c.highlightedText, syntaxTheme, _codeStyle);
        GUILayout.EndScrollView();
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
            EditorUtility.SetDirty(notebook);
        }
        
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

    private static void DrawOutput(Notebook.Cell cell)
    {
        foreach (var output in cell.outputs)
        {
            switch (output.outputType)
            {
                case Notebook.OutputType.Stream:
                    EditorGUILayout.TextArea(string.Concat(output.text), _textStyleNoBackground);
                    break;
                case Notebook.OutputType.DisplayData:
                case Notebook.OutputType.ExecuteResult:
                    foreach (var data in output.data)
                    {
                        EditorGUILayout.TextArea(string.Concat(data.stringData), _textStyleNoBackground);
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