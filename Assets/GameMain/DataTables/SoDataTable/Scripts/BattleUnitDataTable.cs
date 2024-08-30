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

    [Header("死亡后多久销毁(ms)\n用于播放死亡动画")]
    public long hideTime;

    [Header("区分单位特征")]
    public string[] tags;
    public int GetArmorIdAt(int index)
    {
        switch (index)
        {
            case 0:
                return armorId0;
            case 1:
                return armorId1;
            case 2:
                return armorId2;
            case 3:
                return 0;
            
            default:
                return armorId0;
        }

        return armorId0;
    }
    
    public int GetWeaponIdAt(int index)
    {
        switch (index)
        {
            case 0:
                return weaponId;
           
            case 1:
                return 0;
            default:
                return weaponId;
        }

        return weaponId;
    }
}

[CreateAssetMenu(menuName = "GameMain/DataTable/BattleUnitDataTable",fileName = "BattleUnitDataTable")]
public class BattleUnitDataTable : SoDataTable<BattleUnitDataRow>
{
    
}
