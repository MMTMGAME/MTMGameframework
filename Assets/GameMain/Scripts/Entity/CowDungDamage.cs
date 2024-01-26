using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class CowDungDamage : MonoBehaviour
{
    private SceneItem cowDung;

   

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player") || collider.gameObject.layer==LayerMask.NameToLayer("BodyBones"))
        {
            Debug.LogError("碰到牛粪"+collider.gameObject.name);
            cowDung = GetComponentInParent<SceneItem>();
            var victim = collider.gameObject.GetComponentInParent<TargetableObject>();
            victim.ApplyDamage(cowDung,cowDung.sceneItemData.attack);
        }
    }
    
    
}
