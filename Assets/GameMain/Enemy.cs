using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;


public class Enemy : BattleUnit
{
    private EnemyData enemyData;

    private Player player;

    private float attackDistance=2.4f;
    private Animator animator;
    private static readonly int Attack = Animator.StringToHash("Attack");

    private float zDistance = 5;
    private bool attackAble = false;//开局演出期间不攻击

    private float farFollowDistance = 10;//不让相机看见
    private float normalFollowDistance = 5;
    private float attackFollowDistance = 0;//攻击距离

    private float lerpValue = 4f;

    private bool isBarrageMode;
    
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        enemyData = (EnemyData)userData;

        GetPlayer();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameEntry.Event.Subscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
        GameEntry.Event.Subscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
    }
    
    void OnPlayerEnterBarrageRoad(object sender, GameEventArgs args)
    {
        isBarrageMode = true;
    }

    void OnPlayerExitBarrageRoad(object sender, GameEventArgs args)
    {
        isBarrageMode = false;
    }

    private void OnDisable()
    {
        GameEntry.Event.Unsubscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
                GameEntry.Event.Unsubscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
    }

    void GetPlayer()
    {
        var procedure = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
        if (procedure != null)
        {
            player = procedure.GetGameBase().Player;
        }

        StartCoroutine(StartDistanceCo());
    }

    //开局演出
    IEnumerator StartDistanceCo()
    {
        attackAble = false;
        zDistance = farFollowDistance;
        yield return new WaitForSeconds(2);
        zDistance =attackFollowDistance;
        yield return new WaitForSeconds(5);
        zDistance = normalFollowDistance;
        yield return new WaitForSeconds(1);
        attackAble = true;
    }
    
    
    
    
    

    void LateUpdate()
    {
        if (player == null || player.Available == false)
        {
            GetPlayer();
            return;
        }

        if (attackAble)//表示演出结束
        {
            if (Time.time < player.PlayerMove.lastStumbleTime + 3.5f && Time.time > player.PlayerMove.lastStumbleTime + 0.3f)
            {
                zDistance = attackFollowDistance;
            }
            else
            {
                zDistance = normalFollowDistance;
            }
        }
           
        
        transform.position = Vector3.Lerp(transform.position,player.transform.TransformPoint(0,0,-zDistance),4f*Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, player.rigid.rotation, 10 * Time.deltaTime);

        var distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < attackDistance && player.IsDead==false && attackAble)
        {
            m_Weapons[0].TryAttack();//因为是举例，武器直接大范围攻击
        }
        
        //远程武器逻辑，远程武器只要在弹幕路段就可以直接攻击
        if (isBarrageMode)
        {
            m_Weapons[1].TryAttack();
        }
        else
        {
            m_Weapons[1].StopAttack();
            
        }
    }

    protected override void TryAttack()
    {
        //base.TryAttack();
        //这个游戏中不使用onUpdate的代码进行控制,覆盖父类代码，不要执行父类逻辑
    }
    
    
}
