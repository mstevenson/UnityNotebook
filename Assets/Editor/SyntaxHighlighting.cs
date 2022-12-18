using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Editor
{
    public class SyntaxHighlighting
    {
        [MenuItem("Notebook/Highlight")]
        public static void Test()
        {
            string code = @"using System;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";
            var html = SyntaxToHtml(code);
            Debug.Log(html);
        }
        
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
                    case IdentifierToken:
                        Set(token, new Color(0.66f, 0.85f, 0.99f));
                        break;
                    case NamespaceKeyword:
                    case ClassKeyword:
                    case StaticKeyword:
                    case ReadOnlyKeyword:
                    case VoidKeyword:
                    case UsingKeyword:
                    case AbstractKeyword:
                    case StringKeyword:
                    case FloatKeyword:
                    case IntKeyword:
                        Set(token, new Color(0.4f, 0.6f, 0.83f));
                        break;
                    case ReturnKeyword:
                        Set(token, new Color(0.73f, 0.54f, 0.75f));
                        break;
                    case NumericLiteralToken:
                        Set(token, new Color(0.73f, 0.8f, 0.67f));
                        break;
                    case StringLiteralToken:
                        Set(token, new Color(0.77f, 0.58f, 0.49f));
                        break;
                    // case SyntaxKind.Parameter:
                    //     Set(token, new Color(0.66f, 0.85f, 0.99f));
                    //     break;
                    default:
                        SetNormal(token);
                        Debug.Log(token + "    " + kind);
                        break;
                }
            }
            
            return sb.ToString();
        }
    }
}