using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public sealed class PlayerEnterBarrageRoadEvtArgs : GameEventArgs
{
    /// <summary>
    /// 显示实体成功事件编号。
    /// </summary>
    public static readonly int EventId = typeof(PlayerEnterBarrageRoadEvtArgs).GetHashCode();

    public override void Clear()
    {
        //DONothing
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }
        
    public static PlayerEnterBarrageRoadEvtArgs Create()
    {
        PlayerEnterBarrageRoadEvtArgs playerEnterBarrageRoadEvtArgs = ReferencePool.Acquire<PlayerEnterBarrageRoadEvtArgs>();
            
        return playerEnterBarrageRoadEvtArgs;
    }
}

public sealed class PlayerExitBarrageRoadEvtArgs : GameEventArgs
{
    /// <summary>
    /// 显示实体成功事件编号。
    /// </summary>
    public static readonly int EventId = typeof(PlayerExitBarrageRoadEvtArgs).GetHashCode();

    public override void Clear()
    {
        //DONothing
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }
        
    public static PlayerExitBarrageRoadEvtArgs Create()
    {
        PlayerExitBarrageRoadEvtArgs playerExitBarrageRoadEvtArgs = ReferencePool.Acquire<PlayerExitBarrageRoadEvtArgs>();
            
        return playerExitBarrageRoadEvtArgs;
    }
}

public class BarrageRoadEventTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEntry.Event.Fire(this,PlayerEnterBarrageRoadEvtArgs.Create());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEntry.Event.Fire(this,PlayerExitBarrageRoadEvtArgs.Create());
        }
    }
}
