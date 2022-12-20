using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

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
        EditorCoroutineUtility.StopCoroutine(_editorCoroutine);
        _editorCoroutine = null;
    }

    private static IEnumerator StartCoroutineWithReturnValues(IEnumerator routine)
    {
        yield return RunInternal(routine, output =>
        {
            if (output != null && output is not YieldInstruction && output is not EditorWaitForSeconds)
            {
                RuntimeMethods.Show(output);
            }
        });
        NotebookWindowData.instance.RunningCell = -1;
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
            yield return result;
        }
    }
}
