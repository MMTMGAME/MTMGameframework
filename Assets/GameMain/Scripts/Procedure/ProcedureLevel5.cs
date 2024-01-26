using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class ProcedureLevel5 : ProcedureLevel
{
   
    

    public override GameMode GameLevel
    {
        get
        {
            return GameMode.Level5;
        }
    }
    
    
    protected override void NextLevel()
    {
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level6"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 6);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }
    
}
