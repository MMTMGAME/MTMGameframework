using System.Collections;
using System.Collections.Generic;
using System.Resources;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class ShowEntityUiItemInfo 
{
    public int serialId;
    public Entity entity;
    public int typeId;

   

    public ShowEntityUiItemInfo(int serialId, Entity entity, int typeId)
    {
        this.serialId = serialId;
        this.entity = entity;
        this.typeId = typeId;
    }

    public void Clear()
    {
        serialId = 0;
        entity = null;
    }
}

/// <summary>
/// 管理所有Enitty的跟踪型UI，没有使用对象池，因为ugf的对象池要求每中类型单独使用一个对象池，那样的话没办法用一个类管理所有EntityUI,需要硬编码代码，
/// 用反射的话性能消耗反而会更高，而且有些地方即使用反射也需要硬编码才能支持脱战性的EntityUi
/// </summary>
public class EntityUiComponent : GameFrameworkComponent
{
    public Dictionary<Entity, List<EntityUiItem>> activeUiElements = new Dictionary<Entity, List<EntityUiItem>>();
    //public List<EntityUiItem> allUis = new List<EntityUiItem>();
    
    private IResourceManager resourceManager;
    
    private  LoadAssetCallbacks loadAssetCallbacks;

    private int serialId;

    public Transform canvas;
    // Start is called before the first frame update
    void Start()
    {
        loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
       
        BaseComponent baseComponent = GameEntry.Base;
        if (baseComponent == null)
        {
            Log.Fatal("Base component is invalid.");
            return;
        }

       

        if (baseComponent.EditorResourceMode)
        {
            resourceManager = baseComponent.EditorResourceHelper;
        }
        else
        {
            resourceManager=GameFrameworkEntry.GetModule<IResourceManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var value in activeUiElements.Values)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (!value[i].died &&  !value[i].Refresh())
                {
                    value[i].Die();
                    value.RemoveAt(i);
                    i--;
                }
            }
        }
       
    }

    public void HideUi(int uiSerialId)
    {
        foreach (var value in activeUiElements.Values)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (!value[i].died)
                {
                    value[i].Die();
                    value.RemoveAt(i);
                    return;
                }
            }
        }
        
    }
    

    public void HideUis(Entity entity, int typeId)
    {
        if (activeUiElements.TryGetValue(entity, out var uis))
        {
            for (int i = uis.Count - 1; i >= 0; i--)
            {
                if (uis[i].typeId == typeId)
                {
                    Destroy(uis[i].gameObject);
                    uis.RemoveAt(i);
                }
            }
        }
    }

    public void ShowEntityUi(object userData)
    {
        ShowEntityUiItemInfo showEntityUiItemInfo=userData as ShowEntityUiItemInfo;
        int typeId = showEntityUiItemInfo.typeId;
        IDataTable<DREntityUi> table = GameEntry.DataTable.GetDataTable<DREntityUi>();
        var element = table.GetDataRow(typeId);

        bool needNewInstance=false;
        bool isSingleton = element.IsSingleton;
        if (isSingleton)
        {
           
            if (showEntityUiItemInfo == null || showEntityUiItemInfo.entity == null)
            {
                Log.Fatal("userData 错误");
                return;
            }

            if (activeUiElements.TryGetValue(showEntityUiItemInfo.entity, out var uis))
            {

                foreach (var ui in uis)
                {
                    if (ui.typeId == typeId)
                    {
                        InternalShowEntityUi(ui.gameObject,userData);
                        
                        
                        
                        return;
                    }
                }
                
               
            }
            needNewInstance = true;
        }
        
        if(!needNewInstance)
            return;
        
        var assetName = AssetUtility.GetEntityUiAsset(element.AssetName);
        
        resourceManager.LoadAsset(assetName, 100, loadAssetCallbacks, userData);

    }

    void InternalShowEntityUi(object instance,object userData)
    {
        GameObject go = instance as GameObject;
        if (go == null)
        {
            Log.Fatal("Entity UI instance 无效.");
            return;
        }

        Transform uiTransform = go.transform;
        uiTransform.SetParent(canvas);
        

        EntityUiItem entityUiItem = go.GetComponent<EntityUiItem>();
        
        entityUiItem.Init(userData);
        
       
        
        var data = userData as ShowEntityUiItemInfo;
        if (activeUiElements.ContainsKey(data.entity))
        {
            activeUiElements[data.entity].Add(entityUiItem);
        }
        else
        {
            activeUiElements[data.entity] = new List<EntityUiItem>();
            activeUiElements[data.entity].Add(entityUiItem);
        }
    }

    private void LoadAssetSuccessCallback(string assetName, object asset, float duration, object userData)
    {
        ShowEntityUiItemInfo showEntityUiItemInfo=userData as ShowEntityUiItemInfo;
        if (showEntityUiItemInfo == null)
        {
            Log.Fatal($"生成EntityUI{assetName}失败");
            return;
        }
        
        var instance=GameObject.Instantiate((Object)asset);
        
        InternalShowEntityUi(instance,userData);

    }

    private void LoadAssetFailureCallback(string assetName, LoadResourceStatus status, string errorMessage,
        object userData)
    {
        Log.Fatal($"加载{assetName}[{(userData as ShowEntityUiItemInfo).serialId}]失败，状态:{status},Message:{errorMessage}");
    }
    
    
    private void LoadAssetUpdateCallback(string assetName, float progress, object userData)
    {
        
    }

    private void LoadAssetDependencyAssetCallback(string assetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
    {
       
    }

    public int GenerateSerialId()
    {
        return ++serialId;
    }
}
