//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Timers;
using UGFExtensions;
using UGFExtensions.Timer;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        public static BuiltinDataComponent BuiltinData
        {
            get;
            private set;
        }
        
        public static EntityUiComponent EntityUi
        {
            get;
            private set;
        }
        
        public static AIComponent AI
        {
            get;
            private set;
        }

        public static TimerComponent Timer
        {
            get;
            private set;
        }

        public static TimingWheelComponent TimingWheel
        {
            get;
            private set;
        }

        private static void InitCustomComponents()
        {
            BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            EntityUi = UnityGameFramework.Runtime.GameEntry.GetComponent<EntityUiComponent>();
            AI=UnityGameFramework.Runtime.GameEntry.GetComponent<AIComponent>();
            Timer=UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            TimingWheel=UnityGameFramework.Runtime.GameEntry.GetComponent<TimingWheelComponent>();
        }
    }
}
