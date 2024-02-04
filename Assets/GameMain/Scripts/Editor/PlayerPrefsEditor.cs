using UnityEngine;
using UnityEditor;

public class PlayerPrefsEditor : EditorWindow
{
    private string key = "";
    private int intValue = 0;
    private float floatValue = 0.0f;
    private string stringValue = "";
    private bool boolValue = false;

    [MenuItem("GameMain/PlayerPrefs Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PlayerPrefsEditor));
    }

    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Settings", EditorStyles.boldLabel);
        key = EditorGUILayout.TextField("Key", key);

        EditorGUILayout.Space();

        intValue = EditorGUILayout.IntField("Int Value", intValue);
        if (GUILayout.Button("Set Int"))
        {
            PlayerPrefs.SetInt(key, intValue);
        }

        floatValue = EditorGUILayout.FloatField("Float Value", floatValue);
        if (GUILayout.Button("Set Float"))
        {
            PlayerPrefs.SetFloat(key, floatValue);
        }

        stringValue = EditorGUILayout.TextField("String Value", stringValue);
        if (GUILayout.Button("Set String"))
        {
            PlayerPrefs.SetString(key, stringValue);
        }

        boolValue = EditorGUILayout.Toggle("Bool Value", boolValue);
        if (GUILayout.Button("Set Bool"))
        {
            PlayerPrefs.SetInt(key, boolValue ? 1 : 0);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Save PlayerPrefs"))
        {
            PlayerPrefs.Save();
        }

        if (GUILayout.Button("Delete Key"))
        {
            PlayerPrefs.DeleteKey(key);
        }

        if (GUILayout.Button("Delete All PlayerPrefs"))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}