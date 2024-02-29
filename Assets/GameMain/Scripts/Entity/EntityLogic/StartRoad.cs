using System.Collections;
using System.Collections.Generic;
using System.Timers;
using GameMain;
using UnityEngine;
using Entity = GameMain.Entity;
public class StartRoad : Entity
{

    private StartRoadData startRoadData;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        startRoadData = (StartRoadData)userData;
        
        Invoke(nameof(HideSelf),5);
    }

    void HideSelf()
    {
        GameEntry.Entity.HideEntity(this);
    }
}
