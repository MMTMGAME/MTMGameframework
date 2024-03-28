using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public abstract class BulletStrategy : MonoBehaviour
{
    protected Bullet bullet;
    protected BulletData bulletData;

    protected Rigidbody rb;
    protected Collider col;

    private bool hited;//有时候碰撞体可能一帧触发多次碰撞，可能导致多次伤害，所以要判断是否出法国
    public virtual void Init(Bullet bullet)
    {
        this.bullet = bullet;
        bulletData = bullet.m_BulletData;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = bulletData.useGravity;
        col.isTrigger = bulletData.isTrigger;

        hited = false;
    }


    public virtual void Update()
    {
        
    }
   
    
    public virtual void PerformCollision(Collision collision)
    {
       
        TargetableObject victim = collision.gameObject.GetComponentInParent<TargetableObject>();
        if (victim == null)
        {
            return;
        }

        
        if(victim.Id==bulletData.OwnerId)
            return;
        var owner = GameEntry.Entity.GetEntity(bulletData.OwnerId);
        if (owner != null)
        {
            
            if(hited)
                return;
            AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, victim);
            hited = true;

        }
        
        
    }

    

    public virtual void PerformTrigger(Collider other)
    {
        TargetableObject victim = other.gameObject.GetComponentInParent<TargetableObject>();
        if (victim == null)
        {
            return;
        }
        
        
        if(victim.Id==bulletData.OwnerId)
            return;
        var owner = GameEntry.Entity.GetEntity(bulletData.OwnerId);
        if (owner != null)
        {
            
            if(hited)
                return;
            AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, victim);

            bullet.PlaySoundAndFxByPhysMat(other);
            hited = true;
        }
            
    }
}
