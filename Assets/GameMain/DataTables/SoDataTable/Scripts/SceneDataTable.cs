using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


[System.Serializable]
public class SceneDataRow:SoDataRow
{
    [FormerlySerializedAs("sceneAsset")] public string sceneName;
    public int sceneBgmId;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/SceneDataTable",fileName = "SceneDataTable")]
public class SceneDataTable : SoDataTable<SceneDataRow>
{
    
}
