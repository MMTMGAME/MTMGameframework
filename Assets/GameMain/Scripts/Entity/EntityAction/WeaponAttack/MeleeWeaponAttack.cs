using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class MeleeWeaponAttack : WeaponAttack
{

    private Animator animator;


    private BattleUnit ownerBattleUnit;

   
    public override void HandleAnimeEvent(string eventName, float radius)
    {
        if (eventName == "Hit")
        {
            Hit(radius);
        }
    }

    public void Hit(float radius)
    {
        if (radius == 0)
        {
            Log.Error($"武器{transform.name}的攻击范围不应该是0,请设置帧事件的float属性");
            radius = 1;
        }
            
        var entities = AIUtility.FindBattleUnits((BattleUnit)Weapon.OwnerEntity, RelationType.Hostile | RelationType.Neutral,
            transform.position, radius);
        foreach (var entity in entities)
        {
            Attack(entity);
        }
    }
}