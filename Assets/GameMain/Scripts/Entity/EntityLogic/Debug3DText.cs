using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class Debug3DText : Entity
{
    private Debug3DTextData debug3DTextData;

    private TextMesh textMesh;
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        debug3DTextData = (Debug3DTextData)userData;

        textMesh = GetComponent<TextMesh>();
        textMesh.text = debug3DTextData.Msg;
        
        Invoke(nameof(HideSelf),debug3DTextData.Duration);
    }

    void HideSelf()
    {
        GameEntry.Entity.HideEntity(this);
    }
}
