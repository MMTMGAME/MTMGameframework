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
        
        //ownerBattleUnit?.CachedAnimator.SetTrigger("Attack");
    }
}