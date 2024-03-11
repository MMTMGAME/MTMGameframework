//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections;
using System.Reflection;
using DG.Tweening;
using GameFramework;
using GameFramework.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace GameMain
{
    
    public sealed class GameStartEventArgs : GameEventArgs
    {
        /// <summary>
        /// 显示实体成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(GameStartEventArgs).GetHashCode();

        public override void Clear()
        {
            //DONothing
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }
        
        public static GameStartEventArgs Create()
        {
            GameStartEventArgs gameStartEventArgs = ReferencePool.Acquire<GameStartEventArgs>();
            
            return gameStartEventArgs;
        }
    }

  
    
    public abstract class GameBase
    {
        /// <summary>
        /// 记录这个GameBase加载了几次，没啥用，只是用来提醒Gamebase这个示例会被多次加载，所以需要注意在初始化的时候重置相关变量
        /// </summary>
        public int loadCount = 0;
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

        //数据
        public float score
        {
            get;
            private set;
        }//分数
        //技能条
        public float skillPoint;
        private float requiredSkillPoint;
        public RoadGenerator roadGenerator;
      
        public LevelDisplayForm LevelDisplayForm { get; private set; }
        
        public SceneCam SceneCam { get; private set; }
        private PlayerInputActions playerInputActions;

        private float startTime;//记录游戏开始的时间
        public virtual void Initialize()
        {
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            
            
            startTime = Time.realtimeSinceStartup;
            
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
            // GameEntry.Entity.ShowEnemy(new EnemyData(GameEntry.Entity.GenerateSerialId(),20000)
            // {
            //     
            // });
            //天理直接使用placeHolder生成

           
            #endregion

            GameOver = false;
            GameWin = false;

            score = 0;
            skillPoint = 0;

            
            GameEntry.Base.StartCoroutine(InitDisplayUi());

            Debug.Log(this.GetType()+" loadCount:"+loadCount);
            loadCount++;

            roadGenerator = GameEntry.RoadGenerator;
            roadGenerator.StartGenerate();

            requiredSkillPoint = GameEntry.Config.GetFloat("Game.RequiredSkillPoint", 50);
            
            //触发游戏开始事件
            GameEntry.Event.Fire(this,GameStartEventArgs.Create());
        }
        
        
        protected virtual void OpenPauseForm(InputAction.CallbackContext callbackContext)
        {
            GameEntry.UI.OpenUIForm(103);
            GameEntry.Sound.PlayUISound(20007);
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
                    LevelDisplayForm.Init(this);
                    
                }
            }
        }

        void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs args = (OpenUIFormSuccessEventArgs)e;
            Log.Fatal("生成Ui失败{0}",args.UIForm.UIFormAssetName);
        }


        public void AddScore(float delta)
        {
            var prev = score;
            score += delta;
            if (score >= requiredSkillPoint && prev < requiredSkillPoint)
            {
                GameEntry.Sound.PlayUISound(20008);
            }//播放充能完毕音效
            skillPoint += delta;
        }

        private bool usingSKill = false;
        public void UseSkill()
        {
            if(usingSKill || skillPoint<requiredSkillPoint)
                return;
           
            skillPoint = 0;
            GameEntry.Base.StartCoroutine(StartSkillCoroutine());
        }

        IEnumerator StartSkillCoroutine()
        {
            usingSKill = true;
            SwitchSpeedAdjust(false);
            GameEntry.Sound.PlayUISound(20009);
            
            var skillDuration = GameEntry.Config.GetFloat("Game.SkillDuration", 7);
            SceneCam.SwitchSpeedline(true);
            // 启用技能时的设置
            playerMove.SwitchAutoRun(true);
            GameEntry.Base.StartCoroutine(ShowShieldFxRunCoroutine(skillDuration*2
            ));
    
            // 视野缓动变化到 110
            DOTween.To(() => SceneCam.cinemachine.m_Lens.FieldOfView, x => SceneCam.cinemachine.m_Lens.FieldOfView = x, 110f, skillDuration / 2);
            // 游戏速度缓动变化到 2
            DOTween.To(() => GameEntry.Base.GameSpeed, x => GameEntry.Base.GameSpeed = x, 2, skillDuration / 2);

            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("RoadObstacle"));
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"));

            yield return new WaitForSecondsRealtime(skillDuration);

            
           
            // 视野缓动变化回 75
            DOTween.To(() => SceneCam.cinemachine.m_Lens.FieldOfView, x => SceneCam.cinemachine.m_Lens.FieldOfView = x, 75f, skillDuration / 2);
            // 游戏速度缓动恢复正常
            DOTween.To(() => GameEntry.Base.GameSpeed, x => GameEntry.Base.GameSpeed = x, 1, skillDuration / 2).OnComplete(()=>
            {
                // 技能结束后的设置
                playerMove.SwitchAutoRun(false);
                
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("RoadObstacle"), false);
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), false);
                GameEntry.Base.ResetNormalGameSpeed();
                SwitchSpeedAdjust(true);
                usingSKill = false;
                
                SceneCam.SwitchSpeedline(false);
            });

            

           
        }
        
        IEnumerator ShowShieldFxRunCoroutine(float duration)
        {
           

            var id = GameEntry.Entity.GenerateSerialId();
            GameEntry.Entity.ShowEffect(new EffectData(id,70003,duration));
            yield return null;
            yield return null;

            var effectEntity = GameEntry.Entity.GetEntity(id);
            GameEntry.Entity.AttachEntity(effectEntity,Player.Entity);
            effectEntity.transform.localPosition = Vector3.zero;
        
        
            yield return new WaitForSeconds(duration);
           
        }
        

        /// <summary>
        /// 由Procedure的OnLeave调用
        /// </summary>
        public virtual void Shutdown()
        {
            if(LevelDisplayForm)
                LevelDisplayForm.Close(true);
            
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            
            playerInputActions.Player.Esc.performed -= OpenPauseForm;
            playerInputActions.Disable();
            
            
        }

        /// <summary>
        /// GameOver多半是玩家死亡，已经写在了基类中，子类主要进行游戏胜利判断。
        /// </summary>
        protected virtual void CheckGameOverOrWin()
        {
            if (Player != null && (!Player.Available || Player.IsDead || Player.CachedTransform.position.y<-50))
            {
                GameOver = true;
                Log.Debug("GameOver!!");
                
            }
            
        }

        private bool speedAdjustEnabled=true;

        public void SwitchSpeedAdjust(bool status)
        {
            speedAdjustEnabled = status;
        }
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if(GameOver || GameWin)
                return;
           
            CheckGameOverOrWin();


            if (speedAdjustEnabled)
            {
                AdjustSpeed();
            }
            
        }


        public PlayerMove playerMove;
        void AdjustSpeed()
        {
            
            if(playerMove==null)
                return;

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float duration = 450; // 5分钟
            if (elapsedTime <= duration) {
                float progress = elapsedTime / duration;
                GameEntry.Base.GameSpeed = 1 + (1.25f * Mathf.Log10(1 + 9 * progress));
            }
            
        }

        
        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(Player))
            {
                Player = ne.Entity.Logic as Player;
                playerMove = Player.GetComponent<PlayerMove>();
                GameEntry.Entity.ShowSceneCam();

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
