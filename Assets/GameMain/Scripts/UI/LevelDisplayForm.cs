using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

public class LevelDisplayForm : UGuiForm
{
   public Text scoreText;

   public Image primogemIcon;

   private GameBase gameBase;
   public void Init(GameBase gameBase)
   {
      this.gameBase = gameBase;
   }

   

   private void Update()
   {
      UpdateUi();
   }

   private void UpdateUi()
   {
      scoreText.text = Mathf.RoundToInt( gameBase.score)+"";
   }
}
