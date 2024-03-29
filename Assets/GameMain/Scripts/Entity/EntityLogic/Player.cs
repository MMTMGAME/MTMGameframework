using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

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
        
        //攻击
        if (Input.GetMouseButtonDown(0))
        {
            var weapon = GetWeaponByIndex(0);
            if (weapon != null)
            {
                weapon.HandleAnimEvent("Shoot",1f);
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            var weapon = GetWeaponByIndex(1);
            if (weapon != null)
            {
                weapon.HandleAnimEvent("Shoot",1f);
            }
        }
    }

    public override void ApplyDamage(BattleUnit attacker, int damageHP)
    {
        base.ApplyDamage(attacker, damageHP);
        GameEntry.CameraShake.ShakeCamera(0.3f,0.5f,0.3f);

        GameEntry.Sound.PlaySound(20000,transform.position);
    }
}
