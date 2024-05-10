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

    private BattleUnit m_ownerBattleUnit;
    public BattleUnit OwnerBattleUnit
    {
        get
        {
            if (m_ownerBattleUnit == null)
            {
                m_ownerBattleUnit=Weapon.OwnerEntity as BattleUnit;
            }

            return m_ownerBattleUnit;
        }
    }
    
    protected float lastAttackTime;
    private static readonly int Fire = Animator.StringToHash("Fire");
    
    public virtual void Attack(TargetableObject victim)
    {
        AIUtility.Attack((BattleUnit)Weapon.OwnerEntity,Weapon,victim);
    }
    
    public virtual void StartFire()
    {
        TryAttack();
    }

    public virtual void CancelFire()
    {
       
            
    }
    
    public virtual void TryAttack()
    {
        if (Time.time > lastAttackTime + Weapon.m_WeaponData.AttackInterval)
        {
            Weapon.OwnerEntity.CachedAnimator.SetTrigger(Fire);
            lastAttackTime = Time.time;
        }
       
    }

    public abstract void HandleAnimeEvent(string eventName);
}
