using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;

public class Player : BattleUnit
{
    private PlayerData playerData;


    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        playerData=userData as PlayerData;

        if (playerData == null)
        {
            Log.Error("PlayerData is Invalid");
            return;
        }
        Name = Utility.Text.Format("Player ({0})", Id);
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        

        var horizontal = Input.GetAxis("Horizontal");
        var vertical= Input.GetAxis("Vertical");
        
        transform.Rotate(Vector3.up,30*Time.deltaTime*horizontal *(vertical>=0?1:-1) );
        transform.Translate(Vector3.forward * (2 * (Time.deltaTime * vertical)),Space.Self);
    }
}
