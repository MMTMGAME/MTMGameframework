using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class DebugSphereData : EntityData
{
    public float Duration { get; private set; }

    public DebugSphereData(int entityId, int typeId,float duration=5) : base(entityId, typeId)
    {
        this.Duration = duration;
    }
}
