using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.ObjectPool;
using UnityEngine;
using UnityGameFramework.Runtime;

public class FlyTextComponent : GameFrameworkComponent
{
    public Canvas canvas;
    public GameObject flyTextPfb;

    [SerializeField]
    private int poolCapacity = 16;

    private int serialId;

    private Camera mainCam;

    private Camera MainCam
    {
        get
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }

            return mainCam;
        }
        set => mainCam = value;
    }

    private IObjectPool<FlyTextUIObject> flyTextUIPool = null;
    private List<FlyTextUI> activeUIs=new List<FlyTextUI>();
    void Start()
    {
        flyTextUIPool=GameMain.GameEntry.ObjectPool.CreateSingleSpawnObjectPool<FlyTextUIObject>("FlyTextUI", poolCapacity);
        flyTextUIPool.AutoReleaseInterval = 5;
        flyTextUIPool.ExpireTime = 3;
    }

    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1)
        {
            for (int i = 0; i < activeUIs.Count; i++)
            {
                if (activeUIs[i] != null && activeUIs[i].done)
                {
                    activeUIs[i].OnRecycle();
                    flyTextUIPool.Unspawn(activeUIs[i]);
                    activeUIs.RemoveAt(i);
                    i--;
                }
            }

            timer = 0;
        }
        
    }

    public void FlyText(Vector3 worldPos, string msg, Color color, float jumpHeight, float duration)
    {
        // 通过GetFlyTextUi实例化一个新的UI
        FlyTextUI flyTextUI = GetFlyTextUi();
        activeUIs.Add(flyTextUI);
        flyTextUI.name += --serialId;

        // 调用UI的Init方法，ui会在对应世界坐标生成，并执行上升动画
        flyTextUI.Init(worldPos, msg, color,canvas, jumpHeight, duration,MainCam);
    }

    private FlyTextUI GetFlyTextUi()
    {
       
            FlyTextUI flyTextUI = null;
            FlyTextUIObject flyTextUIObject = flyTextUIPool.Spawn();
            if (flyTextUIObject != null)
            {
                flyTextUI = (FlyTextUI)flyTextUIObject.Target;
            }
            else
            {
                var go = Instantiate(flyTextPfb);
                flyTextUI = go.GetComponent<FlyTextUI>();
                
                flyTextUI.transform.SetParent(canvas.transform);
                flyTextUI.transform.localScale = Vector3.one;
                flyTextUIPool.Register(FlyTextUIObject.Create(flyTextUI), true);
            }

            return flyTextUI;
        
    }
}
