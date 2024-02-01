using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Entity=GameMain.Entity;
using GameEntry=GameMain.GameEntry;

public class InteractTipItem : EntityUiItem
{
  

    public Text textComp;


    public override void Init(Entity owner, params object[] args)
    {
        base.Init(owner, args);
        var msg = args[0].ToString();
        textComp.text = msg;
    }
}
