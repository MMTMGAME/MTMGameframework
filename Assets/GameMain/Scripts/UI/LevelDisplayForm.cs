using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayForm : UGuiForm
{
   public Text levelTargetComp;
   public Text dropTextComp;
   public Text useTextComp;
   public GameObject tipRoot;

   public void SetLevelTarget(string key)
   {
      levelTargetComp.text = GameEntry.Localization.GetString(key);
   }

   public void SwitchUseAndDropTip(bool targetStatus)
   {
      tipRoot.gameObject.SetActive(targetStatus);
   }

   protected override void OnOpen(object userData)
   {
      base.OnOpen(userData);
      dropTextComp.text = GameEntry.Localization.GetString("GameControl.DropItemTip");
      useTextComp.text = GameEntry.Localization.GetString("GameControl.UseItemTip");
   }
}
