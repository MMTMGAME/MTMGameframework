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
    public class Bullet : Entity
    {
        [SerializeField]
        private BulletData m_BulletData = null;

       
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
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_BulletData = userData as BulletData;
            if (m_BulletData == null)
            {
                Log.Error("Bullet data is invalid.");
                return;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Entity entity = other.gameObject.GetComponentInParent<Entity>();
            if (entity == null)
            {
                return;
            }
        
            if (entity is TargetableObject)
            {
                (entity as TargetableObject).ApplyDamage(this, m_BulletData.Attack);
               
            }
        
           
        }

        // private void OnCollisionEnter(Collision collision)
        // {
        //     Entity entity = collision.gameObject.GetComponentInParent<Entity>();
        //     if (entity == null)
        //     {
        //         return;
        //     }
        //
        //     if (entity is TargetableObject)
        //     {
        //         (entity as TargetableObject).ApplyDamage(this, m_BulletData.Attack);
        //         
        //     }
        // }

        private float timer = 0;
#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            timer += elapseSeconds;
            CachedTransform.Translate(Vector3.forward * m_BulletData.Speed * elapseSeconds, Space.Self);
            
            if (timer >= m_BulletData.KeepTime)
            {
               
                GameEntry.Entity.HideEntity(this.Entity);
            }
        }
    }
}
