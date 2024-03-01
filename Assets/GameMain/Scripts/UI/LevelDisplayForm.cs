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

   public Image skillFillImage;

   private Animator animator;
   public void Init(GameBase gameBase)
   {
      this.gameBase = gameBase;
      animator = GetComponent<Animator>();
   }

   

   private void Update()
   {
      UpdateUi();
   }

   private void UpdateUi()
   {
      scoreText.text = Mathf.RoundToInt( gameBase.score)+"";

      skillFillImage.fillAmount = gameBase.skillPoint/50;

     
      animator.SetBool("Ready",gameBase.skillPoint >= 50);
      
   }
}
