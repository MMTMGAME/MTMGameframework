using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class FirecrackerRoll : SceneItem
{
   private ParticleSystem ParticleSystem;
   private Transform explosionPos;

   public bool FireComplete
   {
      get;
      private set;
   } //燃放完毕

   protected override void OnShow(object userData)
   {
      base.OnShow(userData);
      ParticleSystem = GetComponentInChildren<ParticleSystem>();
      ParticleSystem.Stop();

      explosionPos = transform.Find("ExplosionPos");
      FireComplete = false;
   }

   protected override void OnDead(Entity attacker)
   {
     
      StartCoroutine(Fire());
   }

   IEnumerator Fire()
   {
      ParticleSystem.Play();
      //播放引信声音
      GameEntry.Sound.PlaySound(10008,transform.position);
      yield return new WaitForSeconds(3);

      int? id1=GameEntry.Sound.PlaySound(10009,transform.position);
      //int? id2=GameEntry.Sound.PlaySound(10010,transform.position);
      yield return new WaitForSeconds(0.1f);
      int? id3=GameEntry.Sound.PlaySound(10009,transform.position);
      yield return new WaitForSeconds(0.1f);
      int? id4=GameEntry.Sound.PlaySound(10009,transform.position);
      
      float elpasedTimer = 0;
      while (elpasedTimer < 15)
      {
         transform.position+=Vector3.up * (0.1f * Time.deltaTime);

         var spawn = UnityEngine.Random.Range(0, 2) > 0;
         if (spawn)
         {
            GameEntry.Entity.ShowFirecrackerBullet(new PhysicsBulletData(GameEntry.Entity.GenerateSerialId(),60001,Id,CampType.Neutral,sceneItemData.attack,UnityEngine.Random.Range(300,500),0.22f)
            {
               Position = explosionPos.transform.position,
               Rotation = UnityEngine.Random.rotation
            });
         }
        
         //生成爆竹子弹
         yield return null;
         elpasedTimer += Time.deltaTime;
         

      }

      GameEntry.Sound.StopSound(id1.Value);
      //GameEntry.Sound.StopSound(id2.Value);
      GameEntry.Sound.StopSound(id3.Value);
      GameEntry.Sound.StopSound(id4.Value);
      yield return new WaitForSeconds(0.5f);
      FireComplete = true;
   }
}
