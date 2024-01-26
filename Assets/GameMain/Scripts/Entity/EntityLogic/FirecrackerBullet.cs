using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class FirecrackerBullet : PhysicsBullet
{
   protected override void OnHide(bool isShutdown, object userData)
   {
      base.OnHide(isShutdown, userData);
      var entity =GameEntry.Entity.GetEntity(m_BulletData.OwnerId);
      if(entity==null)
         return;
      var owner = (TargetableObject)entity.Logic;
      if(owner==null)
         return;
      //AIUtility.Explosion(owner,transform.position,0.2f,m_BulletData.Attack);
      
      
      // GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),m_BulletData.hideEffectId)
      // {
      //    Position = transform.position,
      //    Rotation = UnityEngine.Random.rotation,
      // });
      
      //GameEntry.Sound.PlaySound(100);
      
   }
}
