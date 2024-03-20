using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class FollowPlayer : MonoBehaviour
{
    private Player Player { get; set;}

    private Vector3 offset;
    private float moveTimer;
    // Start is called before the first frame update
    void Awake()
    {
        
        GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        
    }

    protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.EntityLogicType == typeof(Player))
        {
            Player = ne.Entity.Logic as Player;

            offset = Player.transform.position - transform.position;
        }
    }
    

    void Update()
    {

        moveTimer += Time.deltaTime;
        if (moveTimer > 600)//每10分钟同步一次位置，避免玩家跑太远没有云了
        {
            if(Player!=null)
                UpdatePos();
            moveTimer = 0;
        }
        
    }

    void UpdatePos()
    {

        transform.DOMove(Player.transform.position - offset, 30);

    }

    private void OnDisable()
    {
        try
        {
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        }
        catch (Exception e)
        {
            //DoNothing
            
        }
        
    }
}
