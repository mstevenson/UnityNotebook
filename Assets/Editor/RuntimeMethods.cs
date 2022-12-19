using Editor;
using UnityEngine;

public static class RuntimeMethods
{
    public static void Draw(object data)
    {
        if (NotebookWindowData.instance.runningCell != null)
        {
            AddDataToOutput(NotebookWindowData.instance.runningCell, data);
        }
    }
    
    private static void AddDataToOutput(Notebook.Cell cell, object data)
    {
        // TODO get the running cell
        
        switch (data)
        {
            case string s:
                break;
            case Vector3 v:
                break;
            case Vector2 v:
                break;
            case Quaternion q:
                break;
            case Matrix4x4 m:
                break;
            case AnimationCurve a:
                break;
            case Color c:
                break;
            case Texture2D t:
                break;
            case Material m:
                break;
            case Mesh m:
                break;
            default:
                break;
        }
    }
}
