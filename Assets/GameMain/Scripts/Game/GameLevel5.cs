using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class GameLevel5 : GameBase
{

    public List<Cabbage> cabbages=new List<Cabbage>();

    public OldMan oldMan;

    private OldManPath oldManPath;
    private  bool hasCabbageDestroyed = false;
    public override void Initialize()
    {
        base.Initialize();
        GameEntry.Entity.ShowOldMan( new OldManData(GameEntry.Entity.GenerateSerialId(),20000){});
        oldManPath = Object.FindObjectOfType<OldManPath>();

        hasCabbageDestroyed = false;
        cabbages.Clear();
    }

    protected override void CheckGameOverOrWin()
    {
        base.CheckGameOverOrWin();
        if(oldMan==null)//动态加载可能需要一点时间
            return;
        //被老头看见
        if (oldMan.AngryEnd)
        {
            GameOver = true;
            return;
        }

       
        foreach (var cabbage in cabbages)
        {
            if (cabbage.IsDead)
                hasCabbageDestroyed = true;
        }

        if (hasCabbageDestroyed && !Player.IsDead)
        {
            GameWin = true;
        }
        
    }

    protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        base.OnShowEntitySuccess(sender, e);
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.EntityLogicType == typeof(Cabbage))
        {
            cabbages.Add(ne.Entity.Logic as Cabbage);
        }

        if (ne.EntityLogicType == typeof(OldMan))
        {
            oldMan=ne.Entity.Logic as OldMan;
            if (oldMan != null)
            {
                oldMan.SetCartTrans(oldManPath.cartTrans);
            }
        }
    }
}
