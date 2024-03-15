using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class EffectCloud : Entity
{

    private Transform playerTransform;

    private EffectCloudData effectCloudData;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        effectCloudData = (EffectCloudData)userData;
        playerTransform = effectCloudData.followTarget;
    }


    // Update is called once per frame
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        transform.position = playerTransform.position;
    }
}
