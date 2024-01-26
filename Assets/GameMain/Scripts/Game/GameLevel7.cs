using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;

public class GameLevel7 : GameBase
{

   public FirecrackerRoll firecrackerRoll;
   protected override void CheckGameOverOrWin()
   {
      base.CheckGameOverOrWin();

      if (firecrackerRoll != null && firecrackerRoll.FireComplete && Player.IsDead == false)
      {
         GameWin = true;
      }
   }


   protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
   {
      base.OnShowEntitySuccess(sender, e);
      ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
      if (ne.EntityLogicType == typeof(FirecrackerRoll))
      {
         firecrackerRoll = ne.Entity.Logic as FirecrackerRoll;
        
      }
   }
}
