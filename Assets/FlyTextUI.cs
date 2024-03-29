using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.ObjectPool;
using GameMain;
using UnityEngine;
using UnityEngine.UI;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

public class FlyTextUIObject : ObjectBase
{
    public static FlyTextUIObject Create(object target)
    {
        FlyTextUIObject flyTextUIObject = ReferencePool.Acquire<FlyTextUIObject>();
        flyTextUIObject.Initialize(target);
        return flyTextUIObject;
    }

    protected override void Release(bool isShutdown)
    {
        FlyTextUI flyTextUI = (FlyTextUI)Target;
        if (flyTextUI == null)
        {
            return;
        }

        Object.Destroy(flyTextUI.gameObject);
    }
}

public class FlyTextUI : MonoBehaviour
{
    public Text textComponent; // 假设有一个Text组件用于显示消息
    private Vector3 worldPosition;
    private float elapsedTime = 0f;
    private float jumpHeight;
    private float duration;
    private Canvas canvas;

    public void Init(Vector3 worldPos, string msg, Color color, Canvas canvas,float jumpHeight, float duration)
    {
        textComponent.text = msg;
        textComponent.color = color;
        
        this.worldPosition = worldPos;
        this.jumpHeight = jumpHeight;
        this.duration = duration;
        this.canvas = canvas;

        gameObject.SetActive(true);
        

    }

    public bool done;
    void Update()
    {
        // 计算已过时间比例
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        // 计算当前上升高度
        float currentHeight = Mathf.Lerp(0, jumpHeight, t);

        // 更新世界位置
        Vector3 updatedWorldPos = new Vector3(worldPosition.x, worldPosition.y + currentHeight, worldPosition.z);

        // 将更新后的世界坐标转换为Canvas内的坐标
        Vector2 screenPos = Camera.main.WorldToScreenPoint(updatedWorldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 anchoredPos);
        transform.localPosition = anchoredPos;

        // 如果动画时间结束，可以进行一些收尾工作，比如隐藏或回收UI元素
        if (elapsedTime >= duration)
        {
            // 动画完成后的处理
            gameObject.SetActive(false);
           
            done = true;
        }
        
    }
    public void OnRecycle()
    {
        elapsedTime = 0;
        done = false;
    }
    
}

