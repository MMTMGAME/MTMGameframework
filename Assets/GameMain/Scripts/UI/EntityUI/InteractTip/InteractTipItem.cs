using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Entity=GameMain.Entity;
using GameEntry=GameMain.GameEntry;

public class ShowInteractTipItemInfoData : ShowEntityUiItemInfo
{
    public string msg;
    

    public ShowInteractTipItemInfoData(int serialId, Entity entity, int typeId, string msg) : base(serialId, entity, typeId)
    {
        this.msg = msg;
    }
}
public class InteractTipItem : EntityUiItem
{
  

    public Text textComp;


    public override void Init(object userData)
    {
        base.Init(userData);
        ShowInteractTipItemInfoData data=userData as ShowInteractTipItemInfoData;
        var msg = data.msg.ToString();
        textComp.text = msg;
    }
}
