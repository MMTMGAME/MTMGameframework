//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public abstract class ProcedureLevel : ProcedureBase
    {
        private  float GameOverDelayedSeconds = 2f;

        private readonly Dictionary<GameMode, GameBase> m_Games = new Dictionary<GameMode, GameBase>();
        private GameBase m_CurrentGame = null;
        private bool m_GotoMenu = false;
        private float m_GotoMenuDelaySeconds = 0f;

        protected ProcedureOwner procedureOwner;

        public GameBase GetGameBase()
        {
            return m_CurrentGame;
        }
        
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        public void GotoMenu(float delaySecond)
        {
           
            GameOverDelayedSeconds = delaySecond;
            m_GotoMenu = true;
        }

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            
            //这里只是Add,没有启用，启用是在OnEnter中启用
            m_Games.Add(GameMode.Level1, new GameLevel1());
          
            
            
        }

        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);

            m_Games.Clear();
        }

        public abstract GameMode GameLevel { get; }


        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_GotoMenu = false;
            
            m_CurrentGame = m_Games[GameLevel];//启用设置好的GameLevel
            m_CurrentGame.Initialize();

            m_GotoMenuDelaySeconds = 0;
            
            GameEnded = false;

            this.procedureOwner = procedureOwner;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Shutdown();
                m_CurrentGame = null;
            }
            

            base.OnLeave(procedureOwner, isShutdown);
        }

        public bool GameEnded
        {
            get;
            protected set;
        }
        
        
        protected virtual void OnGameEnd()
        {
            GameEntry.UI.CloseAllLoadedUIForms();
            
            //更新历史记录和排行榜
            DateTime dateTime = DateTime.Now; // 获取当前时间
            var nowTimeStr = dateTime.ToString("yyyy-MM-dd HH:mm"); // 格式化为年-月-日 时:分


            var nowScore = GetGameBase().Score;

            var leaderboardMgr = new LeaderboardManager();
            leaderboardMgr.UpdateLeaderboard(Mathf.RoundToInt(nowScore),nowTimeStr);

            var highestScore = leaderboardMgr.GetHighestScore();

            var elpasedTimeStr ="时间："+ TimeUtility.FormatElapsedTime(Mathf.RoundToInt( GetGameBase().ElapsedTime));
            var scoreInfoStr = $"分数：{nowScore}\n最高分：{highestScore}";
            
            var currentGame = GetGameBase();
            if (currentGame.GameWin)
            {
                GameEntry.UI.OpenDialog(new DialogParams()
                {
                    Mode = 2,
                    Title = GameEntry.Localization.GetString("GameWin.Title"),
                    Message = GameEntry.Localization.GetString("GameWin.Message"),
                    OnClickConfirm = delegate(object userData) { NextLevel(); },
                    OnClickCancel = (userData) => { GotoMenu(0.5f); },
                    ConfirmText = GameEntry.Localization.GetString("GameWin.NextLevel"),
                    CancelText = GameEntry.Localization.GetString("GameWin.MainMenu"),
                });

                //解锁下一关
                var levelProgress = PlayerPrefs.GetInt("LevelProgress", 1);
                
                //存档
                if((int)GameLevel+1>levelProgress)
                    PlayerPrefs.SetInt("LevelProgress",(int)GameLevel+1);
            }

            if (currentGame.GameOver)
            {
                //GameEntry.UI.OpenGameEndDialog();
                GameEntry.UI.OpenGameEndDialog(new DialogParams()
                {
                    Mode = 3,
                    Title = GameEntry.Localization.GetString("GameOver.Title"),
                    Message = elpasedTimeStr+"\n"+scoreInfoStr,
                    OnClickConfirm = delegate(object userData) { Retry(); },
                    ConfirmText = GameEntry.Localization.GetString("GameOver.Retry"),
                    OnClickCancel = delegate(object userData) { GotoMenu(0.5f); },
                    OnClickOther = delegate(object o) { GameEntry.UI.OpenUIForm(UIFormId.LeaderboardForm);},
                    OtherText = "排行榜",
                    CancelText = GameEntry.Localization.GetString("GameOver.MainMenu"),
                });
            }
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);


            if (m_GotoMenu)
            {
                m_GotoMenuDelaySeconds += elapseSeconds;
                if (m_GotoMenuDelaySeconds >= GameOverDelayedSeconds)
                {
                    procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Menu"));
                    ChangeState<ProcedureChangeScene>(procedureOwner);
                }
            }

           
            if(GameEnded)
                return;
            
            if (m_CurrentGame != null )
            {
                if (m_CurrentGame.GameOver || m_CurrentGame.GameWin)
                {
                    GameEnded = true;
                    OnGameEnd();
                    return;
                }
                m_CurrentGame.Update(elapseSeconds, realElapseSeconds);
                
            }
            
           
            
        }


        protected virtual void NextLevel()
        {
            // 示例
            // procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level2"));
            // procedureOwner.SetData<VarInt32>("TargetLevel", 2);
            // ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        public virtual void Retry()
        {
            ChangeState<ProcedureRetry>(procedureOwner);
        }
    }
}
