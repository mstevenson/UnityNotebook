using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    // Converts a markdown string to a styled Unity Rich Text string.
    public static class Markdown
    {
        private static readonly GUIStyle Style = new(EditorStyles.label)
        {
            richText = true,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = true
        };
        
        private static readonly GUIStyle H1 = new(EditorStyles.boldLabel)
        {
            fontSize = 22,
            richText = true,
            padding = new RectOffset(4, 4, 6, 4),
            wordWrap = true
        };
        
        private static readonly GUIStyle H2 = new(EditorStyles.boldLabel)
        {
            fontSize = 20,
            richText = true,
            padding = new RectOffset(4, 4, 6, 4),
            wordWrap = true
        };
        
        private static readonly GUIStyle H3 = new(EditorStyles.boldLabel)
        {
            fontSize = 18,
            richText = true,
            padding = new RectOffset(4, 4, 4, 2),
            wordWrap = true
        };
        
        private static readonly GUIStyle H4 = new(EditorStyles.boldLabel)
        {
            fontSize = 16,
            richText = true,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = true
        };
        
        private static readonly GUIStyle H5 = new(EditorStyles.boldLabel)
        {
            fontSize = 14,
            richText = true,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = true
        };
        
        private static readonly GUIStyle H6 = new(EditorStyles.boldLabel)
        {
            fontSize = 12,
            richText = true,
            padding = new RectOffset(4, 4, 2, 2),
            wordWrap = true
        };
        
        private static readonly GUIStyle BlockQuote = new(EditorStyles.label)
        {
            richText = true,
            fontStyle = FontStyle.Italic,
            fontSize = 14,
            padding = new RectOffset(10, 4, 4, 4),
            wordWrap = true
        };

        
        // TODO implement EditorGUI.hyperLinkClicked
        // https://docs.unity3d.com/ScriptReference/EditorGUI-hyperLinkClicked.html
        
        public static void Draw(string[] lines)
        {
            bool codeBlock = false;
            if (lines == null || lines.Length == 0)
            {
                GUILayout.Label("");
            }
            foreach (var line in lines)
            {
                var l = line.Trim();
                
                // skip double newlines
                if (l == "")
                {
                    continue;
                }
                
                // unordered list
                bool orderedList = false;
                if (l.StartsWith('*') || l.StartsWith('+'))
                {
                    orderedList = true;
                    l = l.Replace("*", "");
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(8);
                    GUILayout.Label("•", GUILayout.Width(10));
                    // EndHorizontal is called a the end
                }

                // h6
                if (Regex.IsMatch(l, "^#{6}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H6);
                }
                // h5
                else if (Regex.IsMatch(l, "^#{5}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H5);
                } 
                // h4
                else if (Regex.IsMatch(l, "^#{4}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H4);
                }
                // h3
                else if (Regex.IsMatch(l, "^#{3}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H3);
                }
                // h2
                else if (Regex.IsMatch(l, "^#{2}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H2);
                }
                // h1
                else if (Regex.IsMatch(l, "^#{1}"))
                {
                    GUILayout.Label(l.Replace("#", ""), H1);
                }
                // ordered list
                // TODO renumber
                else if (Regex.IsMatch(l, @"^\d+\."))
                {
                    GUILayout.Label(l.Replace(@"\d+\.", ""), Style);
                }
                // bold
                else if (Regex.IsMatch(l, @"\*\*.*\*\*"))
                {
                    GUILayout.Label(Regex.Replace(l, @"\*\*(.*)\*\*", "<b>$1</b>"), Style);
                }
                // italic
                else if (Regex.IsMatch(l, @"\*.*\*"))
                {
                    GUILayout.Label(Regex.Replace(l, @"\*(.*)\*", "<i>$1</i>"), Style);
                }
                // links
                else if (Regex.IsMatch(l, @"\[.*\]\(.*\)"))
                {
                    GUILayout.Label(Regex.Replace(l, @"\[(.*)\]\((.*)\)", "<a href=\"$2\">$1</a>"), Style);
                }
                // block quote
                else if (Regex.IsMatch(l, @"^>"))
                {
                    // TODO indent the label
                    GUILayout.Label(l.Replace(">", ""), BlockQuote);
                }
                // inline code
                // TODO monospace font
                else if (Regex.IsMatch(l, @"`.*`"))
                {
                    GUILayout.Label(Regex.Replace(l, "`(.*)`", "<color=#aff>$1</color>"), Style);
                }
                
                else if (!codeBlock && Regex.IsMatch(l, @"^```"))
                {
                    codeBlock = true;
                    GUILayout.Label(l.Replace("```", ""), Style);
                }
                else if (codeBlock)
                {
                    if (Regex.IsMatch(l, @"^```"))
                    {
                        codeBlock = false;
                    }
                    // TODO display code block
                }
                // horizontal line, match any number of dashes
                else if (Regex.IsMatch(l, "^---+$"))
                {
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
                }
                else
                {
                    GUILayout.Label(l, Style);
                }
                
                // TODO tables?

                if (orderedList)
                {
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}