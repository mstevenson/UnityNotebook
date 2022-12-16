
using UnityEngine;

public class Evaluator
{
    public static bool IsRunning { get; private set; }
    
    public static void Evaluate(Notebook.Cell cell)
    {
        IsRunning = true;
        Debug.Log("evaluate cell");
        IsRunning = false;
    }

    public static void Stop()
    {
        
    }
}
