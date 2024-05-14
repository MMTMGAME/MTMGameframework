using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameMain;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;


public class BattleUnitDieEventArgs : GameEventArgs
{
    /// <summary>
    /// 单位死亡
    /// </summary>
    public static readonly int EventId = typeof(BattleUnitDieEventArgs).GetHashCode();

    public BattleUnit battleUnit;
    

    /// <summary>
    /// 获取关闭界面完成事件编号。
    /// </summary>
    public override int Id
    {
        get
        {
            return EventId;
        }
    }
    
    
    public static BattleUnitDieEventArgs Create(BattleUnit unit)
    {
        BattleUnitDieEventArgs battleUnitDieArgs = ReferencePool.Acquire<BattleUnitDieEventArgs>();
        battleUnitDieArgs.battleUnit = unit;
        return battleUnitDieArgs;
    }


    public override void Clear()
    {
        battleUnit = null;
    }
}

public class BattleUnit : Entity
{
    [SerializeField]
    private BattleUnitData BattleUnitData;

    [SerializeField] protected List<Weapon> m_Weapons = new List<Weapon>();

    [SerializeField] protected List<Armor> m_Armors = new List<Armor>();

    private WeaponAnimEventListener weaponAnimEventListener;//监听武器攻击动画帧事件
    public BattleUnitData GetBattleUnitData()
    {
        return BattleUnitData;
    }

    public ChaState chaState;


    public bool IsValid()
    {
        return Visible && !chaState.dead;
    }
    
    public Weapon GetWeaponByIndex(int index)
    {
        if (m_Weapons.Count <= index)
        {
            return null;
        }
        return m_Weapons[index];
    }


    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        chaState = gameObject.GetOrAddComponent<ChaState>();
        chaState.onDead += OnDead;
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

        weaponAnimEventListener = gameObject.AddComponent<WeaponAnimEventListener>();
        weaponAnimEventListener.Init(this);

        chaState.InitBaseProp(new ChaProperty(10,BattleUnitData.baseHP,BattleUnitData.baseMP,BattleUnitData.baseAttack,BattleUnitData.baseDefense));
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
            var weapon = (Weapon)childEntity;
            m_Weapons.Add(weapon);
            chaState.equipmentProp[0] += new ChaProperty();
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

    protected virtual void OnDead()
    {
        GameEntry.Event.Fire(this,BattleUnitDieEventArgs.Create(this));
        
        

        CustomEvent.Trigger(gameObject, "OnDead"); // "OnDead"是在Visual Scripting中定义的事件名
        
        
        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), BattleUnitData.DeadEffectId)
        {
            Position = CachedTransform.localPosition,
        });
        GameEntry.Sound.PlaySound(BattleUnitData.DeadSoundId);
    }

    
    public CampType GetCamp()
    {
        return BattleUnitData.Camp;
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
    
    
    
}