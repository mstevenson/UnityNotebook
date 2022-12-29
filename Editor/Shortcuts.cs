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
                    var newCell = new Cell { cellType = notebook.cells[selectedCell].cellType };
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
                    case KeyCode.Escape when HasModifiers(None):
                    case KeyCode.M when HasModifiers(Control):
                    {
                        Commands.EnterCommandMode();
                        return true;
                    }
                    // backspace in empty cell (edit mode)
                    case KeyCode.Backspace when HasModifiers(None) && notebook.cells.Count > 0 && (notebook.cells[selectedCell].source.Length == 0 || notebook.cells[selectedCell].source[0].Length == 0):
                    {
                        Commands.DeleteCell(NBState.SelectedCell);
                        return true;
                    }
                    // ctrl+shift+minus (edit mode)
                    case KeyCode.Minus when HasModifiers(Control | Shift):
                    {
                        return Commands.SplitCell();
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
                        Commands.DeleteCell(NBState.SelectedCell);
                        return true;
                    }
                    // ctrl+enter (command mode)
                    case KeyCode.Return when HasModifiers(None):
                    {
                        Commands.EnterEditMode();
                        ConsumeReturnKey = true;
                        return true;
                    }
                    // esc / Q (command mode)
                    case KeyCode.Q when HasModifiers(None):
                    case KeyCode.Escape when HasModifiers(None):
                    {
                        Commands.EnterEditMode();
                        return true;
                    }
                    // J / down (command mode)
                    case KeyCode.J when HasModifiers(None) && selectedCell < notebook.cells.Count - 1:
                    case KeyCode.DownArrow when HasModifiers(None) && selectedCell < notebook.cells.Count - 1:
                    {
                        Commands.SelectNextCell();
                        return true;
                    }
                    // K / up (command mode)
                    case KeyCode.K when HasModifiers(None) && selectedCell > 0:
                    case KeyCode.UpArrow when HasModifiers(None) && selectedCell > 0:
                    {
                        Commands.SelectPreviousCell();
                        return true;
                    }
                    // B (command mode)
                    case KeyCode.B when HasModifiers(None):
                    {
                        Commands.AddCellBelow();
                        return true;
                    }
                    // A (command mode)
                    case KeyCode.A when HasModifiers(None):
                    {
                        Commands.AddCellAbove();
                        return true;
                    }
                    // ctrl-shift+M (command mode)
                    case KeyCode.M when HasModifiers(Control | Shift):
                    {
                        return Commands.MergeCellBelow();
                    }
                    // ctrl-backspace (command mode)
                    case KeyCode.Backspace when HasModifiers(Control):
                    {
                        return Commands.MergeCellAbove();
                    }
                    // 0..6 (command mode)
                    // set header level
                    case KeyCode.Alpha1 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(1);
                        return true;
                    }
                    case KeyCode.Alpha2 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(2);
                        return true;
                    }
                    case KeyCode.Alpha3 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(3);
                        return true;
                    }
                    case KeyCode.Alpha4 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(4);
                        return true;
                    }
                    case KeyCode.Alpha5 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(5);
                        return true;
                    }
                    case KeyCode.Alpha6 when HasModifiers(None):
                    {
                        Commands.SetTextCellHeaderLevel(6);
                        return true;
                    }
                    // M (command mode)
                    // change cell type to markdown
                    case KeyCode.M when HasModifiers(None):
                    {
                        Commands.ConvertCellToMarkdown();
                        return true;
                    }
                    // Y (command mode)
                    // change cell type to code
                    case KeyCode.Y when HasModifiers(None):
                    {
                        Commands.ConvertCellToCode();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}