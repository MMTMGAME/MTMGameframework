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

        public BattleUnit OwnerBattleUnit
        {
            get { return (BattleUnit)ownerEntity; }
        }

        [SerializeField]
        public WeaponData m_WeaponData = null;

        //private float m_NextAttackTime = 0f;
        
       

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


            GameEntry.Timer.AddFrameTimer(() =>//此时还没有bindManager
            {
                var battleUnit = ownerEntity.Logic as BattleUnit;
                var pathKey = battleUnit.GetBattleUnitData().GetWeaponPath(m_WeaponData.SlotIndex);
                var bindPoint = battleUnit.chaState.GetBindManager()
                    .GetBindPointByKey(pathKey);

                GameEntry.Entity.AttachEntity(Entity, m_WeaponData.OwnerId,
                    bindPoint == null ? battleUnit.transform : bindPoint.transform);

            });

            
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
        
        
    }
}
