using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class UiSoundDataRow:SoDataRow
{
    public AudioClip audioClip;
    [Header("优先级（默认0，128最高，-128最低）")]
    public int priority;
    [Range(0,1)]
    public float volume=1;
   
}

[CreateAssetMenu(menuName = "GameMain/DataTable/UiSoundDataTable",fileName = "UiSoundDataTable")]
public class UiSoundDataTable : SoDataTable<UiSoundDataRow>
{
    
}
