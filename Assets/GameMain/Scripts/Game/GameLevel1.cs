//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------


using GameFramework.Event;

using UnityGameFramework.Runtime;

namespace GameMain
{

    public class GameLevel1 : GameBase
    {
       
        public SkyRocket SkyRocket { get; private set; }


        protected override void CheckGameOverOrWin()
        {
            base.CheckGameOverOrWin();
            if (SkyRocket != null && !SkyRocket.Available && Player.IsDead == false)
            {
                GameWin = true;
            }
        }


        protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            base.OnShowEntitySuccess(sender,e);
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(SkyRocket))
            {

                SkyRocket = ne.Entity.Logic as SkyRocket;

            }
        }

      
    }
}
