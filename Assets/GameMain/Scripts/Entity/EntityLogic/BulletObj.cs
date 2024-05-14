using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class BulletObj : Entity
{
   private BulletObjData bulletObjData;

   protected override void OnShow(object userData)
   {
      base.OnShow(userData);
      bulletObjData = (BulletObjData)userData;
   }
}
