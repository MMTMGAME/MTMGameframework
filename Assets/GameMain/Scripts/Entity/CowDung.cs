using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using Random = System.Random;

public class CowDung : SceneItem
{
    protected override void OnDead(Entity attacker)
    {
        
        var count = 50;
        for (int i = 0; i < count; i++)
        {
            var randomRotation = UnityEngine.Random.rotation;
            GameEntry.Entity.ShowPhysicsBullet(new PhysicsBulletData(GameEntry.Entity.GenerateSerialId(),60000,Id,CampType.Neutral,6,UnityEngine.Random.Range(300,500) ,5f)
            {
                Position = transform.position,
                Rotation = randomRotation
            });
        }
        
        base.OnDead(attacker);
        
        
    }
}
