using System;
using System.Collections.Generic;
using Editor;
using UnityEngine;

public static class CodeArea
{
    public static void Draw(ref string rawText, ref string highlightedText, GUIStyle style, params GUILayoutOption[] options)
    {
        var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
        var content = new GUIContent(rawText);
        var rect = GUILayoutUtility.GetRect(content, style, options);
        var editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), controlId);
        editor.text = content.text;
        editor.SaveBackup();
        editor.controlID = controlId;
        editor.position = rect;
        editor.multiline = true;
        editor.style = style;
        editor.DetectFocusChange();
        HandleTextFieldEvent(rect, controlId, content, ref highlightedText, style, editor);
        editor.UpdateScrollOffsetIfNeeded(Event.current);

        rawText = content.text;
        
        // TODO only update highlighted text if gui changed
        
        highlightedText = SyntaxHighlighting.SyntaxToHtml(rawText);
    }
    
    private static void HandleTextFieldEvent(Rect position, int id, GUIContent content, ref string highlightedText, GUIStyle style, TextEditor editor)
    {
        var current = Event.current;
        var flag = false;
        switch (current.type)
        {
            case EventType.MouseDown:
            {
                if (position.Contains(current.mousePosition))
                {
                    GUIUtility.hotControl = id;
                    GUIUtility.keyboardControl = id;
                    editor.MoveCursorToPosition(Event.current.mousePosition);
                    if (Event.current.clickCount == 2 && GUI.skin.settings.doubleClickSelectsWord)
                    {
                        editor.SelectCurrentWord();
                        editor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
                        editor.MouseDragSelectsWholeWords(true);
                    }
                    if (Event.current.clickCount == 3 && GUI.skin.settings.tripleClickSelectsLine)
                    {
                        editor.SelectCurrentParagraph();
                        editor.MouseDragSelectsWholeWords(true);
                        editor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
                    }
                    current.Use();
                }
                break;
            }
            case EventType.MouseUp:
            {
                if (GUIUtility.hotControl == id)
                {
                    editor.MouseDragSelectsWholeWords(false);
                    GUIUtility.hotControl = 0;
                    current.Use();
                }
                break;
            }
            case EventType.MouseDrag:
            {
                if (GUIUtility.hotControl == id)
                {
                    if (current.shift)
                        editor.MoveCursorToPosition(Event.current.mousePosition);
                    else
                        editor.SelectToPosition(Event.current.mousePosition);
                    current.Use();
                }
                break;
            }
            case EventType.KeyDown:
            {
                if (GUIUtility.keyboardControl != id)
                    return;
                if (current.keyCode == KeyCode.Tab || current.character == '\t')
                {
                    editor.ReplaceSelection("    ");
                    current.Use();
                    break;
                }
                if (editor.HandleKeyEvent(current))
                {
                    current.Use();
                    flag = true;
                    content.text = editor.text;
                    break;
                }

                var character = current.character;
                var font = style.font != null ? style.font : GUI.skin.font;
                if (font.HasCharacter(character) || character == '\n')
                {
                    editor.Insert(character);
                    flag = true;
                    break;
                }
                
                highlightedText = SyntaxHighlighting.SyntaxToHtml(content.text);
                
                break;
            }
            case EventType.Repaint:
            {
                var highlightedContent = new GUIContent(highlightedText);
                if (GUIUtility.keyboardControl != id)
                {
                    style.Draw(position, highlightedContent, id, false);
                    break;
                }
                // TODO need to duplicate this class and modify its draw function to draw highlighted text but edit raw text
                editor.DrawCursor(content.text);
                break;
            }
        }
        
        if (!flag)
            return;
        GUI.changed = true;
        content.text = editor.text;
        current.Use();
    }
}
