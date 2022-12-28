using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace UnityNotebook
{
    public class LocalVariableExtractor : CSharpSyntaxRewriter
    {
        private readonly SyntaxGenerator _syntaxGenerator;
        
        private LocalVariableExtractor(SyntaxGenerator syntaxGenerator)
        {
            _syntaxGenerator = syntaxGenerator;
        }
        
        public static string Extract(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp);
            var rewriter = new LocalVariableExtractor(syntaxGenerator);
            var newRoot = rewriter.Visit(root);
            var newCode = newRoot.ToFullString();
            return newCode;
        }
        
        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            // Extract the local variables into a global scope
            List<SyntaxNode> declarations = new();
            foreach (var v in node.Declaration.Variables)
            {
                var field = _syntaxGenerator.FieldDeclaration(v.Identifier.ValueText, v.Initializer.Value);
                declarations.Add(field);
            }
            
            // This creates a class called GlobalScope, but how do I actually add it to the executing scope?
            SyntaxNode classDeclaration = _syntaxGenerator.ClassDeclaration("GlobalScope", null, Accessibility.Public, DeclarationModifiers.None, null, null, declarations);

            // Add the global scope class to the root node
            var rootNode = (CompilationUnitSyntax)node.SyntaxTree.GetRoot();
            rootNode.AddMembers((MemberDeclarationSyntax)classDeclaration);

            // Replace the local variable declaration with a reference to the global variable
            foreach (var variable in node.Declaration.Variables)
            {
                var identifier = variable.Identifier;
                var identifierName = SyntaxFactory.IdentifierName(identifier);
                var globalIdentifierName = SyntaxFactory.IdentifierName("GlobalScope." + identifier.ValueText);
                var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, identifierName, globalIdentifierName);
                var expressionStatement = SyntaxFactory.ExpressionStatement(assignment);
                node = node.ReplaceNode(identifierName, expressionStatement);
            }
            
            return node;
        }
    }
}