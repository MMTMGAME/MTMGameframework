using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

[System.Serializable]
public class PlayerData : BattleUnitData
{
    public PlayerData(int entityId, int typeId) : base(entityId, typeId, CampType.Player)
    {
    }
}
