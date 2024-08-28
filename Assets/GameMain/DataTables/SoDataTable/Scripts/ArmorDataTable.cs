using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ArmorDataRow:SoDataRow
{
    [Header("附加路径")]
    public string pathKey;
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
    public string[] buffs;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/ArmorDataTable",fileName = "ArmorDataTable")]
public class ArmorDataTable : SoDataTable<ArmorDataRow>
{
    
}
