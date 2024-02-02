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
        //举例攻击，因此用简单的写法
        for (int i = 0; i < m_Weapons.Count; i++)
        {
            m_Weapons[i].TryAttack();//因为是举例，武器直接大范围攻击
        }

        var horizontal = Input.GetAxis("Horizontal");
        var vertical= Input.GetAxis("Vertical");
        
        transform.Rotate(Vector3.up,30*Time.deltaTime*horizontal *(vertical>=0?1:-1) );
        transform.Translate(Vector3.forward * (2 * (Time.deltaTime * vertical)),Space.Self);
    }
}
