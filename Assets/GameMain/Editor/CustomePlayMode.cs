using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class CustomPlayMode
{
    private const string PrefKeyDefaultStartScenePath = "CustomPlayMode_DefaultStartScenePath";
    private const string PrefKeySceneToReturnTo = "CustomPlayMode_SceneToReturnTo";
    private const string PrefKeyAutoJumpEnabled = "CustomPlayMode_AutoJumpEnabled"; // 新增

    static CustomPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!AutoJumpEnabled) // 检查是否启用自动跳转
        {
            return;
        }

        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                string currentScene = SceneManager.GetActiveScene().path;
                EditorPrefs.SetString(PrefKeySceneToReturnTo, currentScene);
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(DefaultStartScenePath);
                break;

            case PlayModeStateChange.EnteredEditMode:
                EditorApplication.update += ReturnToOriginalScene;
                break;
        }
    }

    private static void ReturnToOriginalScene()
    {
        EditorApplication.update -= ReturnToOriginalScene;

        if (!AutoJumpEnabled) // 再次检查是否启用自动跳转
        {
            return;
        }

        string sceneToReturnTo = EditorPrefs.GetString(PrefKeySceneToReturnTo, "");
        if (!string.IsNullOrEmpty(sceneToReturnTo))
        {
            EditorSceneManager.OpenScene(sceneToReturnTo);
        }
    }

    [MenuItem("Custom/Start Scene Settings/Set Default Start Scene")]
    private static void SetDefaultStartScene()
    {
        string scenePath = EditorUtility.OpenFilePanel("Select Start Scene", "Assets", "unity");
        if (!string.IsNullOrEmpty(scenePath))
        {
            string relativePath = FileUtil.GetProjectRelativePath(scenePath);
            if (!string.IsNullOrEmpty(relativePath))
            {
                DefaultStartScenePath = relativePath;
            }
        }
    }

    [MenuItem("Custom/Start Scene Settings/Toggle Auto Jump")] // 新增，使用快捷键 Ctrl+Shift+J 或 Cmd+Shift+J
    private static void ToggleAutoJump()
    {
        AutoJumpEnabled = !AutoJumpEnabled;
        Menu.SetChecked("Custom/Start Scene Settings/Toggle Auto Jump", AutoJumpEnabled);
        EditorPrefs.SetBool(PrefKeyAutoJumpEnabled, AutoJumpEnabled);
    }

    private static string DefaultStartScenePath
    {
        get => EditorPrefs.GetString(PrefKeyDefaultStartScenePath, "Assets/YourStartScenePath.unity");
        set => EditorPrefs.SetString(PrefKeyDefaultStartScenePath, value);
    }

    private static bool AutoJumpEnabled
    {
        get => EditorPrefs.GetBool(PrefKeyAutoJumpEnabled, true);
        set =>
            EditorPrefs.SetBool(PrefKeyAutoJumpEnabled, value);
    }
}