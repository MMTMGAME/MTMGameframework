using UnityEngine;
using UnityEditor;

public class PrintPathTool
{
    [MenuItem("Custom/Print Selected Object Path")]
    private static void PrintSelectedObjectPath()
    {
        if (Selection.activeTransform != null)
        {
            string path = GetGameObjectPath(Selection.activeTransform);
            Debug.Log("Selected Object Path: " + path);
        }
        else
        {
            Debug.Log("No object selected in the hierarchy.");
        }
    }

    private static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}