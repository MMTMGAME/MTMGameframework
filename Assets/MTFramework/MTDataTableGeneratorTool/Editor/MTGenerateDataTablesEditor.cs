using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameMain.Editor.DataTableTools;
using UnityEditor;
using UnityEngine;

public class MTGenerateDataTablesEditor : EditorWindow
{
    private static List<string> needConvertFiles;
    
    public static string GetScriptFolderPath(EditorWindow scriptWindow)
    {
        // 获取脚本的 MonoScript 对象
        MonoScript monoScript = MonoScript.FromScriptableObject(scriptWindow);

        // 获取脚本的相对路径
        string scriptPath = AssetDatabase.GetAssetPath(monoScript);

        // 返回脚本所在的文件夹路径
        return Path.GetDirectoryName(scriptPath);
    }
    public void GenerateDataTables(string file)//调用生成文件
    {
        MTDataTableProcessor dataTableProcessor = MTDataTableGenerator.CreateDataTableProcessor(file,Path.GetDirectoryName(file),GetScriptFolderPath(this)+"\\DataTableCodeTemplate.txt");

        if (!MTDataTableGenerator.CheckRawData(dataTableProcessor, file))
        {
            Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", file));
        }

        MTDataTableGenerator.GenerateDataFile(dataTableProcessor, file);
        MTDataTableGenerator.GenerateCodeFile(dataTableProcessor, file);

        AssetDatabase.Refresh();
    }

    
    
    // 添加一个菜单项
    [MenuItem("Game Framework/Generate DataTables")]
    public static void ShowWindow()
    {
        // 显示现有窗口实例。如果没有，则创建一个。
        EditorWindow.GetWindow(typeof(MTGenerateDataTablesEditor));
        needConvertFiles = new List<string>();
    }

    void OnGUI()
    {
        // 按钮

        
        // 拖放区域
        GUILayout.Box("把需要转换的txt表拖拽到此", GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));

        // 处理拖放事件
        EventType eventType = Event.current.type;
        bool isDragEvent = eventType == EventType.DragUpdated || eventType == EventType.DragPerform;
        if (isDragEvent && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            // 显示拖放视觉反馈
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var draggedObject in DragAndDrop.paths)
                {
                    // 处理拖放的对象
                    needConvertFiles.Add(draggedObject);
                }
            }
            // 消耗事件
            Event.current.Use();
        }
        GUILayout.Label("需要转换的txt,点击删除");
        for (int i = needConvertFiles.Count-1; i >=0; i--)
        {
            var split = needConvertFiles[i].Split("/");
            if (GUILayout.Button(split[split.Length - 1].Replace("\"", "")))
            {
                needConvertFiles.RemoveAt(i);
            }
        }
        
        if (GUILayout.Button("转换",GUILayout.Height(40)))
        {
            foreach (var VARIABLE in needConvertFiles)
            {
                GenerateDataTables(VARIABLE);
                File.Delete(VARIABLE+".bytes");
            }
            this.Close();
        }
    }
}