using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;

public class FireDamage : MonoBehaviour
{
   private Entity lighter;
   public float damageInterval = 1;
   private float lastDamageTime;
   private void OnTriggerStay(Collider other)
   {
      if (Time.time > lastDamageTime + damageInterval)
      {
         if (other.gameObject.layer == LayerMask.NameToLayer("BodyBones") ||
             other.gameObject.layer == LayerMask.NameToLayer("SceneItem"))
         {
            var entity = other.transform.GetComponentInParent<TargetableObject>();
            if (entity != null)
            {
               lastDamageTime = Time.time; // 更新上次伤害时间

               //Log.Error("好烧呀");

               lighter = GetComponentInParent<SceneItem>();
               entity.ApplyDamage(lighter, 1);

               //怎么不扣血了？
            }
         }
      }
   }

}
