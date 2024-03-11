using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameMain;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class Road : Entity
{
    public RoadData roadData;

   
    [HideInInspector]public bool triggered;
    [HideInInspector]public float triggerTime;

    [HideInInspector]public bool exited;
    [HideInInspector] public float exitedTime;
    
    private Rigidbody rigidbody;
    
    [HideInInspector]
    public RoadConfig roadConfig;

    //刚生成时的位置，缓动前的位置，记录这个位置，在下一个Road生成时使用这两个位置，而不是实时位置，否则位置不正确
    [HideInInspector]public Vector3 spawnPos;
    [HideInInspector]public Vector3[] spawnTailPos;
    [FormerlySerializedAs("spawnTailPosRotations")] [HideInInspector]public Quaternion[] spawnTailRotations;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        rigidbody = GetComponent<Rigidbody>();
        roadConfig = GetComponent<RoadConfig>();
        
        
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        roadData = (RoadData)userData;

        //对象池复用
        triggered = false;
        triggerTime = 0;

        exited = false;
        exitedTime = 0;
        
        //增加宽度
        //transform.localScale = new Vector3(1.5f, 1, 1);

        spawnPos = transform.position;
        spawnTailPos = new Vector3[roadConfig.tails.Length];
        spawnTailRotations = new Quaternion[roadConfig.tails.Length];
        for (var i = 0; i < roadConfig.tails.Length; i++)
        {
            var tailTrans = roadConfig.tails[i];
            spawnTailPos[i] = tailTrans.position;
            spawnTailRotations[i] = tailTrans.rotation;
        }

        //缓动 
        CachedTransform.position -= Vector3.up * 5f;
        CachedTransform.DOLocalMove(spawnPos, 0.3f).SetEase(Ease.InQuad).OnComplete(SpawnPrimogem);
        
        
        
        PillarCheck();
    }

    void PillarCheck()
    {
        var colliders = Physics.OverlapBox(transform.TransformPoint(0, 0, 5), new Vector3(2, 5, 5), transform.rotation);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Pillar"))
            {
                var pillarComp = collider.GetComponent<Pillar>();
                if (pillarComp)
                {
                    var dir = transform.forward;
                    dir.y = 0;
                    pillarComp.Collapse(dir*20);
                }
            }
        }
    }

    
    void SpawnPrimogem()
    {
        //生成原石
        foreach (var primogem in roadConfig.primogems)
        {
            var id = GameEntry.Entity.GenerateSerialId();
            
            GameEntry.Entity.ShowPrimogem(new PrimogemData(id,10002,Id)
            {
                Position = primogem.position,
                Rotation = primogem.rotation,
            });
        }


        foreach (var t in roadConfig.obstacles)
        {
            GameEntry.Entity.ShowObstacle(new ObstacleData(GameEntry.Entity.GenerateSerialId(),UnityEngine.Random.Range(85000,85002),Id)
            {
                Position = t.position,
                Rotation = t.rotation,
            });
        }
        
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

            exited = false;
            
        }
            
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            exited = true;
            exitedTime = Time.time;
            //Log.Error("triggerd");
        }
            
    }
    
}
