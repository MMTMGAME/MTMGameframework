using System.Collections;
using System.Collections.Generic;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

/// <summary>
/// 第三关胜利失败条件和第二关条件一模一样
/// </summary>
public class ProcedureLevel3 : ProcedureLevel
{
    public override GameMode GameLevel
    {
        get
        {
            return GameMode.Level3;
        }
        
    }
    
   

    protected override void NextLevel()
    {
        procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Level4"));
        procedureOwner.SetData<VarInt32>("TargetLevel", 4);
        ChangeState<ProcedureChangeScene>(procedureOwner);
    }
   
}
