using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class Debug3DTextData : EntityData
{

    public string Msg { get; private set; }
    public float Duration { get; private set; }
    public Debug3DTextData(int entityId, int typeId,string msg,float duration=5) : base(entityId, typeId)
    {
        this.Msg = msg;
        this.Duration = duration;
    }
}
