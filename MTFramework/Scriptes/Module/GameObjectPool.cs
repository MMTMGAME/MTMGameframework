using System.Collections;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameObjectPoolContainer:ObjectBase
{
    public void SetGameObject(GameObject go)
    {
        Initialize(go.gameObject.name,go);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        ((GameObject)Target).SetActive(true);
    }

    protected override void OnUnspawn()
    {
        base.OnUnspawn();
        ((GameObject)Target).SetActive(false);
    }

    protected override void Release(bool isShutdown)
    {
        Object.Destroy((GameObject)Target);
    }
}

public class GameObjectPool:GameFrameworkComponent
{
    private Dictionary<GameObject,IObjectPool<GameObjectPoolContainer>> allGo;
    [SerializeField]
    private List<GameObject> pools;
    
    protected override void Awake()
    {
        base.Awake();
        pools = new List<GameObject>();
        allGo = new Dictionary<GameObject,IObjectPool<GameObjectPoolContainer>>();
    }
    /// <summary>
    /// 创建对象池并生成这个对象。
    /// </summary>
    /// <param name="prefab">预制体对象</param>
    public GameObject Spawn(GameObject prefab)
    {
        GameObjectPoolContainer spawned = null;
        IObjectPool<GameObjectPoolContainer> newpool = null;
        if (!allGo.ContainsKey(prefab))
        {
            newpool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<GameObjectPoolContainer>(prefab.name);
            allGo.Add(prefab, newpool);
            pools.Add(prefab);
        }
        else newpool = allGo[prefab];
        
        spawned = newpool.Spawn();
        if (spawned != null)
        {
            return (GameObject)spawned.Target;
        }
        else
        {
            spawned = new GameObjectPoolContainer();
            GameObject obj = Instantiate(prefab);
            obj.name = obj.name.Replace("(Clone)", "");
            spawned.SetGameObject(obj);
            newpool.Register(spawned,true);
        }
        return (GameObject)spawned.Target;
    }
    /// <summary>
    /// 把对象放回对象池
    /// </summary>
    /// <param name="go">对象</param>
    public void Despawn(GameObject go)
    {
        GameEntry.ObjectPool.GetObjectPool<GameObjectPoolContainer>(go.name).Unspawn(go);
    }
    /// <summary>
    /// 释放这个对象
    /// </summary>
    /// <param name="go">对象</param>
    public void ReleaseObject(GameObject go)
    {
        GameEntry.ObjectPool.GetObjectPool<GameObjectPoolContainer>(go.name).ReleaseObject(go);
    }
}
