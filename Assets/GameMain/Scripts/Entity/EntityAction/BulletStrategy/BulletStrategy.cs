using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public abstract class BulletStrategy : MonoBehaviour
{
    protected Bullet bullet;
    protected BulletData bulletData;

    protected Rigidbody rigidBody;
    protected Collider collider;
    public virtual void Init(Bullet bullet)
    {
        this.bullet = bullet;
        bulletData = bullet.m_BulletData;
        rigidBody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        rigidBody.useGravity = bulletData.useGravity;
        collider.isTrigger = bulletData.isTrigger;
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
            AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, victim);
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
            
        AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, victim);
        
    }
}
