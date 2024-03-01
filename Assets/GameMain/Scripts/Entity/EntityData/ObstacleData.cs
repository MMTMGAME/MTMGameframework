using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class ObstacleData : EntityData
{
    public int ownerId;
    public ObstacleData(int entityId, int typeId,int ownerId) : base(entityId, typeId)
    {
        this.ownerId = ownerId;
    }
}
