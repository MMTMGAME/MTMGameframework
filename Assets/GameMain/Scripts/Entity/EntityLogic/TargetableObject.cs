//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// 可作为目标的实体类。
    /// </summary>
    public class TargetableObject : Entity
    {

        private bool died;
        [SerializeField]
        private TargetableObjectData m_TargetableObjectData = null;

        public bool IsDead
        {
            get
            {
                return m_TargetableObjectData.HP <= 0;
            }
        }

        public bool IsValid()
        {
            return Visible && !IsDead;
        }

        public virtual BattleData GetImpactData()
        {
            return new BattleData(m_TargetableObjectData.Camp, m_TargetableObjectData.HP,0);
        }

        public virtual void ApplyDamage(BattleUnit attacker, int damageHP)
        {
            if(died)
                return;
            
            attacker.OnBeforeDamageVictim(this, damageHP);
            
            float fromHPRatio = m_TargetableObjectData.HPRatio;
            m_TargetableObjectData.HP -= damageHP;
            float toHPRatio = m_TargetableObjectData.HPRatio;
            
            attacker.OnAfterDamageVictim(this, damageHP);
            
            //Log.Debug(this.gameObject.name+"显示HPBarItem，血量:"+toHPRatio);
            GameEntry.EntityUi.ShowEntityUi (new ShowHpBarItemInfo(GameEntry.EntityUi.GenerateSerialId(), this, 1, toHPRatio));
            

            if (m_TargetableObjectData.HP <= 0 && !died)
            {
                died = true;
                OnDead(attacker);
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
            //gameObject.SetLayerRecursively(Constant.Layer.TargetableObjectLayerId);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_TargetableObjectData = userData as TargetableObjectData;
            if (m_TargetableObjectData == null)
            {
                Log.Error("Targetable object data is invalid.");
                return;
            }
        }

        protected virtual void OnDead(Entity attacker)
        {
            GameEntry.Entity.HideEntity(this);
        }
    }
}
