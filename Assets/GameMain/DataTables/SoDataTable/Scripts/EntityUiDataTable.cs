using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EntityUiDataRow:SoDataRow
{
    public GameObject assetGameObject;
    [Header("单个实例？")]
    public bool isSingleton=false;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/EntityUiDataTable",fileName = "EntityUiDataTable")]
public class EntityUiDataTable : SoDataTable<EntityUiDataRow>
{
    
}
