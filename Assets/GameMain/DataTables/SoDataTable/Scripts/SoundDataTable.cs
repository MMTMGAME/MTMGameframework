using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SoundDataRow:SoDataRow
{
    public AudioClip audioClip;
    [Header("优先级（默认0，128最高，-128最低）")]
    public int priority;
    [Header("是否循环")]
    public bool loop;
    [Range(0,1)]
    public float volume=1;
    [Header("空间混合")]
    [Range(0,1)]
    public float spatialBlend=1;

    [Header("最大距离")] public float maxDistance=100;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/SoundDataTable",fileName = "SoundDataTable")]
public class SoundDataTable : SoDataTable<SoundDataRow>
{
    
}
