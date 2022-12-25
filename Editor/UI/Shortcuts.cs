using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventModifiers;

namespace UnityNotebook
{
    // TODO evaluate Unity's ShortcutManager
    // It doesn't seem to allow binding esc or return, and it
    // can't bind multiple shortcuts to the same command.
    
    public static class Shortcuts
    {
        // Thanks, ChatGPT
        public static bool HasModifiers(EventModifiers requiredModifiers)
        {
            // Check if all of the required keys are being held down
            if ((Event.current.modifiers & requiredModifiers) != requiredModifiers)
            {
                return false;
            }
            // Check if any additional keys are being held down
            var invertedModifiers = ~requiredModifiers;
            // Exclude values that we don't care about
            invertedModifiers &= ~Numeric;
            invertedModifiers &= ~FunctionKey;
            invertedModifiers &= ~CapsLock;
            if ((Event.current.modifiers & invertedModifiers) != 0)
            {
                return false;
            }
            // All of the required keys are being held down and no additional keys are being held down
            return true;
        }
        
        public static bool ConsumeReturnKey;
        public static bool HandleKeyboardShortcuts(Notebook notebook)
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return false;
            }
            
            var selectedCell = NBState.SelectedCell;

            // Any mode shortcuts
            switch (Event.current.keyCode)
            {
                // shift+enter (any mode)
                // run cell, select below
                case KeyCode.Return when HasModifiers(Shift):
                {
                    Evaluator.ExecuteCell(notebook, selectedCell);
                    if (selectedCell < notebook.cells.Count - 1)
                    {
                        NBState.SelectedCell = selectedCell + 1;
                    }
                    // consumeReturnKey = true;
                    return true;
                }
                // alt+enter (any mode)
                // run cell, insert below
                case KeyCode.Return when HasModifiers(Alt):
                {
                    GUI.FocusControl(null);
                    Evaluator.ExecuteCell(notebook, selectedCell);
                    var newCell = new Notebook.Cell { cellType = notebook.cells[selectedCell].cellType };
                    notebook.cells.Insert(selectedCell + 1, newCell);
                    NBState.SelectedCell = selectedCell + 1;
                    return true;
                }
                // ctrl+enter (any mode)
                // run cell
                case KeyCode.Return when HasModifiers(Control):
                {
                    Evaluator.ExecuteCell(notebook, selectedCell);
                    return true;
                }
            }

