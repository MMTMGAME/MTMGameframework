using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class MeleeWeaponAttack : WeaponAttack
{

    private Animator animator;


    

   
    public override void HandleAnimeEvent(string eventName)
    {
        if (eventName == "Hit")
        {
            Hit();
        }
    }

    public void Hit()
    {

        if (OwnerBattleUnit == null)
        {
            Debug.LogWarning("OwnerBattleUnit为空了，不对劲");
            return;
        }
           
        var data = OwnerBattleUnit.GetBattleUnitData();
        var aiData = data.AIData;
            
        var entities = AIUtility.FindBattleUnits((BattleUnit)Weapon.OwnerEntity, RelationType.Hostile | RelationType.Neutral,
            transform.position, aiData.AttackDistance);
        foreach (var entity in entities)
        {
            Attack(entity);
        }
    }
}