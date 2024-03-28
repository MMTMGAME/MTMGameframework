//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameMain
{
    [Serializable]
    public class WeaponData : AccessoryObjectData
    {
        [SerializeField]
        private int m_Attack = 0;

        [SerializeField]
        private float m_AttackInterval = 0f;

        [SerializeField]
        private int m_BulletId = 0;

        [SerializeField]
        private float m_BulletSpeed = 0f;

        [FormerlySerializedAs("m_BulletSoundId")] [SerializeField]
        private int m_ShootSoundId = 0;

        

        private string attackLogicComponent;
        [FormerlySerializedAs("shootPoint")] public string shootPointPath;

        //所处玩家槽位
        private int slotIndex;
        public WeaponData(int entityId, int typeId, int ownerId, CampType ownerCamp,int slotIndex)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            IDataTable<DRWeapon> dtWeapon = GameEntry.DataTable.GetDataTable<DRWeapon>();
            DRWeapon drWeapon = dtWeapon.GetDataRow(TypeId);
            if (drWeapon == null)
            {
                return;
            }

            this.slotIndex = slotIndex;
            m_Attack = drWeapon.Attack;
            m_AttackInterval = drWeapon.AttackInterval;
            m_BulletId = drWeapon.BulletId;
            m_BulletSpeed = drWeapon.BulletSpeed;
            m_ShootSoundId = drWeapon.ShootSoundId;
            
            attackLogicComponent = drWeapon.AttackLogicComponent;
            shootPointPath = drWeapon.ShootPoint;
        }

        /// <summary>
        /// 攻击力。
        /// </summary>
        public int Attack
        {
            get
            {
                return m_Attack;
            }
        }

        /// <summary>
        /// 攻击间隔。
        /// </summary>
        public float AttackInterval
        {
            get
            {
                return m_AttackInterval;
            }
        }

        /// <summary>
        /// 子弹编号。
        /// </summary>
        public int BulletId
        {
            get
            {
                return m_BulletId;
            }
        }

        /// <summary>
        /// 子弹速度。
        /// </summary>
        public float BulletSpeed
        {
            get
            {
                return m_BulletSpeed;
            }
        }

        /// <summary>
        /// 子弹声音编号。
        /// </summary>
        public int ShootSoundId
        {
            get
            {
                return m_ShootSoundId;
            }
        }

        public int SlotIndex
        {
            get
            {
                return slotIndex;
            }
        }

        public string AttackLogicComponent
        {
            get
            {
                return attackLogicComponent;
            }
        }
    }
}
