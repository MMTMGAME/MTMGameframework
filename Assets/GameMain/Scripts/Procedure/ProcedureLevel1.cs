using System.Collections;
using System.Collections.Generic;
using GameFramework.Entity;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameMain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;
using ProcedureBase = GameMain.ProcedureBase;
using ShowEntityFailureEventArgs = UnityGameFramework.Runtime.ShowEntityFailureEventArgs;
using ShowEntitySuccessEventArgs = UnityGameFramework.Runtime.ShowEntitySuccessEventArgs;

public class ProcedureLevel1 : ProcedureLevel
{
    public override GameMode GameLevel => GameMode.Level1;


    protected override void NextLevel()
    {
        base.NextLevel();
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level2"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 2);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }
}