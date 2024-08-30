using UnityEditor;
using UnityEngine;

public class QuickNavigation : EditorWindow
{
    // 添加菜单项到Unity的Window菜单
    [MenuItem("Window/Quick Navigation")]
    public static void ShowWindow()
    {
        // 显示现有窗口实例。如果没有，就创建一个。
        EditorWindow.GetWindow(typeof(QuickNavigation), false, "Quick Navigation");
    }

    void OnGUI()
    {
        // 按钮用于快速导航到材质文件夹
        if (GUILayout.Button("Go to BattleUnits Folder"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/Entities/BattleUnit/Player", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        // 按钮用于快速导航到模型文件夹
        if (GUILayout.Button("Scenes"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/Scenes/Level_1", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        // 按钮用于快速导航到模型文件夹
        if (GUILayout.Button("Launcher"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/Launcher", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        if (GUILayout.Button("StateGraphs"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/StateGraphs/SubGraph", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        if (GUILayout.Button("SoDataTable"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/DataTables/SoDataTable/Scripts", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        if (GUILayout.Button("GameMainSounds"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/GameMain/Sounds/BulletImpact", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        // 按钮用于快速导航到材质文件夹
        if (GUILayout.Button("Launcher Scene"))
        {
            // 设置Project窗口的当前选择路径
            Selection.activeObject = AssetDatabase.LoadAssetAtPath("Assets/Launcher.unity", typeof(Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}