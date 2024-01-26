using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class LevelSelectForm : UGuiForm
{
    //可以搞LevelSelectItemUi并且读表加载，但是没必要
    public GameObject[] lockMaskBgs;

    private int levelProgress;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        levelProgress = PlayerPrefs.GetInt("LevelProgress", 1);

        for (int i = 0; i < levelProgress; i++)
        {
            lockMaskBgs[i].gameObject.SetActive(false);
        }
    }

    public void SelectLevel(int level)
    {
        if (levelProgress < level)
        {
            return;
            
        }
        
        var procedureMenu = GameEntry.Procedure.CurrentProcedure as ProcedureMenu;
        procedureMenu.SelectLevel(level);
        Close(true);
        
    }
}
