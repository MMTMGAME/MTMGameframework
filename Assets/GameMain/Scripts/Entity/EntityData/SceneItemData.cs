using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameFramework.DataTable;
using GameMain;
using UnityEngine;

public class SceneItemData : TargetableObjectData
{
    public int attack;
    public int defense;
    public int deadEffectId;
    public int deadSoundId;

    public bool PickAble { get; set; }

    public SceneItemData(int entityId, int typeId) : base(entityId, typeId, CampType.Neutral)
    {
        IDataTable<DRSceneItem> drSceneItems = GameEntry.DataTable.GetDataTable<DRSceneItem>();
        DRSceneItem drSceneItem = drSceneItems.GetDataRow(TypeId);
        if (drSceneItem == null)
        {
            return;
        }

        HP = MaxHP = drSceneItem.MaxHp;
        attack = drSceneItem.Attack;
        defense = drSceneItem.Defense;
        deadEffectId = drSceneItem.DeadEffectId;
        deadSoundId = drSceneItem.DeadSoundId;
    }

    public override int MaxHP { get; }
}
