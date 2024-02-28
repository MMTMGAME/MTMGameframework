using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class Player : BattleUnit
{
    private PlayerData playerData;

    public Rigidbody rigid;

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

        rigid = GetComponent<Rigidbody>();
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

    }

    public override void ApplyDamage(Entity attacker, int damageHP)
    {
        base.ApplyDamage(attacker, damageHP);
        CachedAnimator.SetTrigger("Damaged");
    }

    protected override void OnDead(Entity attacker)
    {
        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), BattleUnitData.DeadEffectId)
        {
            Position = CachedTransform.localPosition,
        });
        GameEntry.Sound.PlaySound(BattleUnitData.DeadSoundId);
        
        GameEntry.Entity.transform.gameObject.SetActive(true);
        GetComponent<PlayerMove>().enabled = false;
        CachedAnimator.SetTrigger("Die");
    }
}
