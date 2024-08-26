using System.Collections;
using System.Collections.Generic;
using System.Resources;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using AssetUtility = GameMain.AssetUtility;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;


public class LoadStateGraphInfo
{
    public int EntityId { get; private set; }
    public string AssetName{ get; private set; }

    public LoadStateGraphInfo(int entityId, string assetName)
    {
        EntityId = entityId;
        AssetName = assetName;
    }
}
public class AIComponent : GameFrameworkComponent
{
    private IResourceManager resourceManager;
    
    private  LoadAssetCallbacks loadAssetCallbacks;

    
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


    public void AttachStateGraphToEntityByName(LoadStateGraphInfo userData)
    {
        var assetName = AssetUtility.GetStateGraphAsset(userData.AssetName);
        
        resourceManager.LoadAsset(assetName, 100, loadAssetCallbacks, userData);
    }
    
    public void AttachStateGraphToEntityByAsset(StateGraphAsset stateGraphAsset,LoadStateGraphInfo userData)
    {
        LoadStateGraphInfo loadStateGraphInfo=userData as LoadStateGraphInfo;
        if (loadStateGraphInfo == null)
        {
            Log.Fatal($"实例化状态机失败");
            return;
        }

        var entity = GameEntry.Entity.GetEntity(loadStateGraphInfo.EntityId);
        if (entity != null && entity.Handle is GameObject go)
        {
            var stateMachine = go.AddComponent<StateMachine>();
            //设置graph
            stateMachine.nest.SwitchToMacro((StateGraphAsset)stateGraphAsset);
            
        }
    }
    
    private void LoadAssetSuccessCallback(string assetName, object asset, float duration, object userData)
    {
        LoadStateGraphInfo loadStateGraphInfo=userData as LoadStateGraphInfo;
        if (loadStateGraphInfo == null)
        {
            Log.Fatal($"实例化状态机{assetName}失败");
            return;
        }
        
       

        var entity = GameEntry.Entity.GetEntity(loadStateGraphInfo.EntityId);
        if (entity != null && entity.Handle is GameObject go)
        {
            var stateMachine = go.AddComponent<StateMachine>();
            //设置graph
            stateMachine.nest.SwitchToMacro((StateGraphAsset)asset);
            
        }
        

    }

    private void LoadAssetFailureCallback(string assetName, LoadResourceStatus status, string errorMessage,
        object userData)
    {
        Log.Fatal($"加载{assetName}失败，状态:{status},Message:{errorMessage}");
    }
    
    
    private void LoadAssetUpdateCallback(string assetName, float progress, object userData)
    {
        
    }

    private void LoadAssetDependencyAssetCallback(string assetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
    {
       
    }
}
