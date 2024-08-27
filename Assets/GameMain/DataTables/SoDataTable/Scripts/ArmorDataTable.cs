using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ArmorDataRow:SoDataRow
{
    [Header("附加路径")]
    public string pathKey;
    public int hpAdd;
    public float hpTimes;
    public int mpAdd;
    public float mpTimes;
    public int attackAdd;
    public float attackTimes;
    public int defenseAdd;
    public float defenseTimes;
    public int moveSpeedAdd;
    public float moveSpeedTimes;
    public int actionSpeedAdd;
    public float actionSpeedTimes;
    public string[] buffs;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/ArmorDataTable",fileName = "ArmorDataTable")]
public class ArmorDataTable : SoDataTable<ArmorDataRow>
{
    
}
