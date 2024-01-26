using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class ProcedureLevel6 : ProcedureLevel
{
  

    public override GameMode GameLevel
    {
        get { return GameMode.Level6; }
    }
    
    protected override void NextLevel()
    {
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level7"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 7);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }
}
