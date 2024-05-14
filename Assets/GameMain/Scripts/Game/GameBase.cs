//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections;
using System.Reflection;
using GameFramework;
using GameFramework.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

public class ScoreChangeEventArgs : GameEventArgs
{
    
    public static readonly int EventId = typeof(ScoreChangeEventArgs).GetHashCode();
    
    public float newValue;
    public float delta;
    
    public override void Clear()
    {
        newValue = 0;
        delta = 0;
    }

    public static ScoreChangeEventArgs Create(float newValue, float delta)
    {
        ScoreChangeEventArgs scoreChangeEventArgs=ReferencePool.Acquire<ScoreChangeEventArgs>();
        scoreChangeEventArgs.delta = delta;
        scoreChangeEventArgs.newValue = newValue;
        return scoreChangeEventArgs;
    }

    public override int Id => EventId;
}

namespace GameMain
{
    public abstract class GameBase
    {
        /// <summary>
        /// 记录这个GameBase加载了几次，没啥用，只是用来提醒Gamebase这个示例会被多次加载，所以需要注意在初始化的时候重置相关变量
        /// </summary>
        public int loadCount = 0;

        #region 游戏数据

        private float m_Score;
        public float Score
        {
            get { return m_Score;}
            set
            {
                var delta = value - m_Score;
                m_Score = value;
                GameEntry.Event.Fire(this,ScoreChangeEventArgs.Create(m_Score,delta));
            }
        }

        private float m_ElapsedTime;//这个持续变化不需要事件
        public float ElapsedTime
        {
            get
            {
                return m_ElapsedTime;
            }
            set { m_ElapsedTime = value; }
        }
        

        #endregion
        
        public Player Player
        {
            get;
            private set;
        }


        public bool GameWin
        {
            get;
            protected set;
        }
        public bool GameOver
        {
            get;
            protected set;
        }

      
        public LevelDisplayForm LevelDisplayForm { get; private set; }
        
        public SceneCam SceneCam { get; private set; }
        private PlayerInputActions playerInputActions;
        public virtual void Initialize()
        {
            
            //DataNode
            GameEntry.DataNode.SetData<VarObject>("GameBase",new VarObject(){Value = this});
            
            
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            
            GameEntry.Event.Subscribe(BattleUnitDieEventArgs.EventId,OnBattleUnitDie);
            

            #region MyRegion
            //绑定输入
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Esc.performed += OpenPauseForm;
            playerInputActions.Enable();
            
            #endregion

            
            
            #region 动态生成场景中实体

            bool spawnedPlayer=false;
            var placeholders = Object.FindObjectsOfType<EntityPlaceholder>();
            foreach (var placeholder in placeholders)
            {
                
                placeholder.SpawnEntity();
                placeholder.gameObject.SetActive(false);
                GameObject.Destroy(placeholder.gameObject,3);
                
                if (placeholder as PlayerPlaceholder!= null)
                    spawnedPlayer = true;
            }
            
            

            //如果没有生成玩家
            if (!spawnedPlayer)
            {
                Log.Error("没有生成玩家，系统自动生成玩家在0坐标，要让玩家生成在指定位置请拖拽玩家预制体到场景中并添加EntityInfo组件");
                GameEntry.Entity.ShowPlayer(new PlayerData(GameEntry.Entity.GenerateSerialId(),10000));
            }

            GameEntry.Entity.ShowSceneCam();
            #endregion

            GameOver = false;
            GameWin = false;

            ElapsedTime = 0;
            Score = 0;

            GameEntry.Base.StartCoroutine(InitDisplayUi());

            Debug.Log(this.GetType()+" loadCount:"+loadCount);
            loadCount++;
            
            //定时器例子
            GameEntry.Timer.AddOnceTimer(3,()=>{Debug.Log("3秒Timer定时器");});
            GameEntry.TimingWheel.AddTask(3000,(success)=>{Debug.Log("3秒TimingWheel定时器");});
        }
        
        
        protected virtual void OpenPauseForm(InputAction.CallbackContext callbackContext)
        {
            if(!GameOver && !GameWin)
                GameEntry.UI.OpenUIForm(103);
        }


        IEnumerator InitDisplayUi()
        {
            #region 生成主Ui

            GameEntry.UI.OpenUIForm(200);
            yield return null;

            #endregion
        }
        
        
        void OnOpenUIFormSuccess(object sender,GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs args = (OpenUIFormSuccessEventArgs)e;
            if (args.UIForm.Logic as LevelDisplayForm)
            {
                LevelDisplayForm= args.UIForm.Logic as LevelDisplayForm;
                

                if (LevelDisplayForm)
                {
                    var curProcedure = (GameEntry.Procedure.CurrentProcedure as ProcedureLevel);
                    if(curProcedure==null)
                        Log.Fatal("这里怎么成null了呢");
                    LevelDisplayForm.SetLevelTarget("LevelTarget."+curProcedure.GameLevel.ToString());
                }
            }
        }

        void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs args = (OpenUIFormSuccessEventArgs)e;
            Log.Fatal("生成Ui失败{0}",args.UIForm.UIFormAssetName);
        }

     
        
        

        /// <summary>
        /// 由Procedure的OnLeave调用
        /// </summary>
        public virtual void Shutdown()
        {
            if(LevelDisplayForm.Visible)
                LevelDisplayForm.Close(true);
            
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            
            GameEntry.Event.Unsubscribe(BattleUnitDieEventArgs.EventId,OnBattleUnitDie);
            
            playerInputActions.Player.Esc.performed -= OpenPauseForm;
            playerInputActions.Disable();
            
            
        }

        /// <summary>
        /// GameOver多半是玩家死亡，已经写在了基类中，子类主要进行游戏胜利判断。
        /// </summary>
        protected virtual void CheckGameOverOrWin()
        {
            if (Player != null && (!Player.Available || Player.chaState.dead))
            {
                GameOver = true;
                Log.Debug("GameOver!!");
                
            }
            
        }
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            ElapsedTime += elapseSeconds;
            if(GameOver || GameWin)
                return;
           
           
            CheckGameOverOrWin();
            
        }

        protected virtual void OnBattleUnitDie(object sender, GameEventArgs e)
        {
            BattleUnitDieEventArgs ne = (BattleUnitDieEventArgs)e;
            if (ne != null)
            {
                var dieScore = ne.battleUnit.GetBattleUnitData().DieScore;
                Score += dieScore +  (dieScore==0?0:UnityEngine.Random.Range(-10,10));//加随机数以创建排行榜
            }
        }
        
        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(Player))
            {
                Player = ne.Entity.Logic as Player;
                
            }

            if (ne.EntityLogicType == typeof(SceneCam) )
            {
                SceneCam = ne.Entity.Logic as SceneCam;
                var playerCamInfo = Player.GetComponent<PlayerCamFollowAndLookTransforms>();
                if (playerCamInfo)
                {
                    SceneCam.SetFollow(playerCamInfo.follow);
                    SceneCam.AddToTargetGroup(playerCamInfo.lookAt);
                }
                else
                {
                    SceneCam.SetFollow(Player.transform);
                    SceneCam.AddToTargetGroup(Player.transform);
                }
                
            }
        }

        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }
        
    }
}
