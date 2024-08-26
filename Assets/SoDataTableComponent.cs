using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework;
using GameFramework.Resource;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;

public class SoDataTableComponent : GameFrameworkComponent
{
    // 在 Inspector 中管理的 SoDataTable 列表
    [SerializeField]
    private List<SoDataTable<SoDataRow>> soDataTables = new List<SoDataTable<SoDataRow>>();

    // 用于快速查找的 Dictionary，Type 作为键
    private Dictionary<Type, SoDataTable<SoDataRow>> soDataTableDict;

    // 在游戏开始时从列表构建 Dictionary
    private void Awake()
    {
        soDataTableDict = new Dictionary<Type, SoDataTable<SoDataRow>>();

        // 将列表转换为 Dictionary，类型作为键
        foreach (var table in soDataTables)
        {
            Type tableType = table.GetType();

            if (!soDataTableDict.ContainsKey(tableType))
            {
                soDataTableDict.Add(tableType, table);
            }
            else
            {
                Debug.LogWarning($"Table of type {tableType} already exists.");
            }
        }
    }

    // 泛型方法，返回特定类型的 SoDataTable<T>
    public SoDataTable<T> GetTable<T>() where T : SoDataRow
    {
        Type tableType = typeof(SoDataTable<T>);
        
        if (soDataTableDict.TryGetValue(tableType, out SoDataTable<SoDataRow> table))
        {
            return table as SoDataTable<T>;
        }

        Debug.LogWarning($"Table of type {tableType} not found.");
        return null;
    }
   
}
