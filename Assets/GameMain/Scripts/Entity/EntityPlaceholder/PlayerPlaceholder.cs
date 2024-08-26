using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class PlayerPlaceholder : EntityPlaceholder
{
    public override void SpawnEntity()
    {
        GameEntry.Entity.ShowPlayer(new PlayerData(GameEntry.Entity.GenerateSerialId(),typeId)
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }
}
