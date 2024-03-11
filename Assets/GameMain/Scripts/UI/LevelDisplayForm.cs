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

   public Text distanceText;
   
   private GameBase gameBase;

   public Image skillFillImage;

   private Animator animator;
   private float requiredSkillPoint;
   
   
   public void Init(GameBase gameBase)
   {
      this.gameBase = gameBase;
      animator = GetComponent<Animator>();
      
         
      requiredSkillPoint = GameEntry.Config.GetFloat("Game.RequiredSkillPoint", 50);
   }

   public void OpenPauseForm()
   {
      GameEntry.UI.OpenUIForm(UIFormId.PauseForm);
   }

   public void UseSkill()
   {
      gameBase?.UseSkill();
      
   }
   private void Update()
   {
      UpdateUi();
   }

   private void UpdateUi()
   {
      scoreText.text = Mathf.RoundToInt( gameBase.score)+"";

      skillFillImage.fillAmount = gameBase.skillPoint/requiredSkillPoint;

      distanceText.text = Mathf.CeilToInt(gameBase.playerMove.distance)+"米";
     
      animator.SetBool("Ready",gameBase.skillPoint >= requiredSkillPoint);
      
   }
}
