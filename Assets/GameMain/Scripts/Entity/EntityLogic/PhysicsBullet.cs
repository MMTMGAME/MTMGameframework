//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// 子弹类。
    /// </summary>
    public class PhysicsBullet : Entity
    {
        [SerializeField]
        protected PhysicsBulletData m_BulletData = null;

        private Rigidbody rigidbody;
        public ImpactData GetImpactData()
        {
            return new ImpactData(m_BulletData.OwnerCamp, 0, m_BulletData.Attack, 0);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
            rigidbody = GetComponent<Rigidbody>();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_BulletData = userData as PhysicsBulletData;
            if (m_BulletData == null)
            {
                Log.Error("Bullet data is invalid.");
                return;
            }
            rigidbody.AddForce(transform.forward*m_BulletData.Force);

            timer = 0;
        }

        

        private void OnCollisionEnter(Collision collision)
        {
            Entity entity = collision.gameObject.GetComponentInParent<Entity>();
            if (entity == null)
            {
                return;
            }

            if (entity is TargetableObject)
            {
                (entity as TargetableObject).ApplyDamage(this, m_BulletData.Attack);
                
            }
        }

        private float timer = 0;
#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            timer += elapseSeconds;
            
            if (timer >= m_BulletData.KeepTime)
            {
                
                GameEntry.Entity.HideEntity(this.Entity);
            }
        }


        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);
            if(isShutdown)
                return;
            if(m_BulletData.hideEffectId==0)
                return;
           
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),m_BulletData.hideEffectId)
            {
                Position = transform.position,
            });

           
            if (m_BulletData.hideSoundIds is { Length: > 0 })
            {
                var targetId = m_BulletData.hideSoundIds.RandomNonEmptyElement();
                if(targetId!=0)
                    GameEntry.Sound.PlaySound(m_BulletData.hideSoundIds.RandomNonEmptyElement());
            }
            
        }
    }
}
