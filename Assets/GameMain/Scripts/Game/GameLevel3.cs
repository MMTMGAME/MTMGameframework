using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameLevel3 : GameLevel2
{
    public Basin basin;


    protected override void CheckGameOverOrWin()
    {
        base.CheckGameOverOrWin();
        if (basin != null && basin.IsDead && Player.IsDead == false)
        {
            GameWin = true;
        }
    }

    protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        base.OnShowEntitySuccess(sender,e);
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.EntityLogicType == typeof(Basin))
        {

            basin = ne.Entity.Logic as Basin;

        }
    }
}
