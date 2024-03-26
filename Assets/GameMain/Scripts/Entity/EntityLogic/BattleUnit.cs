using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class BattleUnit : TargetableObject
{
    [SerializeField]
    private BattleUnitData BattleUnitData;

    [SerializeField] protected List<Weapon> m_Weapons = new List<Weapon>();

    [SerializeField] protected List<Armor> m_Armors = new List<Armor>();


    public BattleUnitData GetBattleUnitData()
    {
        return BattleUnitData;
    }

    

    public Weapon GetWeaponByIndex(int index)
    {
        return m_Weapons[index];
    }

    public void OnBeforeDamageVictim(TargetableObject victim, int damageHp)
    {
        //暂不实现，接入技能，Buff,天赋等系统
    }
    
    public void OnAfterDamageVictim(TargetableObject victim, int damageHp)
    {
        //暂不实现
    }

#if UNITY_2017_3_OR_NEWER
    protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
    {
        base.OnShow(userData);

        BattleUnitData = userData as BattleUnitData;
        if (BattleUnitData == null)
        {
            Log.Error("BattleUnit data is invalid.");
            return;
        }

        Name = Utility.Text.Format("BattleUnit ({0})", Id);


        List<WeaponData> weaponDatas = BattleUnitData.GetAllWeaponDatas();
        for (int i = 0; i < weaponDatas.Count; i++)
        {
            GameEntry.Entity.ShowWeapon(weaponDatas[i]);
        }

        List<ArmorData> armorDatas = BattleUnitData.GetAllArmorDatas();
        for (int i = 0; i < armorDatas.Count; i++)
        {
            GameEntry.Entity.ShowArmor(armorDatas[i]);
        }
    }

#if UNITY_2017_3_OR_NEWER
    protected override void OnHide(bool isShutdown, object userData)
#else
        protected internal override void OnHide(bool isShutdown, object userData)
#endif
    {
        base.OnHide(isShutdown, userData);
    }

#if UNITY_2017_3_OR_NEWER
    protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
#endif
    {
        base.OnAttached(childEntity, parentTransform, userData);


        if (childEntity is Weapon)
        {
            m_Weapons.Add((Weapon)childEntity);
            return;
        }

        if (childEntity is Armor)
        {
            m_Armors.Add((Armor)childEntity);
            return;
        }
    }

#if UNITY_2017_3_OR_NEWER
    protected override void OnDetached(EntityLogic childEntity, object userData)
#else
        protected internal override void OnDetached(EntityLogic childEntity, object userData)
#endif
    {
        base.OnDetached(childEntity, userData);


        if (childEntity is Weapon)
        {
            m_Weapons.Remove((Weapon)childEntity);
            return;
        }

        if (childEntity is Armor)
        {
            m_Armors.Remove((Armor)childEntity);
            return;
        }
    }

    protected override void OnDead(Entity attacker)
    {
        base.OnDead(attacker);

        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), BattleUnitData.DeadEffectId)
        {
            Position = CachedTransform.localPosition,
        });
        GameEntry.Sound.PlaySound(BattleUnitData.DeadSoundId);
    }


    public int GetWeaponAttack()
    {
        int attack = 0;
        foreach (var mWeapon in m_Weapons)
        {
            attack += mWeapon.m_WeaponData.Attack;
        }

        return attack;
    }

    public CampType GetCamp()
    {
        return BattleUnitData.Camp;
    }
    
    public override GameMain.BattleData GetImpactData()
    {
        
        return new GameMain.BattleData(BattleUnitData.Camp, BattleUnitData.HP, BattleUnitData.Defense);
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        
        
        //TryAttack();
        //不再用tryAttack了，用状态机进行攻击逻辑控制
    }

    // protected virtual void TryAttack()
    // {
    //     //举例攻击，因此用简单的写法
    //     for (int i = 0; i < m_Weapons.Count; i++)
    //     {
    //         m_Weapons[i].TryAttack();//因为是举例，武器直接大范围攻击
    //     }
    // }
    
    //动画事件
    public void Hit(int weaponIndex)
    {
        GetWeaponByIndex(weaponIndex).Hit();   
    }
}