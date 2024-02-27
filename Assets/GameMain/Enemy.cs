using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;


public class Enemy : Entity
{
    private EnemyData enemyData;

    private Player player;


    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        enemyData = (EnemyData)userData;

        GetPlayer();
    }

    void GetPlayer()
    {
        var procedure = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
        if (procedure != null)
        {
            player = procedure.GetGameBase().Player;
        }
    }

    void Update()
    {
        if (player == null || player.Available == false)
        {
            GetPlayer();
            return;
        }
           
        
        transform.position = player.transform.TransformPoint(0, 0, -2);
    }
}
