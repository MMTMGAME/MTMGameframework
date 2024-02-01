using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity=GameMain.Entity;

public class EntityUiComponent : GameFrameworkComponent
{
    private Dictionary<Type, IObjectPool<EntityUiItemObject>> uiPools;
    private Dictionary<Entity, List<EntityUiItem>> activeUIElements;
    private Dictionary<Type, EntityUiItem> uiPrefabTemplates;

    private Dictionary<Type, Transform> canvases;

    public GameObject canvasTemplate;
    public EntityUiItem hpBarItemTemplate;
    public EntityUiItem interactItemTemplate;
    protected void Start()
    {
        uiPools = new Dictionary<Type, IObjectPool<EntityUiItemObject>>();
        activeUIElements = new Dictionary<Entity, List<EntityUiItem>>();
        uiPrefabTemplates = new Dictionary<Type, EntityUiItem>();
        
        canvases = new Dictionary<Type, Transform>();

        // 初始化各种类型的UI元素池
        InitializeUiPools();
        
        

        //为每种UI元素类型设置预制体模板
        //例如：
        uiPrefabTemplates[typeof(HPBarItem)] = hpBarItemTemplate;
        uiPrefabTemplates[typeof(InteractTipItem)] = interactItemTemplate;
        
    }

    
    public void ShowUI<T>(Entity entity, params object[] args) where T : EntityUiItem, new()
    {
        if (entity == null)
        {
            Log.Warning("Entity is invalid.");
            return;
        }

        T uiElement = GetOrCreateUIElement<T>(entity);
        uiElement.Init(entity, args);

        if (!activeUIElements.ContainsKey(entity))
        {
            activeUIElements[entity] = new List<EntityUiItem>();
        }
        activeUIElements[entity].Add(uiElement);
    }

    public void HideUI<T>(Entity entity) where T : EntityUiItem
    {
        if (entity == null || !activeUIElements.ContainsKey(entity))
        {
            return;
        }

        var uiItems = activeUIElements[entity];
        for (int i = uiItems.Count - 1; i >= 0; i--)
        {
            if (uiItems[i] is T)
            {
                T uiItem = (T)uiItems[i];
                uiItem.Reset();
                uiPools[typeof(T)].Unspawn(uiItem);
                //pool.Unspawn(uiItem);
                uiItems.RemoveAt(i);
            }
        }
    }
   

    private T GetOrCreateUIElement<T>(Entity entity) where T : EntityUiItem, new()
    {
        IObjectPool<EntityUiItemObject> pool;
        if (!uiPools.TryGetValue(typeof(T), out pool))
        {
            // 创建新的对象池
            pool = CreateUiPool<T>();
            uiPools[typeof(T)] = pool;
        }

        var uiItemObject = pool.Spawn();
        T uiItem;
        if (uiItemObject != null)
        {
            uiItem = (T)uiItemObject.Target;
        }
        else
        {
            // 使用预制体模板创建一个新的UI元素实例
            if (uiPrefabTemplates.TryGetValue(typeof(T), out EntityUiItem prefab))
            {
                uiItem = Instantiate(prefab) as T;
            }
            else
            {
                throw new InvalidOperationException($"No prefab template registered for UI element type {typeof(T)}.");
            }
        
            // 根据需要设置UI元素的属性
            var transform = uiItem.GetComponent<Transform>();
            transform.SetParent(canvases[typeof(T)]);
            transform.localScale = Vector3.one;

            // 注册新创建的UI元素到对象池中
            pool.Register(EntityUiItemObject.Create(uiItem), true);
        }

        return uiItem;
    }

    private void InitializeUiPools()
    {
        // 初始化UI对象池，例如：
         uiPools[typeof(HPBarItem)] = CreateUiPool<HPBarItem>();
         uiPools[typeof(InteractTipItem)] = CreateUiPool<InteractTipItem>();


         canvases.Add(typeof(HPBarItem),GameObject.Instantiate(canvasTemplate, Vector3.zero, Quaternion.identity, transform).transform);
         canvases.Add(typeof(InteractTipItem),GameObject.Instantiate(canvasTemplate, Vector3.zero, Quaternion.identity, transform).transform);
         
         // var hpBarCanvas = ;
         // hpBarCanvas.name = "HpBarItemCanvas";
         // canvases [typeof(HPBarItem)] = hpBarCanvas.transform;
         
    }

    private IObjectPool<EntityUiItemObject> CreateUiPool<T>() where T : EntityUiItem
    {
        return  GameMain.GameEntry.ObjectPool.CreateSingleSpawnObjectPool<EntityUiItemObject>(uiPools.Count.ToString(), 8);
    }

    protected void Update()
    {
        foreach (var pair in activeUIElements)
        {
            for (int i = pair.Value.Count - 1; i >= 0; i--)
            {
                var uiItem = pair.Value[i];
                if (!uiItem.Refresh())
                {
                    if (uiItem is HPBarItem)
                    {
                        HideUI<HPBarItem>(pair.Key);
                        continue;
                    }

                    if (uiItem is InteractTipItem)
                    {
                        HideUI<InteractTipItem>(pair.Key);
                        continue;
                    }
                }
            }
        }

    }

    // 其他通用方法...
}
