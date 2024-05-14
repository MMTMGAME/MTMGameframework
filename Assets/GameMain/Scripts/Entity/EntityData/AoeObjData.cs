using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class AoeObjData : EntityData
{
    public AoeLauncher aoeLauncher;

    public AoeObjData(int entityId, int typeId,AoeLauncher aoeLauncher) : base(entityId, typeId)
    {
        this.aoeLauncher = this.aoeLauncher;
    }
}
