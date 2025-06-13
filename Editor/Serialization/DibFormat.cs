using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityNotebook
{
    // Differences between .dib and .ipynb files:
    // https://github.com/dotnet/interactive/blob/main/docs/FAQ.md#whats-the-difference-between-a-dib-file-and-an-ipynb-file
    
    // .dib notebook example:
    // https://github.com/dotnet/interactive/blob/main/NotebookTestScript.dib

    public static class DibFormat
    {
        private static readonly Regex MagicCommandRegex = new Regex(@"^#!(\w+)(.*)$", RegexOptions.Multiline);
        private static readonly char[] NewlineChars = { '\n', '\r' };

        public static Notebook ParseDibToNotebook(string dibContent)
        {
            var notebook = ScriptableObject.CreateInstance<Notebook>();
            notebook.format = 4;
            notebook.formatMinor = 2;
            notebook.cells = new List<Cell>();
            
            if (string.IsNullOrWhiteSpace(dibContent))
            {
                return notebook;
            }
            
            // Split content by magic commands
            var lines = dibContent.Split(NewlineChars, StringSplitOptions.None);
            var currentCell = new StringBuilder();
            var currentCellType = CellType.Code; // Default to code

            foreach (var line in lines)
            {
                var match = MagicCommandRegex.Match(line);
                if (match.Success)
                {
                    // Save current cell if it has content, trimming trailing whitespace
                    if (currentCell.Length > 0)
                    {
                        AddCellToNotebook(notebook, TrimTrailingWhitespace(currentCell.ToString()), currentCellType);
                        currentCell.Clear();
                    }

                    // Determine cell type from magic command
                    var magic = match.Groups[1].Value.ToLower();
                    currentCellType = magic switch
                    {
                        "markdown" => CellType.Markdown,
                        "csharp" => CellType.Code,
                        "c#" => CellType.Code,
                        "meta" => CellType.Raw, // Skip metadata for now
                        _ => CellType.Code // For other magic commands, treat as code
                    };
                }
                else
                {
                    // Skip empty lines immediately after magic commands
                    if (currentCell.Length == 0 && string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    
                    // Add line to current cell
                    if (currentCell.Length > 0)
                    {
                        currentCell.AppendLine();
                    }
                    currentCell.Append(line);
                }
            }

            // Add final cell if it has content, trimming trailing whitespace
            if (currentCell.Length > 0)
            {
                AddCellToNotebook(notebook, TrimTrailingWhitespace(currentCell.ToString()), currentCellType);
            }

            return notebook;
        }

        private static void AddCellToNotebook(Notebook notebook, string content, CellType cellType)
        {
            // Skip raw cells (like meta)
            if (cellType == CellType.Raw)
            {
                return;
            }

            var cell = new Cell
            {
                cellType = cellType,
                source = content.Split(NewlineChars, StringSplitOptions.None),
                outputs = new List<CellOutput>()
            };

            notebook.cells.Add(cell);
        }

        private static string TrimTrailingWhitespace(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }
            
            // Remove trailing newlines and whitespace
            return content.TrimEnd('\r', '\n', ' ', '\t');
        }
        
        public static string NotebookToDib(Notebook notebook)
        {
            var dibBuilder = new StringBuilder();
            
            const string metadata = @"#!meta
{
    ""kernelInfo"": {
        ""defaultKernelName"": ""csharp"",
        ""items"": [
            {
                ""name"": ""csharp"",
                ""languageName"": ""csharp"",
                ""aliases"": []
            }
        ]
    }
}

";
            dibBuilder.Append(metadata);

            foreach (var cell in notebook.cells)
            {
                // Add magic command based on cell type
                string magic = cell.cellType switch
                {
                    CellType.Markdown => "#!markdown",
                    CellType.Code => "#!csharp",
                    _ => "#!meta", // Skip raw cells in output
                };
                dibBuilder.AppendLine(magic);
                dibBuilder.AppendLine();
                
                // Add cell content
                if (cell.source is { Length: > 0 })
                {
                    foreach (var line in cell.source)
                    {
                        dibBuilder.AppendLine(line.Trim());
                    }
                }
                
                dibBuilder.AppendLine();
            }
            
            return dibBuilder.ToString();
        }
    }
}