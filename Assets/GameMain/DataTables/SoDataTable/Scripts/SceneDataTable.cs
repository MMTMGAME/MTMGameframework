using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SceneDataRow:SoDataRow
{
    public SceneAsset sceneAsset;
    public int sceneBgmId;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/SceneDataTable",fileName = "SceneDataTable")]
public class SceneDataTable : SoDataTable<SceneDataRow>
{
    
}
