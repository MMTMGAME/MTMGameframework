using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class ProcedureLevel4 : ProcedureLevel1
{
    public override GameMode GameLevel
    {
        get
        {
            return GameMode.Level4;
        }
    }

    protected override void NextLevel()
    {
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level5"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 5);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }
}
