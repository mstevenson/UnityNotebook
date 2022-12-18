using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Editor
{
    public class SyntaxHighlighting
    {
        public static string SyntaxToHtml(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var sb = new StringBuilder();
            
            void Set(SyntaxToken token, Color color)
            {
                sb.Append($"{token.LeadingTrivia.ToFullString()}<color=#{ColorUtility.ToHtmlStringRGB(color)}>{token.ToString()}</color>{token.TrailingTrivia.ToFullString()}");
            }
            void SetNormal(SyntaxToken token)
            {
                sb.Append($"{token.LeadingTrivia.ToFullString()}{token.ToString()}{token.TrailingTrivia.ToFullString()}");
            }

            foreach (var token in root.DescendantTokens())
            {
                var kind = token.Kind();
                switch (kind)
                {
                    // built-in types
                    case var _ when (int)kind >= 8304 && (int)kind <= 8320:
                        Set(token, new Color(0.4f, 0.6f, 0.83f));
                        break;
                    // control flow
                    case var _ when (int)kind >= 8321 && (int)kind <= 8342:
                        Set(token, new Color(0.73f, 0.54f, 0.75f));
                        break;
                    // scope and accessibility modifiers
                    case var _ when (int)kind >= 8343 && (int)kind <= 8384:
                    case VarKeyword:
                        Set(token, new Color(0.4f, 0.6f, 0.83f));
                        break;
                    case var _ when (int)kind >= 8405 && (int)kind <= 8440:
                        Set(token, new Color(0.73f, 0.54f, 0.75f));
                        break;
                    case NumericLiteralToken:
                        Set(token, new Color(0.73f, 0.8f, 0.67f));
                        break;
                    case StringLiteralToken:
                    case CharacterLiteralToken:
                        Set(token, new Color(0.77f, 0.58f, 0.49f));
                        break;
                    case IdentifierToken:
                        Set(token, new Color(0.66f, 0.85f, 0.99f));
                        break;
                    default:
                        SetNormal(token);
                        // Debug.Log(token + "    " + kind);
                        break;
                }
            }
            
            return sb.ToString();
        }
    }
}