using System;
using UnityEngine;

namespace UnityNotebook
{
    // IMGUI widget for displaying editable code.
    public static class CodeArea
    {
        public static void Draw(ref string rawText, ref string highlightedText, SyntaxHighlighting.Theme theme, GUIStyle style, params GUILayoutOption[] options)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var content = new GUIContent(rawText);
            var rect = GUILayoutUtility.GetRect(content, style, options);
            var editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), controlId);
            var oldText = content.text;
            editor.text = content.text;
            editor.SaveBackup();
            editor.controlID = controlId;
            editor.position = rect;
            editor.multiline = true;
            editor.style = style;
            editor.DetectFocusChange();
            HandleTextFieldEvent(rect, controlId, content, ref highlightedText, theme, style, editor);
            editor.UpdateScrollOffsetIfNeeded(Event.current);

            rawText = content.text;
            
            // Only run syntax highlighter when the text changes
            if (string.IsNullOrEmpty(highlightedText) || oldText != content.text)
            {
                highlightedText = SyntaxHighlighting.SyntaxToHtml(rawText, theme);
            }
        }

        private static void HandleTextFieldEvent(Rect position, int id, GUIContent content, ref string highlightedText,
            SyntaxHighlighting.Theme theme, GUIStyle style, TextEditor editor)
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
                    if (editor.HandleKeyEvent(current))
                    {
                        current.Use();
                        flag = true;
                        content.text = editor.text;
                        break;
                    }

                    if (current.character == '\n' && (current.shift || current.alt))
                    {
                        // do nothing, this is used elsewhere to execute the cell
                        current.Use();
                        break;
                    }
                    if (current.keyCode == KeyCode.Tab) // == '\t')
                    {
                        editor.MoveLineStart();
                        // move until we reach a non-space character
                        int spaces = 0;
                        while (editor.cursorIndex < editor.text.Length && editor.text[editor.cursorIndex] == ' ')
                        {
                            spaces++;
                            editor.MoveRight();
                        }

                        // Add/remove spaces
                        for (int i = 0; i < 4 - (spaces % 4); i++)
                        {
                            if (current.shift && editor.cursorIndex > 0 && editor.text[editor.cursorIndex - 1] == ' ')
                            {
                                editor.Backspace();
                            }
                            else
                            {
                                editor.Insert(' ');
                            }
                        }

                        content.text = editor.text;
                        current.Use();
                        flag = true;
                        break;
                    }

                    var character = current.character;
                    var font = style.font != null ? style.font : GUI.skin.font;
                    if (font.HasCharacter(character) || character == '\n')
                    {
                        if (character == '\n')
                        {
                            // record current cursor Vector2 position
                            int cursorIndex = editor.cursorIndex;
                            // count spaces at start of line
                            int spaces = 0;
                            editor.MoveLineStart();
                            while (editor.cursorIndex < cursorIndex && editor.text[editor.cursorIndex] == ' ')
                            {
                                spaces++;
                                editor.MoveRight();
                            }

                            // move back to original position
                            while (editor.cursorIndex != cursorIndex)
                            {
                                editor.MoveRight();
                            }

                            // insert new line
                            editor.Insert('\n');
                            // insert spaces
                            for (int i = 0; i < spaces; i++)
                            {
                                editor.Insert(' ');
                            }
                        }
                        else
                        {
                            editor.Insert(character);
                        }

                        flag = true;
                        break;
                    }

                    highlightedText = SyntaxHighlighting.SyntaxToHtml(content.text, theme);

                    flag = true;

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
}