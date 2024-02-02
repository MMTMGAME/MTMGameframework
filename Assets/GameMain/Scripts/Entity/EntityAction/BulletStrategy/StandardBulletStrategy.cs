using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class StandardBulletStrategy : BulletStrategy
{
    public override void Init(Bullet bullet)
    {
        base.Init(bullet);
        rigidBody.velocity = transform.forward * bulletData.Speed;
    }
}
