using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;


public class Lighter : SceneItem
{
   private GameObject fireColliderAndFx;

   private void Start()
   {
      fireColliderAndFx = GetComponentInChildren<ParticleSystem>().gameObject;
      StopUse();
   }

   public override void StartUse()
   {
      base.StartUse();
      fireColliderAndFx.gameObject.SetActive(true);
      GameEntry.Sound.PlaySound(10004,transform.position);
   }

   public override void StopUse()
   {
      fireColliderAndFx.gameObject.SetActive(false);
      GameEntry.Sound.PlaySound(10005,transform.position);
   }
}
