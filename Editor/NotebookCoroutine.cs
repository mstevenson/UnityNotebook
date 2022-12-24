using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace UnityNotebook
{
    public class NotebookCoroutine : MonoBehaviour
    {
        // private static NotebookCoroutine _instance;
        private static EditorCoroutine _editorCoroutine;
    
        [UsedImplicitly]
        public static void Run(IEnumerator routine)
        {
            _editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(StartCoroutineWithReturnValues(routine));
        }
    
        [UsedImplicitly]
        public static void StopAll()
        {
            if (_editorCoroutine == null)
            {
                return;
            }
            NBState.RunningCell = -1;
            EditorCoroutineUtility.StopCoroutine(_editorCoroutine);
            _editorCoroutine = null;
        }
    
        private static IEnumerator StartCoroutineWithReturnValues(IEnumerator routine)
        {
            yield return null; // let the UI update once before potentially blocking
            yield return RunInternal(routine, output =>
            {
                if (output != null && output is not YieldInstruction && output is not EditorWaitForSeconds)
                {
                    Evaluator.CaptureOutput(output);
                }
            });
            NBState.RunningCell = -1;
        }
    
        private static IEnumerator RunInternal(IEnumerator target, Action<object> output)
        {
            while (target.MoveNext())
            {
                var result = target.Current;
                if (result is WaitForSeconds)
                {
                    // Convert to EditorWaitForSeconds, editor coroutines don't support runtime WaitForSeconds
                    var seconds = (float)result.GetType().GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(result);
                    result = new EditorWaitForSeconds(seconds);
                }
                output(result);
                // TODO add output to the ScriptState     
                yield return result;
            }
        }
        
        // public class LocalVariableExtractor : CSharpSyntaxRewriter
        // {
        //     private readonly SemanticModel _semanticModel;
        //     private readonly SyntaxGenerator _syntaxGenerator;
        //
        //     public LocalVariableExtractor(SemanticModel semanticModel)
        //     {
        //         _semanticModel = semanticModel;
        //         _syntaxGenerator = SyntaxGenerator.GetGenerator(_semanticModel.SyntaxTree);
        //     }
        //
        //     public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        //     {
        //         // Extract the local variables into a global scope
        //         var declarations = node.Declaration.Variables
        //             .Select(v => _syntaxGenerator.FieldDeclaration(v.Identifier.ValueText, v.Initializer.Value))
        //             .ToArray();
        //         var fieldDeclaration = _syntaxGenerator.FieldDeclaration(declarations);
        //         var classDeclaration = _syntaxGenerator.ClassDeclaration("GlobalScope", null, null, null, new[] { fieldDeclaration });
        //
        //         // Replace the local variable declaration with a reference to the global variable
        //         var globalScopeReference = _syntaxGenerator.MemberAccessExpression(_syntaxGenerator.IdentifierName("GlobalScope"), v.Identifier);
        //         var newAssignment = _syntaxGenerator.AssignmentStatement(globalScopeReference, v.Initializer.Value);
        //         var newLocalDeclaration = node.ReplaceNode(v, newAssignment);
        //         return base.VisitLocalDeclarationStatement(newLocalDeclaration);
        //     }
        // }
    }
}
