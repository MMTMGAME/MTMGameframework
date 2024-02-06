using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;

public class Road : Entity
{
    private RoadData roadData;

   
    public bool triggered;
    public float triggerTime;

    private Rigidbody rigidbody;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        userData = (RoadData)userData;

        //对象池复用
        triggered = false;
        triggerTime = 0;
    }

    //坠落后Hide
    public void Fall()
    {
        transform.DOMove(transform.position - Vector3.up * 5f, 1)
            .OnComplete(()=>GameMain.GameEntry.Entity.HideEntity(this.Entity));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = true;
            triggerTime = Time.time;
            //Log.Error("triggerd");
        }
            
    }
    
    
}
