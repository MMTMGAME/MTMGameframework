using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameLevel6 : GameBase
{
    
    private bool isCowDungLoaded = false;

    public override void Initialize()
    {
        base.Initialize();
        cowDungDiedTimer = 0;
        isCowDungLoaded = false;
        CowDung = null;
    }
    

    private CowDung CowDung;
    private float cowDungDiedTimer;
    protected override void CheckGameOverOrWin()
    {
        if (!isCowDungLoaded)
        {
            //Debug.Log("CowDung not loaded yet");
            return;
        }

        if (CowDung == null)//被销毁
        {
            Debug.Log("cowDungDiedTimer"+cowDungDiedTimer);
            cowDungDiedTimer += Time.deltaTime;
            if (cowDungDiedTimer > 2)
            {
                if (Player.IsDead)
                {
                    GameOver = true;
                }
                else
                {
                    GameWin = true;
                }
            }
        }
    }

    protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
    {
        base.OnShowEntitySuccess(sender, e);
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.EntityLogicType == typeof(CowDung))
        {

            CowDung = ne.Entity.Logic as CowDung;
            isCowDungLoaded = true;
        }
    }
}
