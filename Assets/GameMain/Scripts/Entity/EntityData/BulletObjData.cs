using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class BulletObjData : EntityData
{
    public BulletLauncher bulletLauncher;

    public BulletObjData(int entityId, int typeId,BulletLauncher bulletLauncher) : base(entityId, typeId)
    {
        this.bulletLauncher = this.bulletLauncher;
    }
}
