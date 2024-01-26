using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.ObjectPool;
using GameMain;
using UnityEngine;

public class InteractTipItemObject : ObjectBase
{
    public static InteractTipItemObject Create(object target)
    {
        InteractTipItemObject interactTipItemObject = ReferencePool.Acquire<InteractTipItemObject>();
        interactTipItemObject.Initialize(target);
        return interactTipItemObject;
    }

    protected override void Release(bool isShutdown)
    {
        InteractTipItem hpBarItem = (InteractTipItem)Target;
        if (hpBarItem == null)
        {
            return;
        }

        Object.Destroy(hpBarItem.gameObject);
    }

    
}
