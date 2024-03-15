using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class EffectCloudData : EntityData
{

    public Transform followTarget { get; private set; }

    public EffectCloudData(int entityId, int typeId,Transform followTarget) : base(entityId, typeId)
    {
        this.followTarget = followTarget;
    }
}
