//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections;
using System.Reflection;
using GameFramework.Event;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace GameMain
{
    public abstract class GameBase
    {

        public int testCount = 0;
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
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            
            GameEntry.Base.StartCoroutine(SpawnPlayer());

            #region MyRegion
            //绑定输入
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Esc.performed += OpenPauseForm;
            playerInputActions.Enable();
            
            #endregion

            
            
            #region 动态生成道具

            var sceneItems = Object.FindObjectOfType<SceneItems>();

            foreach (var config in sceneItems.toSpawn)
            {
                config.targetTransform.gameObject.SetActive(false);

                // 根据表格获取对应物体所需Entity类
                var sceneItemTable = GameEntry.DataTable.GetDataTable<DRSceneItem>();
                var row = sceneItemTable.GetDataRow(config.typeId);
                var entityLogicTypeName = row.EntityLogic;
                Type entityLogicType = Type.GetType(entityLogicTypeName);

                if (entityLogicType != null && typeof(SceneItem).IsAssignableFrom(entityLogicType))
                {
                    MethodInfo methodInfo = typeof(EntityExtension).GetMethod(nameof(EntityExtension.ShowSceneItem));
                    MethodInfo genericMethod = methodInfo.MakeGenericMethod(entityLogicType);

                    SceneItemData data = new SceneItemData(GameEntry.Entity.GenerateSerialId(), config.typeId)
                    {
                        Position = config.targetTransform.position,
                        Rotation = config.targetTransform.rotation,
                        PickAble=config.pickAble,
                    };

                    genericMethod.Invoke(GameEntry.Entity, new object[] { GameEntry.Entity, data });

                }
                else
                {
                    Debug.LogError("Invalid entity logic type: " + entityLogicTypeName);
                }
            }

            #endregion

            GameOver = false;
            GameWin = false;

            GameEntry.Base.StartCoroutine(InitDisplayUi());

            Debug.Log("TestCount:"+testCount);
            testCount++;
        }
        
        
        protected virtual void OpenPauseForm(InputAction.CallbackContext callbackContext)
        {
            GameEntry.UI.OpenUIForm(103);
        }

        IEnumerator InitDisplayUi()
        {
            #region 生成主Ui

            int? serialId = GameEntry.UI.OpenUIForm(200);
            if (serialId != null)
            {

                yield return new WaitForSeconds(0.1f);
                LevelDisplayForm= (LevelDisplayForm)GameEntry.UI.GetUIForm(200,"Display");

                if (LevelDisplayForm)
                {
                    var curProcedure = (GameEntry.Procedure.CurrentProcedure as ProcedureLevel);
                    if(curProcedure==null)
                        Debug.LogError("这里怎么成null了呢");
                    LevelDisplayForm.SetLevelTarget("LevelTarget."+curProcedure.GameLevel.ToString());
                }
              
            }

            #endregion
        }

        IEnumerator SpawnPlayer()
        {
            //yield return new WaitForSeconds(2f);
            #region 玩家生成和相机配置,相机配置在生成实体回调中执行

            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer)
            {
                Object.Destroy(existingPlayer.gameObject); //销毁场景中的player
            }

            yield return new WaitForSeconds(0.1f);
            //生成新player
            
          
            
            var playerTrans = GameObject.FindGameObjectWithTag("PlayerTrans");
            var spineTrans = playerTrans.transform.GetChild(0);
            GameEntry.Entity.ShowPlayer(new PlayerData(GameEntry.Entity.GenerateSerialId(), 10000)
            {
                Position = playerTrans.transform.position,
                Rotation = playerTrans.transform.rotation,
                SpinePosition = spineTrans.transform.position,
                SpineRotation = spineTrans.transform.rotation,
                
            });

            
            
            #endregion
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
            
            playerInputActions.Player.Esc.performed -= OpenPauseForm;
            playerInputActions.Disable();
            
            GameOver = false;
        }

        /// <summary>
        /// GameOver多半是玩家死亡，已经写在了基类中，子类主要进行游戏胜利判断。
        /// </summary>
        protected virtual void CheckGameOverOrWin()
        {
            if (Player != null && Player.IsDead)
            {
                GameOver = true;
                Debug.Log("GameOver!!");
                
            }
            
        }
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if(GameOver || GameWin)
                return;
           
            CheckGameOverOrWin();
        }

        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            if (ne.EntityLogicType == typeof(Player))
            {

                
                Player = ne.Entity.Logic as Player;
                SceneCam = Object.FindObjectOfType<SceneCam>();
                var playerCamInfo = Player.GetComponent<PlayerCamFollowAndLookTransforms>();
                SceneCam.SetFollow(playerCamInfo.follow);
                SceneCam.AddToTargetGroup(playerCamInfo.lookAt);
            }
        }

        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }
        
    }
}
