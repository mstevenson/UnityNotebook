using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace UnityNotebook
{
    // Unity Coroutine wrapper for capturing yielded values and displaying them as cell outputs in the Notebook window
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
                    RuntimeMethods.Show(output);
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
    }
}
