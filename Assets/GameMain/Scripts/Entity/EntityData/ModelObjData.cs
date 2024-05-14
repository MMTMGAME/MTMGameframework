using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class ModelObjData : EntityData
{
    
    //用于展示纯模型实体，比如展示子弹的视觉模块和角色场景的视效模块
    public ModelObjData(int entityId, int typeId) : base(entityId, typeId)
    {
    }
}
