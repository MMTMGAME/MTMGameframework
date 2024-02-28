using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class EnemyPlaceholder : EntityPlaceholder
{
    
    public override void SpawnEntity()
    {
        GameEntry.Entity.ShowEnemy(new EnemyData(GameEntry.Entity.GenerateSerialId(),typeId,CampType.Enemy)
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }
}
