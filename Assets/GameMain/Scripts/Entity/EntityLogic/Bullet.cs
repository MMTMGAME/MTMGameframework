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
            
            

            if (bulletStrategy == null)//避免对象池重复添加
            {
                AddBulletStrategyComponent(m_BulletData.bulletStrategyComp);
            }
            else
            {
                bulletStrategy.Init(this);
            }
            
            
            var fireSoundId = m_BulletData.fireSoundId;
            if (fireSoundId > 0)
            {
                Debug.Log("hideSoundId");
                GameEntry.Sound.PlaySound(fireSoundId,transform.position);
            }

            var fireEffectId = m_BulletData.fireEffectId;
            if (fireEffectId > 0)
            {
                GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),fireEffectId)
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                });
            }

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

        public void PlaySoundAndFxByPhysMat(Collider col)
        {
            Debug.Log("PlaySoundAndFxByPhysMat");
            var physicsMaterial = col.material;
            string matName = "Default";
            if (physicsMaterial != null)
                matName = physicsMaterial.name;
            if (matName == "")
                matName = "Default";
            matName = matName.Replace(" (Instance)", "");
            GameEntry.Sound.PlayBulletImpactSound(m_BulletData.TypeId, matName,transform.position);
            GameEntry.Entity.ShowBulletImpactEffect(m_BulletData.TypeId,matName,transform.position,transform.rotation);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            bulletStrategy.PerformTrigger(other);
        }

       
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(gameObject.name+"碰撞到"+collision.gameObject.name);
            bulletStrategy.PerformCollision(collision);
            PlaySoundAndFxByPhysMat(collision.collider);
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
            var hideSoundId = m_BulletData.hideSoundIds.RandomNonEmptyElement();
            if (hideSoundId > 0)
            {
                Debug.Log("hideSoundId");
                GameEntry.Sound.PlaySound(hideSoundId,transform.position);
            }
            
            var hideEffectId = m_BulletData.hideEffectId;
            if (hideEffectId > 0)
            {
                GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),hideEffectId)
                {
                    Position = transform.position,
                    Rotation = transform.rotation
                });
            }
            
          
            
            
            base.OnHide(isShutdown, userData);
            
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
            timer = 0;
        }
    }
}
