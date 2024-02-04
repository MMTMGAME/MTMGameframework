#if UNITY_EDITOR_WIN
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsViewer : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("GameMain/PlayerPrefs Viewer")]
    private static void Init()
    {
        var window = (PlayerPrefsViewer)EditorWindow.GetWindow(typeof(PlayerPrefsViewer), false, "PlayerPrefs Viewer");
        window.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            Repaint();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        string registryKeyPath = $"Software\\Unity\\UnityEditor\\{PlayerSettings.companyName}\\{PlayerSettings.productName}";
        using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(registryKeyPath))
        {
            if (registryKey != null)
            {
                foreach (string valueName in registryKey.GetValueNames())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(valueName, GUILayout.Width(200));
                    object value = registryKey.GetValue(valueName);
                    GUILayout.Label(value.ToString());
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No PlayerPrefs found or cannot access the registry.");
            }
        }

        GUILayout.EndScrollView();
    }
}
#endif