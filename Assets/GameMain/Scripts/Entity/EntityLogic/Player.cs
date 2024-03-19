using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameMain;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class Player : BattleUnit
{
    private PlayerData playerData;
    public PlayerMove  PlayerMove { get; private set; }

    public Rigidbody rigid;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        playerData=userData as PlayerData;

        PlayerMove = GetComponent<PlayerMove>();
        
        if (playerData == null)
        {
            Log.Error("PlayerData is Invalid");
            return;
        }
        Name = Utility.Text.Format("Player ({0})", Id);

        rigid = GetComponent<Rigidbody>();
        GetComponent<RigBuilder>().Build();
        
        //测试跟随抖动
        //GameEntry.EntityUi.ShowEntityUi (new ShowHpBarItemInfo(GameEntry.EntityUi.GenerateSerialId(), this, 1, 1));

    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

    }

    public bool Invincible { get; set; }

    public override void ApplyDamage(Entity attacker, int damageHP)
    {
        if(Invincible)
            return;
        base.ApplyDamage(attacker, damageHP);
        CachedAnimator.SetTrigger("Damaged");
        GameEntry.Sound.PlaySound(UnityEngine.Random.Range(10040, 10042));
    }

    protected override void OnDead(Entity attacker)
    {
        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), BattleUnitData.DeadEffectId)
        {
            Position = CachedTransform.localPosition,
        });
        GameEntry.Sound.PlaySound(BattleUnitData.DeadSoundId);
        
        GameEntry.Entity.transform.gameObject.SetActive(true);
        PlayerMove.enabled = false;
        CachedAnimator.SetTrigger("Die");
        GetComponent<RigBuilder>().enabled = false;
    }
}
