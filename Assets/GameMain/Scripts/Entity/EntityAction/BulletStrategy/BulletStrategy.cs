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
        Entity entity = collision.gameObject.GetComponentInParent<Entity>();
        if (entity == null)
        {
            return;
        }

        if (entity is TargetableObject targetAbleObject)
        {
            if(entity.Id==bulletData.OwnerId)
                return;
            var owner = GameEntry.Entity.GetEntity(bulletData.OwnerId);
            AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, targetAbleObject);
        }
    }

    public virtual void PerformTrigger(Collider other)
    {
        Entity entity = other.gameObject.GetComponentInParent<Entity>();
        if (entity == null)
        {
            return;
        }
        
        if (entity is TargetableObject targetAbleObject)
        {
            if(entity.Id==bulletData.OwnerId)
                return;
            var owner = GameEntry.Entity.GetEntity(bulletData.OwnerId);
                
            AIUtility.BulletAttack((BattleUnit)owner.Logic,bullet, targetAbleObject);
        }
    }
}
