using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class AoeObj : Entity
{
   private AoeObjData aoeObjData;

   protected override void OnShow(object userData)
   {
      base.OnShow(userData);
      aoeObjData = (AoeObjData)userData;
   }
}
