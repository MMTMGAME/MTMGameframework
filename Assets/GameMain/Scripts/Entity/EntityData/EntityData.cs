﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using Unity.VisualScripting;
using UnityEngine;

namespace GameMain
{
    
    [IncludeInSettings(true)]
    public class AIData
    {
        public string AssetName { get; set; }

        public StateGraphAsset stateGraphAsset;
        public float Radius { get; set; }
        public float AttackDistance { get; set; }

        public bool AddAIMove { get; set; }
        public bool AddAIRotate { get; set; }
    }
    
    [System.Serializable]
    public abstract class EntityData
    {
        
        public AIData AIData { get; set; }

        [SerializeField]
        private int m_Id = 0;

        [SerializeField]
        private int m_TypeId = 0;

        [SerializeField]
        private Vector3 m_Position = Vector3.zero;

        [SerializeField]
        private Quaternion m_Rotation = Quaternion.identity;

        public EntityData(int entityId, int typeId)
        {
            m_Id = entityId;
            m_TypeId = typeId;
            
            //获取AI数据
            var comp = GameEntry.SoDataTableComponent;
            var aiTable = GameEntry.SoDataTableComponent.GetTable<AiDataRow>();
            var drAi = aiTable.GetDataRow(TypeId) as AiDataRow;
            if (drAi != null)
            {
                AIData = new AIData()
                {
                    Radius = drAi.radius,
                    stateGraphAsset = drAi.stateGraph,
                    AttackDistance = drAi.attackDistance,
                    AddAIMove = drAi.addAiMove,
                    AddAIRotate = drAi.addAiMove
                };
            }
        }

        /// <summary>
        /// 实体编号。
        /// </summary>
        public int Id
        {
            get
            {
                return m_Id;
            }
            set { }
        }

        /// <summary>
        /// 实体类型编号。
        /// </summary>
        public int TypeId
        {
            get
            {
                return m_TypeId;
            }
        }

        /// <summary>
        /// 实体位置。
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
            }
        }
        
        public Action<Entity> OnShowCallBack
        {
            get;
            set;
        }

        /// <summary>
        /// 实体朝向。
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return m_Rotation;
            }
            set
            {
                m_Rotation = value;
            }
        }
    }
}
