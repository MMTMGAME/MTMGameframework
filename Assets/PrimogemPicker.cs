using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class PrimogemPicker : GameFrameworkComponent
{
    public GameObject primogemUiItem;
    public Canvas canvas;

    public void ShowUi(Primogem primogem)
    {
        if(primogem==null)
            return;
       

        var procedure = GameEntry.Procedure.CurrentProcedure as ProcedureLevel;
        if (procedure==null)
        {
            return;
        }

        var levelDisplayUi = procedure.GetGameBase().LevelDisplayForm;

        var targetTrans = levelDisplayUi.primogemIcon.transform;

        var instance = GameObject.Instantiate(primogemUiItem, canvas.transform);

        var screenPos = Camera.main.WorldToScreenPoint(primogem.transform.position);
        instance.transform.transform.position = screenPos;
        instance.transform.DOMove(targetTrans.position, 1f).OnComplete(()=>
        {
            Destroy(instance);
            
        });
    }
}