            // Edit mode shortcuts
            if (NBState.IsEditMode)
            {
                switch (Event.current.keyCode)
                {
                    // esc / ctrl-M (edit mode)
                    // enter command mode
                    case KeyCode.Escape when HasModifiers(None):
                    case KeyCode.M when HasModifiers(Control):
                    {
                        GUI.FocusControl(null);
                        NBState.IsEditMode = false;
                        return true;
                    }
                    // backspace in empty cell (edit mode)
                    // delete current empty cell
                    case KeyCode.Backspace when HasModifiers(None) && notebook.cells[selectedCell].source.Length == 0 || notebook.cells[selectedCell].source[0].Length == 0:
                    {
                        Undo.RecordObject(notebook, "Delete Cell");
                        notebook.cells.RemoveAt(selectedCell);
                        NBState.SelectedCell = Mathf.Max(0, selectedCell - 1);
                        return true;
                    }
                    // ctrl+shift+minus (edit mode)
                    // split cell
                    case KeyCode.Minus when HasModifiers(Control | Shift):
                    {
                        if (notebook.cells[selectedCell].source.Length == 0 || notebook.cells[selectedCell].source[0].Length == 0)
                        {
                            return false;
                        }
                        Undo.RecordObject(notebook, "Split Cell");
                        var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        var cursorIndex = editor.selectIndex;
                        var cell = notebook.cells[selectedCell];
                        // split text into two parts
                        var first = cell.rawText[..cursorIndex];
                        var second = cell.rawText[cursorIndex..];
                        // Update the first cell with the first part of split text
                        cell.rawText = first;
                        // remove trailing newline if one exists
                        if (cell.rawText.Length > 0 && cell.rawText[^1] == '\n')
                        {
                            cell.rawText = cell.rawText[..^1];
                        }
                        NBState.CopyRawTextToSourceLines(cell);
                        // Create a new cell with the second part of split text
                        var newSplitCell = new Notebook.Cell { cellType = cell.cellType, rawText = second };
                        NBState.CopyRawTextToSourceLines(newSplitCell);
                        // Insert the new cell after the current cell
                        notebook.cells.Insert(selectedCell + 1, newSplitCell);
                        NBState.SelectedCell = selectedCell + 1;
                        return true;
                    }
                }
            }
            // Command mode shortcuts
            else
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Delete when HasModifiers(None):
                    {
                        Undo.RecordObject(notebook, "Delete Cell");
                        notebook.cells.RemoveAt(selectedCell);
                        NBState.SelectedCell = Mathf.Max(0, selectedCell - 1);
                        return true;
                    }
                    // ctrl+enter (command mode)
                    // enter edit mode
                    case KeyCode.Return when HasModifiers(None):
                    {
                        NBState.IsEditMode = true;
                        ConsumeReturnKey = true;
                        return true;
                    }
                    // esc / Q (command mode)
                    // enter edit mode
                    case KeyCode.Q when HasModifiers(None):
                    case KeyCode.Escape when HasModifiers(None):
                    {
                        NBState.IsEditMode = true;
                        return true;
                    }
                    // J / down (command mode)
                    // select next cell
                    case KeyCode.J when HasModifiers(None) && selectedCell < notebook.cells.Count - 1:
                    case KeyCode.DownArrow when HasModifiers(None) && selectedCell < notebook.cells.Count - 1:
                    {
                        NBState.SelectedCell += 1;
                        return true;
                    }
                    // K / up (command mode)
                    // select previous cell
                    case KeyCode.K when HasModifiers(None) && selectedCell > 0:
                    case KeyCode.UpArrow when HasModifiers(None) && selectedCell > 0:
                    {
                        NBState.SelectedCell -= 1;
                        return true;
                    }
                    // B (command mode)
                    // add a cell below
                    case KeyCode.B when HasModifiers(None):
                    {
                        Undo.RecordObject(notebook, "Add Cell Below");
                        var c = new Notebook.Cell { cellType = Notebook.CellType.Code };
                        notebook.cells.Insert(selectedCell + 1, c);
                        NBState.SelectedCell = selectedCell + 1;
                        return true;
                    }
                    // A (command mode)
                    // add cell above
                    case KeyCode.A when HasModifiers(None):
                    {
                        Undo.RecordObject(notebook, "Add Cell Above");
                        var c2 = new Notebook.Cell { cellType = Notebook.CellType.Code };
                        notebook.cells.Insert(selectedCell, c2);
                        return true;
                    }
                    // ctrl-shift+M (command mode)
                    // merge cell below
                    case KeyCode.M when HasModifiers(Control | Shift):
                    {
                        // ignore if the cell is the last cell
                        if (selectedCell == notebook.cells.Count - 1)
                        {
                            return false;
                        }
                        Undo.RecordObject(notebook, "Merge Cell Below");
                        var cell = notebook.cells[selectedCell];
                        var cellBelow = notebook.cells[selectedCell + 1];
                        cell.rawText += "\n" + cellBelow.rawText;
                        notebook.cells.RemoveAt(selectedCell + 1);
                        NBState.CopyRawTextToSourceLines(cell);
                        NBState.instance.forceSyntaxRefresh = true;
                        return true;
                    }
                    // ctrl-backspace (command mode)
                    // merge cell above
                    case KeyCode.Backspace when HasModifiers(Control):
                    {
                        Debug.Log("merge above");
                        // ignore if the cell is the first cell
                        if (selectedCell == 0)
                        {
                            return false;
                        }
                        Undo.RecordObject(notebook, "Merge Cell Above");
                        var cell = notebook.cells[selectedCell];
                        var cellAbove = notebook.cells[selectedCell - 1];
                        cellAbove.rawText += "\n" + cell.rawText;
                        notebook.cells.RemoveAt(selectedCell);
                        NBState.SelectedCell = selectedCell - 1;
                        NBState.CopyRawTextToSourceLines(cellAbove);
                        NBState.instance.forceSyntaxRefresh = true;
                        return true;
                    }
                    // 0..6 (command mode)
                    // set header level
                    case KeyCode.Alpha1 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(1);
                        return true;
                    }
                    case KeyCode.Alpha2 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(2);
                        return true;
                    }
                    case KeyCode.Alpha3 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(3);
                        return true;
                    }
                    case KeyCode.Alpha4 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(4);
                        return true;
                    }
                    case KeyCode.Alpha5 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(5);
                        return true;
                    }
                    case KeyCode.Alpha6 when HasModifiers(None):
                    {
                        SetTextCellHeaderLevel(6);
                        return true;
                    }
                    // M (command mode)
                    // change cell type to markdown
                    case KeyCode.M when HasModifiers(None):
                    {
                        Undo.RecordObject(notebook, "Change Cell Type");
                        notebook.cells[selectedCell].cellType = Notebook.CellType.Markdown;
                        return true;
                    }
                    // Y (command mode)
                    // change cell type to code
                    case KeyCode.Y when HasModifiers(None):
                    {
                        Undo.RecordObject(notebook, "Change Cell Type");
                        notebook.cells[selectedCell].cellType = Notebook.CellType.Code;
                        return true;
                    }
                }
            }
            return false;
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
    }
}