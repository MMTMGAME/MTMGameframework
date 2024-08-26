using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MusicDataRow:SoDataRow
{
    public AudioClip audioClip;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/MusicDataTable",fileName = "MusicDataTable")]
public class MusicDataTable : SoDataTable<MusicDataRow>
{
    
}
