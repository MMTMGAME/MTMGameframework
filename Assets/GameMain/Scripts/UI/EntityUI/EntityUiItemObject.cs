using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.ObjectPool;
using GameMain;
using UnityEngine;

public class EntityUiItemObject : ObjectBase
{
    

    public static EntityUiItemObject Create(object target)
    {
        EntityUiItemObject entityUiItemObject = ReferencePool.Acquire<EntityUiItemObject>();
        entityUiItemObject.Initialize(target);
        return entityUiItemObject;
    }

    protected override void Release(bool isShutdown)
    {
        EntityUiItem entityUiItem = (EntityUiItem)Target;
        if (entityUiItem == null)
        {
            return;
        }

        Object.Destroy(entityUiItem.gameObject);
    }
}
