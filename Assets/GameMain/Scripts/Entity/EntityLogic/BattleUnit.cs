using System;
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

    

    private AnimEventListener m_AnimEventListener;//监听武器攻击动画帧事件
    public BattleUnitData GetBattleUnitData()
    {
        return BattleUnitData;
    }

    public ChaState chaState;

    //记录id，后面通过键位调用对应index的skill进行调用,在移除武器时也需要通过这个进行Skill的移除
    //注意，无论是角色节能还是武器技能LearnSkill时都会存储到skill列表，而武器技能还会存储到weaponSkillIds，使用CastWeaponSkill
    //可以单独调用武器技能
    public List<string> weaponSkillIds = new List<string>();
    
   
    public void CastWeaponSkill(int index)
    {
        chaState.CastSkill(weaponSkillIds[index]);//武器的技能和角色自带技能区分开，
    }
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

    protected List<Collider> battleUnitColliders=new List<Collider>();//用于防止自身和通过Attachto到自己的子物体防碰撞
    protected Rigidbody rb;
    private static readonly int Die = Animator.StringToHash("Die");

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        chaState = gameObject.GetOrAddComponent<ChaState>();
        chaState.onDead += OnDead;
        var cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            if (!col.isTrigger)
            {
                battleUnitColliders.Add(col);
            }
        }
        
        rb = GetComponent<Rigidbody>();
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

        m_AnimEventListener = gameObject.GetOrAddComponent<AnimEventListener>();
        m_AnimEventListener.Init(this);

        chaState.InitBaseProp(new ChaProperty(BattleUnitData.baseMoveSpeed,BattleUnitData.baseHP,BattleUnitData.baseMP,BattleUnitData.baseAttack,BattleUnitData.baseDefense,BattleUnitData.baseActionSpeed));
        chaState.Camp = BattleUnitData.Camp;

        foreach (var buff in BattleUnitData.buffs)
        {
            //举例:"ExplosionOnBeKilled_ExplosionDamage:1|HitAlly:True|HitFoe:False,HealByInterval_Interval:1|HealPercent|3"
            var arr = buff.Split("_");
            if(arr.Length==0)
                return;
            var buffId = arr[0];

            Dictionary<string, object> buffParam = new Dictionary<string, object>();

            if (arr.Length > 1)
            {
                var buffParamStr = arr[1];
                var buffParamStrArr = buffParamStr.Split("|");
                foreach (var bp in buffParamStrArr)
                {
                    var bpArr = bp.Split(":");

                    if (bpArr.Length == 2)
                    {
                        buffParam.Add(bpArr[0],bpArr[1]);
                    }
                }
            }
           
            
            chaState.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],gameObject,
                gameObject,1,10,true,true,buffParam));
        }
        
        foreach (var skillId in BattleUnitData.skills)
        {
            chaState.LearnSkill(DesingerTables.Skill.data[skillId]);
        }

        chaState.tags = BattleUnitData.tags;
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
            
                foreach (var skillId in weapon.m_WeaponData.skills)
                {
                    if(skillId=="")
                        continue;
                    chaState.LearnSkill(DesingerTables.Skill.data[skillId]);
                }
                weaponSkillIds.AddRange(weapon.m_WeaponData.skills);
            
            
            //buff
            foreach (var buffId in weapon.m_WeaponData.buffs)
            {
                if(buffId=="")
                    continue;
                chaState.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],gameObject,
                    gameObject,1,10,true,true));
            }
            
            chaState.equipmentProp[0] += new ChaProperty();
            return;
        }

        if (childEntity is Armor)
        {
            var armor = (Armor)childEntity;
            m_Armors.Add(armor);
            
            //buff
            foreach (var buffId in armor.m_ArmorData.buffs)
            {
                if(buffId=="")
                    continue;
                chaState.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],gameObject,
                    gameObject,1,10,true,true));
            }
            
            return;
        }
        
        StartCoroutine(HandleOnAttachedTransform(childEntity, parentTransform));
    }

    IEnumerator HandleOnAttachedTransform(EntityLogic childEntity, Transform parentTransform)
    {
        yield  return null;
        
        //特殊情况下要防止碰撞，要用的时候再解开注释
        // var colliders = childEntity.GetComponentsInChildren<Collider>();
        // foreach (var col in colliders)
        // {
        //     foreach (var battleUnitCollider in battleUnitColliders)
        //     {
        //         Physics.IgnoreCollision(battleUnitCollider,col);
        //     }
        //     
        // }
        
        
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
            var weapon = (Weapon)childEntity;
            m_Weapons.Remove((Weapon)childEntity);

            foreach (var skillId in weapon.m_WeaponData.skills)
            {
                if(skillId=="")
                    continue;
                chaState.RemoveSkill(skillId);
            }

            foreach (var buffId in weapon.m_WeaponData.buffs)
            {
                if(buffId=="")
                    continue;
                chaState.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],gameObject,
                    gameObject,1,0,true,true));
            }
            
            return;
        }

        if (childEntity is Armor)
        {
            var armor = (Armor)childEntity;
            m_Armors.Remove(armor);
            foreach (var buffId in armor.m_ArmorData.buffs)
            {
                if(buffId=="")
                    continue;
                chaState.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],gameObject,
                    gameObject,1,0,true,true));
            }
            return;
        }
    }

    protected virtual void OnDead()
    {
        GameEntry.Event.Fire(this,BattleUnitDieEventArgs.Create(this));
        
        
        CachedAnimator?.SetTrigger(Die);

        CustomEvent.Trigger(gameObject, "OnDead"); // "OnDead"是在Visual Scripting中定义的事件名
        
        
        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), BattleUnitData.DeadEffectId)
        {
            Position = CachedTransform.localPosition,
        });
        if (BattleUnitData.DeadSoundId > 0)
        {
            GameEntry.Sound.PlaySound(BattleUnitData.DeadSoundId);
        }
        

        GameEntry.Timer.AddOnceTimer(BattleUnitData.hideTime, () =>
        {
            if(Visible)
                GameEntry.Entity.HideEntity(this);
        });

       
    }

    
}