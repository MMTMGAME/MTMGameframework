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
            
            

          
            
            // var fireSoundId = m_BulletData.fireSoundId;
            // if (fireSoundId > 0)
            // {
            //     Debug.Log("hideSoundId");
            //     GameEntry.Sound.PlaySound(fireSoundId,transform.position);
            // }
            //
            // var fireEffectId = m_BulletData.fireEffectId;
            // if (fireEffectId > 0)
            // {
            //     GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),fireEffectId)
            //     {
            //         Position = transform.position,
            //         Rotation = transform.rotation,
            //     });
            // }

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
        
        


//         private float timer = 0;
// #if UNITY_2017_3_OR_NEWER
//         protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
// #else
//         protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
// #endif
//         {
//             base.OnUpdate(elapseSeconds, realElapseSeconds);
//
//             timer += elapseSeconds;
//             
//             
//             
//             if (timer >= m_BulletData.KeepTime)
//             {
//                
//                 GameEntry.Entity.HideEntity(this.Entity);
//             }
//         }
//
//    
//         protected override void OnRecycle()
//         {
//             base.OnRecycle();
//             timer = 0;
//         }
    }
}
