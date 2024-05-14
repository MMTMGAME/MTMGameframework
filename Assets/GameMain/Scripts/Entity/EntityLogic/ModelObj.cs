using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class ModelObj : Entity
{
    public ModelObjData modelObjData;
    //用于展示纯模型实体，比如展示子弹的视觉模块和角色场景的视效模块
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        this.modelObjData = (ModelObjData)userData;
    }
}
