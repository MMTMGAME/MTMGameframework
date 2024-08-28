using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WeaponDataRow:SoDataRow
{
    public int attack;
    [Header("玩家使用武器的攻击间隔进行攻击，\n可通过某些buff或属性增加攻击速度，这个项目没写")]
    public float attackInterval;
    [Header("属性加减,Add为数值增加，Times为乘算")]
    public int hpAdd;
    public int hpTimes;
    public int mpAdd;
    public int mpTimes;
    public int attackAdd;
    public int attackTimes;
    public int defenseAdd;
    public int defenseTimes;
    public int moveSpeedAdd;
    public int moveSpeedTimes;
    public int actionSpeedAdd;
    public int actionSpeedTimes;
    [Header("填写武器自带技能和buff，\n此项目左键放skills[0]，右键skills[1]")]
    public string[] skills;
    public string[] buffs;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/WeaponDataTable",fileName = "WeaponDataTable")]
public class WeaponDataTable : SoDataTable<WeaponDataRow>
{
    
}
