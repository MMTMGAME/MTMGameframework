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
   public Image useSkillKeyTip;
   
   private GameBase gameBase;

   public Image skillFillImage;

   private Animator animator;
   private float requiredSkillPoint;


   private bool isWindows;
   
   public void Init(GameBase gameBase)
   {
      this.gameBase = gameBase;
      animator = GetComponent<Animator>();
      
         
      requiredSkillPoint = GameEntry.Config.GetFloat("Game.RequiredSkillPoint", 50);
      isWindows = !Application.isMobilePlatform;
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

      if (isWindows)
      {
         TrySetActive(useSkillKeyTip.gameObject,gameBase.skillPoint >= requiredSkillPoint);
      }
      
   }

   void TrySetActive(GameObject target,bool status)
   {
      if(target.activeSelf==status)
         return;//检测是否有必要SetActive，否则会很耗性能
      target.SetActive(status);
      
   }
}
