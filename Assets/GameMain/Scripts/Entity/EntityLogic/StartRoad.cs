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

        StartCoroutine(HideSelf());
    }

    IEnumerator HideSelf()
    {
        yield return new WaitForSeconds(5);
        GameEntry.Entity.HideEntity(this);
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
        StopAllCoroutines();
    }
}
