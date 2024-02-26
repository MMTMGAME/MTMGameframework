using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class StartRoadPlaceholder : EntityPlaceholder
{
   
    public override void SpawnEntity()
    {
        GameEntry.Entity.ShowStartRoad(new StartRoadData(GameEntry.Entity.GenerateSerialId(),typeId)
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }
}
