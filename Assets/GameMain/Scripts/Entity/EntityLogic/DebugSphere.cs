using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class DebugSphere : Entity
{

    public DebugSphereData debugSphereData;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        debugSphereData = (DebugSphereData)userData;
        Invoke(nameof(HideSelf),debugSphereData.Duration);
    }

    void HideSelf()
    {
        GameEntry.Entity.HideEntity(this);
    }
}
