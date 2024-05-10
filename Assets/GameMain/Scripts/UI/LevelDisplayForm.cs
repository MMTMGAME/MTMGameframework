using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayForm : UGuiForm
{
   public Text levelTargetComp;
   public Text dropTextComp;
   public Text useTextComp;
   public GameObject tipRoot;

   public Text scoreText;
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
      
      GameEntry.Event.Subscribe(ScoreChangeEventArgs.EventId,OnScoreChange);
   }

   void OnScoreChange(object sender, GameEventArgs gameEventArgs)
   {
      ScoreChangeEventArgs args = (ScoreChangeEventArgs)gameEventArgs;
      scoreText.text = "分数 "+args.newValue;
   }

   protected override void OnClose(bool isShutdown, object userData)
   {
      GameEntry.Event.Unsubscribe(ScoreChangeEventArgs.EventId,OnScoreChange);
      base.OnClose(isShutdown, userData);
   }
}
