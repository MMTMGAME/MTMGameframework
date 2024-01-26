using System.Collections;
using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class ProcedureLevel2 : ProcedureLevel
{
    public override GameMode GameLevel
    {
        get
        {
            return GameMode.Level2;
        }
    }
   

    

    protected override void NextLevel()
    {
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level3"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 3);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }

}
