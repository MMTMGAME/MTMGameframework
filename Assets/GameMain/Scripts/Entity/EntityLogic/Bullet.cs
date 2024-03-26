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
        public BulletData m_BulletData = null;

        public BulletStrategy bulletStrategy;
        public BattleData GetImpactData()
        {
            return new BattleData(m_BulletData.OwnerCamp, 0,  0);
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
            gameObject.SetActive(true);
            timer = 0;

            AddBulletStrategyComponent(m_BulletData.bulletStrategyComp);
            
        }

        private void AddBulletStrategyComponent(string className)
        {
            Type toAdd = Type.GetType(className);
            if (toAdd != null && toAdd.IsSubclassOf(typeof(BulletStrategy)))
            {
                this.bulletStrategy = (BulletStrategy)gameObject.AddComponent(toAdd);
                if (this.bulletStrategy == null)
                {
                    Log.Fatal("添加子弹策略出错，目标策略名："+className);
                    return;
                }
                // 如果需要，这里可以对 attackComponent 进行进一步的配置
                this.bulletStrategy.Init(this);
            }
            else
            {
                Debug.LogError($"BulletStrategy logic class '{className}' not found or does not extend BulletStrategy.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            bulletStrategy.PerformTrigger(other);
            
           
        }

        private void OnCollisionEnter(Collision collision)
        {
            bulletStrategy.PerformCollision(collision);
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
            
            bulletStrategy.Update();
            
            if (timer >= m_BulletData.KeepTime)
            {
               
                GameEntry.Entity.HideEntity(this.Entity);
            }
        }
    }
}
