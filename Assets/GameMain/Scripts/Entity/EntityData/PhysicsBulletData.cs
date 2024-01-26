//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameFramework.DataTable;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class PhysicsBulletData : EntityData
    {
        [SerializeField]
        private int m_OwnerId = 0;

        [SerializeField]
        private CampType m_OwnerCamp = CampType.Unknown;

        [SerializeField]
        private int m_Attack = 0;

        [SerializeField]
        private float m_Force = 0f;

        [SerializeField]
        private float m_keepTime = 1f;
        
        public int fireSoundId;
        public int[] deadSoundIds;
        public int[] hideSoundIds;
       
        public int fireEffectId;
        public int deadEffectId;
        public int hideEffectId;

        public PhysicsBulletData(int entityId, int typeId, int ownerId, CampType ownerCamp, int attack, float force,float keepTime)
            : base(entityId, typeId)
        {
            m_OwnerId = ownerId;
            m_OwnerCamp = ownerCamp;
            m_Attack = attack;
            m_Force = force;
            this.m_keepTime = keepTime;
            
            IDataTable<DRBullet> table = GameEntry.DataTable.GetDataTable<DRBullet>();
            DRBullet element = table.GetDataRow(TypeId);
            if (element == null)
            {
                return;
            }

            string[] deadSoundIdsArr = element.DeadSoundIdArrStr.Split(',');
            List<int> deadSoundIdsList = new List<int>();
            foreach (var id in deadSoundIdsArr)
            {
                if (int.TryParse(id, out int result))
                {
                    deadSoundIdsList.Add(result);
                }
            } 
            deadSoundIds = deadSoundIdsList.ToArray();

            string[] hideSoundIdsArr = element.HideSoundIdArrStr.Split(',');
            List<int> hideSoundIdsList = new List<int>();
            foreach (var id in hideSoundIdsArr)
            {
                if (int.TryParse(id, out int result))
                {
                    hideSoundIdsList.Add(result);
                }
            }
            hideSoundIds = hideSoundIdsList.ToArray();



            fireEffectId = element.FireEffectId;
            deadEffectId = element.DeadEffectId;
            hideEffectId = element.HideEffectId;
        }

        public int OwnerId
        {
            get
            {
                return m_OwnerId;
            }
        }

        public CampType OwnerCamp
        {
            get
            {
                return m_OwnerCamp;
            }
        }

        public int Attack
        {
            get
            {
                return m_Attack;
            }
        }

        public float Force
        {
            get
            {
                return m_Force;
            }
        }

        public float KeepTime
        {
            get
            {
                return m_keepTime;
            }
        }
    }
}
