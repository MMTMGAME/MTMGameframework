using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class MeleeWeaponAttack : WeaponAttack
{

    private Animator animator;


    private BattleUnit ownerBattleUnit;
    

    public override void Attack()
    {
        var ownerId = Weapon.m_WeaponData.OwnerId;
        var ownerEntity = GameEntry.Entity.GetEntity(ownerId);
        
        if (ownerEntity == null)
        {
           return;
        }

        ownerBattleUnit = ownerEntity.Logic as BattleUnit;
        
        if(ownerBattleUnit==null)
            return;
        
        ownerBattleUnit?.CachedAnimator.SetTrigger("Attack");
    }


    private float lastAttackedTime;
    private void OnTriggerEnter(Collider other)
    {
        var victim = other.GetComponentInParent<BattleUnit>();
        if (victim!=null)
        {
            if(ownerBattleUnit==null)
                return;
            if (Time.time>lastAttackedTime+2 &&  ownerBattleUnit.CachedAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))//播放攻击动画时才会碰撞
            {
                GameEntry.Sound.PlaySound(20000);
                AIUtility.Attack(ownerBattleUnit,victim);
            }
           
        }
    }
}
