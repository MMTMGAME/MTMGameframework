using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;

[Serializable]
public class SceneItem : TargetableObject
{
   //我好像啥都不用干

   public SceneItemData sceneItemData;

   protected Rigidbody RigidbodyComp { get;  set; }
   protected Collider colliderComp;

   public bool holding;

   public override ImpactData GetImpactData()
   {
      return new ImpactData(CampType.Neutral, sceneItemData.HP, sceneItemData.attack, sceneItemData.defense);
   }

   protected override void OnShow(object userData)
   {
      base.OnShow(userData);
      sceneItemData=userData as SceneItemData;
      RigidbodyComp=CachedTransform.gameObject.GetComponent<Rigidbody>();
      if(RigidbodyComp==null)
         RigidbodyComp=CachedTransform.gameObject.AddComponent<Rigidbody>();
      colliderComp = CachedTransform.gameObject.GetComponent<Collider>();
      
      //刷新在场景中时都应该是不会动的
      RigidbodyComp.isKinematic = true;
   }

   protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
   {
      base.OnAttachTo(parentEntity, parentTransform, userData);
      RigidbodyComp.isKinematic = true;
     
      
    
      CachedTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
      CachedTransform.gameObject.layer=Constant.Layer.HoldingSceneItemLayerId;
      
      holding = true;
   }

   protected override void OnDetachFrom(EntityLogic parentEntity, object userData)
   {
      base.OnDetachFrom(parentEntity, userData);
      RigidbodyComp.isKinematic = false;
     
      
      CachedTransform.gameObject.layer=Constant.Layer.SceneItemLayerId;
      
      holding = false;
   }

   public virtual void StartUse()
   {
      
   }

   public virtual void StopUse()
   {
      
   }
}
