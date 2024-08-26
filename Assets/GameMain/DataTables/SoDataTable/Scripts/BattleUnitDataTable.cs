using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BattleUnitDataRow:SoDataRow
{
    [Header("简化为只有一个武器")]
    public int weaponId;
    [Header("武器目标位置")]
    public string weaponPathKey;
    public int armorId0;
    public int armorId1;
    public int armorId2;
    public int deadEffectId;
    public int deadSoundId;
    [Header("死亡分数，计分游戏中常用")]
    public int dieScore;

    public int baseHp;
    public int baseMp;
    public int baseAttack;
    public int baseDefense;
    public int baseMoveSpeed;
    public int baseActionSpeed;
    [Header("单位自带技能和buff，区别于武器的技能和buff")]
    public string[] skills;
    public string[] buffs;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/BattleUnitDataTable",fileName = "BattleUnitDataTable")]
public class BattleUnitDataTable : SoDataTable<BattleUnitDataRow>
{
    
}
