using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public abstract class WeaponAttack : MonoBehaviour
{
    protected Weapon Weapon
    {
        get;
        set;
    }
    public virtual void Init(Weapon weapon)
    {
        Weapon = weapon;
    }
    
    public virtual void Attack(TargetableObject victim)
    {
        AIUtility.Attack((BattleUnit)Weapon.OwnerEntity,Weapon,victim);
    }
    

    public abstract void HandleAnimeEvent(string eventName, float radius);
}
