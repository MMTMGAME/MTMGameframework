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
    // 在 Inspector 中管理不同类型的 SoDataTable 列表
    [SerializeField]
    private List<SoDataTableBase> soDataTables = new List<SoDataTableBase>();

    // 用于快速查找的 Dictionary，Type 作为键
    private Dictionary<Type, SoDataTableBase> soDataTableDict;

    // 在游戏开始时从列表构建 Dictionary
    private void Awake()
    {
        base.Awake();
        soDataTableDict = new Dictionary<Type, SoDataTableBase>();

        // 将列表转换为 Dictionary，以 SoDataRow 的类型为键
        foreach (var table in soDataTables)
        {
            // 获取泛型类型的参数，即 SoDataRow 类型
            Type rowType = GetSoDataRowType(table.GetType());

            if (rowType != null)
            {
                if (!soDataTableDict.ContainsKey(rowType))
                {
                    soDataTableDict.Add(rowType, table);
                }
                else
                {
                    Debug.LogError($"Table of row type {rowType} already exists.");
                }
            }
            else
            {
                Debug.LogError($"Failed to get row type for table: {table.name}");
            }
        }
    }

// 通过反射获取 SoDataTable<T> 中的泛型参数 T，即 SoDataRow 类型
    private Type GetSoDataRowType(Type tableType)
    {
        while (tableType != null && tableType.BaseType != null)
        {
            if (tableType.BaseType.IsGenericType && tableType.BaseType.GetGenericTypeDefinition() == typeof(SoDataTable<>))
            {
                // 获取 SoDataTable<T> 中的 T 类型
                return tableType.BaseType.GetGenericArguments()[0];
            }

            tableType = tableType.BaseType;
        }

        return null;
    }


    // 泛型方法，返回特定类型的 SoDataTable<T>
    public SoDataTable<T> GetTable<T>() where T : SoDataRow
    {
        Type rowType = typeof(T);

        if (soDataTableDict.TryGetValue(rowType, out SoDataTableBase table))
        {
            return table as SoDataTable<T>;
        }

        Debug.LogError($"Table of row type {rowType} not found.");
        return null;
    }


    public T GetSoDataRow<T>(int id) where T : SoDataRow
    {
        // 获取对应类型的 SoDataTable<T>
        var table = GetTable<T>();

        if (table != null)
        {
            // 遍历表中的数据行，查找对应 ID 的 SoDataRow
            foreach (var row in table.soDataRows)
            {
                if (row.id == id)
                {
                    return row;
                }
            }

            Debug.LogError($"Row with ID {id} not found in table of row type {typeof(T)}.");
        }
        else
        {
            Debug.LogError($"Table of row type {typeof(T)} not found.");
        }

        return null;
    }

}
