﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameMain
{
    [Serializable]
    public class WeaponData : AccessoryObjectData
    {
        
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
        
        //属性
        public ChaProperty[] propMod=new ChaProperty[2];

        public List<string> skills = new List<string>();
        public List<string> buffs = new List<string>();
        //所处玩家槽位
        private int slotIndex;
        public WeaponData(int entityId, int typeId, int ownerId, CampType ownerCamp,int slotIndex)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            //IDataTable<DRWeapon> dtWeapon = GameEntry.DataTable.GetDataTable<DRWeapon>();
            var drWeapon = GameEntry.SoDataTableComponent.GetSoDataRow<WeaponDataRow>(TypeId);
            if (drWeapon == null)
            {
                return;
            }

            this.slotIndex = slotIndex;
           
            m_AttackInterval = drWeapon.attackInterval;
           
            
            //属性
            propMod[0].hp = drWeapon.hpAdd;
            propMod[0].mp = drWeapon.mpAdd;
            propMod[0].attack = drWeapon.attackAdd;
            propMod[0].defense = drWeapon.defenseAdd;
            propMod[0].moveSpeed = drWeapon.moveSpeedAdd;
            propMod[0].actionSpeed = drWeapon.actionSpeedAdd;

            propMod[1].hp = drWeapon.hpTimes;
            propMod[1].mp = drWeapon.mpTimes;
            propMod[1].attack = drWeapon.attackTimes;
            propMod[1].defense = drWeapon.defenseTimes;
            propMod[1].moveSpeed = drWeapon.moveSpeedTimes;
            propMod[1].actionSpeed = drWeapon.actionSpeedTimes;

            skills = drWeapon.skills.ToList();
            buffs = drWeapon.buffs.ToList();
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
