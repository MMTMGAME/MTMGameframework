using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.ObjectPool;
using UnityEngine;
using UnityGameFramework.Runtime;

public class InterfacePool : GameFrameworkComponent
{
    public int count;
    private Dictionary<int, Dictionary<Type, List<Component>>> interfaceContainer;
    protected override void Awake()
    {
        base.Awake();
        interfaceContainer = new();
    }

    public void Remove(int instanceid)
    {
        if (interfaceContainer.ContainsKey(instanceid))
        {
            interfaceContainer.Remove(instanceid);
            count--;
        }
    }

    /// <summary>
    /// 获取对象身上的所有对应接口或接口
    /// </summary>
    /// <param name="instanceId"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T[] GetIf_Cp<T>(int instanceId) where T : class
    {
        if (interfaceContainer.TryGetValue(instanceId, out var componentsDict))
        {
            if (componentsDict.TryGetValue(typeof(T), out var component))
            {
                return component.Cast<T>().ToArray();
            }
        }
        return null; 
    }
    
    /// <summary>
    /// 获取对象身上的对应第一个接口或者第一个组件
    /// </summary>
    /// <param name="instanceId"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetIf_CpFirst<T>(int instanceId) where T : class
    {
        if (interfaceContainer.TryGetValue(instanceId, out var componentsDict))
        {
            if (componentsDict.TryGetValue(typeof(T), out var component))
            {
                return component.FirstOrDefault() as T;
            }
        }
        return default; 
    }
    /// <summary>
    /// 安全获得对象身上的对应第一个接口或者第一个组件
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="get"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryGetIf_CpFirst<T>(int instanceId,out T get) where T : class
    {
        if (interfaceContainer.TryGetValue(instanceId, out var componentsDict))
        {
            if (componentsDict.TryGetValue(typeof(T), out var component))
            {
                get = component.FirstOrDefault() as T;
                return true;
            }
        }

        get = default;
        return false;
    }
    
    /// <summary>
    /// 根据ID获取物体
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    public GameObject GetGameObj(int instanceId)
    {
        return GetIf_Cp<Transform>(instanceId)[0].gameObject;
    }
    /// <summary>
    /// 安全根据ID获取物体
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    public bool TryGetGameObj(int instanceId, out GameObject gameObj)
    {
        gameObj = GetGameObj(instanceId);
        return gameObj != null;
    }
    
    /// <summary>
    /// 注册物体
    /// </summary>
    /// <param name="prefab"></param>
    public void RegisterGameObject(GameObject prefab)
    {
        var instanceId = prefab.GetInstanceID();
        CacheInterfaces(instanceId,prefab);

    }
    /// <summary>
    /// 获取物体身上所有接口
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="monoBehaviour"></param>
    public void CacheInterfaces(int instanceId, GameObject gameObject)
    {
        if (!interfaceContainer.ContainsKey(instanceId))
        {
            interfaceContainer[instanceId] = new Dictionary<Type, List<Component>>();
            count++;
        }
        else
        {
            return;
        }

        var components = gameObject.GetComponents<Component>();
        foreach(var component in components)
        {
            var interfaces = component.GetType().GetInterfaces();
            foreach(var intf in interfaces)
            {
                if (!interfaceContainer[instanceId].ContainsKey(intf))
                {
                    interfaceContainer[instanceId][intf] = new List<Component>();
                }
                Debug.Log(intf);
                interfaceContainer[instanceId][intf].Add(component);
            }

            var typename = component.GetType();
            if (!interfaceContainer[instanceId].ContainsKey(typename))
            {
                interfaceContainer[instanceId][typename] = new List<Component>();
            }
            Debug.Log(typename);
            interfaceContainer[instanceId][component.GetType()].Add(component);

        }
    }

    
    public T[] GetAllComponentsByInterface<T>()
    {
        var interfaceType = typeof(T);
        List<T> getallinterface = new List<T>();

        foreach (var prefab in interfaceContainer.Values)
        {
            foreach (var kvp in prefab)
            {
                if (kvp.Key.GetInterfaces().Contains(interfaceType))
                {
                    getallinterface.Concat(kvp.Value.Cast<T>());
                }
            }
        }

        return getallinterface.ToArray();
    }
}
