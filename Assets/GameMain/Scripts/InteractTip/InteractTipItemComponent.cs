using System.Collections;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using GameMain;
using UnityEngine;
using UnityEngine.Serialization;
using UnityGameFramework.Runtime;
using Entity =GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class InteractTipItemComponent : GameFrameworkComponent
{
    public Canvas cachedCanvas;

    [FormerlySerializedAs("interactTopTemplate")] public InteractTipItem interactTipTemplate;

    private IObjectPool<InteractTipItemObject> pool;

    private List<InteractTipItem> activeItemList;
    // Start is called before the first frame update
    void Start()
    {
        pool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<InteractTipItemObject>("InteractTipItem");
        activeItemList = new List<InteractTipItem>();
        
    }

    public void ShowInteractTip(Entity entity,string msg)
    {
        if (entity == null)
        {
            Log.Warning("Entity is invalid.");
            return;
        }

        InteractTipItem interactTipItem = GetActiveItem(entity);
        if (interactTipItem == null)
        {
            interactTipItem = CreatItem(entity);
            activeItemList.Add(interactTipItem);
        }

        interactTipItem.Init(entity, cachedCanvas, msg);
    }

    private void Update()
    {
        for (int i = activeItemList.Count - 1; i >= 0; i--)
        {
            InteractTipItem item = activeItemList[i];
            item.Refresh();

          
        }
    }

    public InteractTipItem CreatItem(Entity entity)
    {
        InteractTipItem interactTipItem;
        var interactTipItemObject = pool.Spawn();
        if (interactTipItemObject != null)
            return interactTipItemObject.Target as InteractTipItem;
        else
        {
            interactTipItem = Instantiate(interactTipTemplate);
            interactTipItem.transform.SetParent(cachedCanvas.transform);
            interactTipItem.transform.localScale=Vector3.one;
            pool.Register(InteractTipItemObject.Create(interactTipItem),true );
            return interactTipItem;
        }
    }

    public void HideItem(Entity entity)
    {
        for (int i = 0; i < activeItemList.Count; i++)
        {
            if (activeItemList[i].Owner == entity)
            {
                var item = activeItemList[i];
                activeItemList.Remove(item);
                item.Reset();
                pool.Unspawn(item);
            }
        }
    }
    
    
    public InteractTipItem GetActiveItem(Entity entity)
    {
        if (entity == null)
        {
            return null;
        }

        for (int i = 0; i < activeItemList.Count; i++)
        {
            if (activeItemList[i].Owner == entity)
            {
                return activeItemList[i];
            }
        }

        return null;
    }
}
