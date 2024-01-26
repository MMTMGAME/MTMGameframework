using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class Basin : SceneItem
{
    protected override void OnDead(Entity attacker)
    {
       //base.OnDead();
       //DoNothing
       Debug.LogError("我说了DONothing了");
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!IsDead)
            return;
        if(RigidbodyComp.velocity.magnitude<0.5f)
            return;
        Entity entity = collision.gameObject.GetComponentInParent<Entity>();
        if (entity == null)
        {
            return;
        }

        if (entity is TargetableObject)
        {
            (entity as TargetableObject).ApplyDamage(this, sceneItemData.attack);
                
        }
    }
}
