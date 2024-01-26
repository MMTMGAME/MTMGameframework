using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class PlayerData : TargetableObjectData
{


    public Vector3 SpinePosition
    {
        get;
        set;
    }

    public Quaternion SpineRotation
    {
        get;
        set;
    }
    
    
    
    public PlayerData(int entityId, int typeId) : base(entityId, typeId, CampType.Player)
    {
        MaxHP = 10;
        HP = MaxHP;
    }

    public override int MaxHP { get; }
}
