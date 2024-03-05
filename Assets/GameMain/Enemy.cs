using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;


public class Enemy : BattleUnit
{
    private EnemyData enemyData;

    private Player player;

    private float attackDistance=2.2f;
    private Animator animator;
    private static readonly int Attack = Animator.StringToHash("Attack");

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        enemyData = (EnemyData)userData;

        GetPlayer();
        animator = GetComponent<Animator>();
    }

    void GetPlayer()
    {
        var procedure = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
        if (procedure != null)
        {
            player = procedure.GetGameBase().Player;
        }
    }

    void LateUpdate()
    {
        if (player == null || player.Available == false)
        {
            GetPlayer();
            return;
        }
           
        
        transform.position = Vector3.Lerp(transform.position,player.rigid.position,3f*Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, player.rigid.rotation, 10 * Time.deltaTime);

        var distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < attackDistance && player.IsDead==false)
        {
            //举例攻击，因此用简单的写法
            for (int i = 0; i < m_Weapons.Count; i++)
            {
                m_Weapons[i].TryAttack();//因为是举例，武器直接大范围攻击
            }
        }
    }

    protected override void TryAttack()
    {
        //base.TryAttack();
        //这个游戏中不使用onUpdate的代码进行控制
    }
}
