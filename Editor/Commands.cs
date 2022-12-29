using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    public static class Commands
    {
        public static Notebook CreateNotebookAsset(string path)
        {
            var notebook = ScriptableObject.CreateInstance<Notebook>();
            var json = JsonConvert.SerializeObject(notebook, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Notebook>(path);
        }
        
        public static void EnterCommandMode()
        {
            GUI.FocusControl(null);
            NBState.IsEditMode = false;
        }

        public static void EnterEditMode()
        {
            NBState.IsEditMode = true;
        }

        public static void SelectNextCell()
        {
            NBState.SelectedCell += 1;
        }
        
        public static void SelectPreviousCell()
        {
            NBState.SelectedCell -= 1;
        }

        public static void AddCell(CellType type, Notebook notebook, int cellIndex)
        {
            Undo.RecordObject(notebook, "Add Cell");
            var c = new Cell { cellType = type };
            notebook.cells.Insert(cellIndex, c);
            NBState.SelectedCell = cellIndex;
            NBState.IsEditMode = true;
        }
        
        public static void DeleteCell(int cellIndex)
        {
            var nb = NBState.OpenedNotebook;
            Undo.RecordObject(nb, "Delete Cell");
            nb.cells.RemoveAt(cellIndex);
            NBState.SelectedCell = Mathf.Max(0, cellIndex - 1);
            NBState.SetNotebookDirty();
        }
        
        public static void ClearCellOutput(Cell cell)
        {
            var nb = NBState.OpenedNotebook;
            Undo.RecordObject(nb, "Clear Output");
            
            // Delete unused texture assets from the ScriptableObject
            foreach (var output in cell.outputs)
            {
                if (output is not CellOutputDisplayData displayData)
                {
                    continue;
                }
                foreach (var value in displayData.values)
                {
                    if (value.Object is UnityObjectPreview preview)
                    {
                        NBState.DiscardTexture(preview.hash);
                    }
                }
            }
            
            cell.executionCount = 0;
            cell.outputs.Clear();
        }

        public static void ClearAllOutputs()
        {
            var nb = NBState.OpenedNotebook;
            Undo.RecordObject(nb, "Clear All Output");
            if (nb == null)
            {
                return;
            }
            foreach (var cell in nb.cells)
            {
                ClearCellOutput(cell);
            }
            EditorUtility.SetDirty(nb);
            NBState.SaveScriptableObject();
        }
        
        public static void MoveCellDown(int cellIndex)
        {
            var notebook = NBState.OpenedNotebook;
            Undo.RecordObject(notebook, "Move Cell Down");
            notebook.cells.Insert(cellIndex + 2, notebook.cells[cellIndex]);
            notebook.cells.RemoveAt(cellIndex);
            NBState.SelectedCell = cellIndex + 1;
            NBState.SetNotebookDirty();
        }

        public static void MoveCellUp(int cellIndex)
        {
            var notebook = NBState.OpenedNotebook;
            Undo.RecordObject(notebook, "Move Cell Up");
            notebook.cells.Insert(cellIndex - 1, notebook.cells[cellIndex]);
            notebook.cells.RemoveAt(cellIndex + 1);
            NBState.SelectedCell = cellIndex - 1;
            NBState.SetNotebookDirty();
        }
        
        public static void AddCellBelow()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
            Undo.RecordObject(notebook, "Add Cell Below");
            var c = new Cell { cellType = CellType.Code };
            notebook.cells.Insert(selectedCell + 1, c);
            NBState.SelectedCell = selectedCell + 1;
        }

        public static void AddCellAbove()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
            Undo.RecordObject(notebook, "Add Cell Above");
            var c2 = new Cell { cellType = CellType.Code };
            notebook.cells.Insert(selectedCell, c2);
        }

        public static bool SplitCell()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
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
            var newSplitCell = new Cell { cellType = cell.cellType, rawText = second };
            NBState.CopyRawTextToSourceLines(newSplitCell);
            // Insert the new cell after the current cell
            notebook.cells.Insert(selectedCell + 1, newSplitCell);
            NBState.SelectedCell = selectedCell + 1;
            return true;
        }

        public static bool MergeCellBelow()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
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

        public static bool MergeCellAbove()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
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

        public static void SetTextCellHeaderLevel(int level)
        {
            var notebook = NBState.OpenedNotebook;
            var cell = NBState.SelectedCell;
            if (notebook.cells[cell].cellType != CellType.Markdown)
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

        public static void ConvertCellToMarkdown()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
            Undo.RecordObject(notebook, "Change Cell Type");
            notebook.cells[selectedCell].cellType = CellType.Markdown;
        }
        
        public static void ConvertCellToCode()
        {
            var notebook = NBState.OpenedNotebook;
            var selectedCell = NBState.SelectedCell;
            Undo.RecordObject(notebook, "Change Cell Type");
            notebook.cells[selectedCell].cellType = CellType.Code;
        }
    }
}