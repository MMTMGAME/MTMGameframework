//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Buffers;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// 武器类。
    /// </summary>
    public class Weapon : Entity
    {

        private Entity ownerEntity;
        public Entity OwnerEntity
        {
            get
            {
                if (ownerEntity == null)
                {
                    ownerEntity=GameEntry.Entity.GetEntity(m_WeaponData.OwnerId).Logic as Entity;
                }

                return ownerEntity;
            }
            
        }

        [SerializeField]
        public WeaponData m_WeaponData = null;

        private float m_NextAttackTime = 0f;
        
        private WeaponAttack WeaponAttack { get; set; }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_WeaponData = userData as WeaponData;
            if (m_WeaponData == null)
            {
                Log.Error("Weapon data is invalid.");
                return;
            }

            var ownerEntity = GameEntry.Entity.GetEntity(m_WeaponData.OwnerId);
            if (ownerEntity == null)
            {
                Log.Fatal("武器组件附加失败，因为OwnerEntity不存在");
                return;
            }


            var battleUnit = ownerEntity.Logic as BattleUnit;
            GameEntry.Entity.AttachEntity(Entity, m_WeaponData.OwnerId, battleUnit.GetBattleUnitData().GetWeaponPath(m_WeaponData.SlotIndex));
            
            AddAttackLogicComponent(m_WeaponData.AttackLogicComponent);
        }
        
        private void AddAttackLogicComponent(string className)
        {
            Type attackLogicType = Type.GetType(className);
            if (attackLogicType != null && attackLogicType.IsSubclassOf(typeof(WeaponAttack)))
            {
                WeaponAttack = (WeaponAttack)gameObject.AddComponent(attackLogicType);
                // 如果需要，这里可以对 attackComponent 进行进一步的配置
                WeaponAttack.Init(this);
            }
            else
            {
                Debug.LogError($"Attack logic class '{className}' not found or does not extend WeaponAttack.");
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#endif
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);

            Name = Utility.Text.Format("Weapon of {0}", parentEntity.Name);
            CachedTransform.localPosition = Vector3.zero;
            CachedTransform.localScale = Vector3.one;
            CachedTransform.localRotation=Quaternion.identity;
        }

        // public void TryAttack()
        // {
        //     if (Time.time < m_NextAttackTime)
        //     {
        //         return;
        //     }
        //
        //     m_NextAttackTime = Time.time + m_WeaponData.AttackInterval;
        //
        //     WeaponAttack.Attack();
        // }

        public virtual void Hit(float radius)
        {
            if (radius == 0)
            {
                Log.Error($"武器{transform.name}的攻击范围不应该是0,请设置帧事件的float属性");
                radius = 1;
            }
            
            var entities = AIUtility.FindBattleUnits((BattleUnit)OwnerEntity, RelationType.Hostile | RelationType.Neutral,
                transform.position, radius);
            foreach (var entity in entities)
            {
                Attack(entity);

            }
        }

        public virtual void Shoot()
        {
            if (WeaponAttack is RangeWeaponAttack rangeWeaponAttack)
            {
                rangeWeaponAttack.Attack();
            }
        }

        public virtual void Attack(TargetableObject victim)
        {
            AIUtility.Attack((BattleUnit)OwnerEntity,this,victim);
        }
    }
}
