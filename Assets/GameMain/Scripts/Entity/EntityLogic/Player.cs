using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class Player : TargetableObject
{
    private PlayerData playerData;

    private Rigidbody spineRigid;
 

    [Header("用于设置初始位置")]
    private Transform spine;

    public Action<UnityGameFramework.Runtime.Entity,string> OnAttachItem;
    public Action<UnityGameFramework.Runtime.Entity, string> OnDetachItem;
  
    //也没啥需要加载的
    public override ImpactData GetImpactData()
    {
        return new ImpactData(CampType.Player, playerData.HP, 1, 0);
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
       
        playerData=userData as PlayerData;

        spine = transform.Find("spine");
         spineRigid = spine.GetComponent<Rigidbody>();
       
        
        
        
        spineRigid.MovePosition(playerData.SpinePosition);
        spineRigid.MoveRotation(playerData.SpineRotation);
    }

    public override void ApplyDamage(Entity attacker, int damageHP)
    {
        base.ApplyDamage(attacker,damageHP);
        Vector3 hitForce = Vector3.up * 5000; // forceMagnitude 是你想施加的力的大小
        spineRigid .AddForceAtPosition(hitForce, spineRigid.centerOfMass);
        GameEntry.Sound.PlaySound(20000,transform.position);
    }

    protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
    {
        base.OnAttached(childEntity, parentTransform, userData);
        OnAttachItem?.Invoke(childEntity.Entity,parentTransform.parent.name);
    }

    protected override void OnDetached(EntityLogic childEntity, object userData)
    {
        base.OnDetached(childEntity, userData);
        OnDetachItem?.Invoke(childEntity.Entity,childEntity.Entity.transform.parent.parent.name);
    }
}
