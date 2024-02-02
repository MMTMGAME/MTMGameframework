using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class BattleUnitPlaceholder : EntityPlaceholder
{

    public CampType campType;
    public override void SpawnEntity()
    {
        //阵营选择需要额外的代码控制，这里只是举例就不写了
        GameEntry.Entity.ShowBattleUnit(new BattleUnitData(GameEntry.Entity.GenerateSerialId(),typeId,campType)
        {
            Position = transform.position,
            Rotation = transform.rotation
        });
    }
}
