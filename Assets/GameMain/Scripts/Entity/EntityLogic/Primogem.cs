using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameMain;
using UnityEngine;

public class Primogem : Entity
{
    private PrimogemData primogemData;

    private Tween tween;
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        primogemData = (PrimogemData)userData;
        GameEntry.Entity.AttachEntity(this,primogemData.ownerId);
        tween=transform.DOJump(transform.position + Vector3.up * 0.1f, 1f, 1, 2).SetLoops(-1);
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        transform.Rotate(Vector3.up,30*elapseSeconds);
        
    }


    protected override void OnHide(bool isShutdown, object userData)
    {
        base.OnHide(isShutdown, userData);
        tween?.Kill();
    }


    private void OnTriggerEnter(Collider other)
    {
        var player = other.gameObject.GetComponent<Player>();
        if (player != null)
        {
            GameEntry.PrimogemPicker.ShowUi(this);
            if (GameEntry.Procedure.CurrentProcedure is ProcedureLevel procedure)
            {
                procedure.GetGameBase().AddScore(1);
                GameEntry.Sound.PlaySound(10001);
                GameEntry.Entity.HideEntity(this);
            }
            
            //添加原石
        }
    }
}
