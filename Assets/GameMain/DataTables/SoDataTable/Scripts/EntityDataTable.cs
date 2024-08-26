using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EntityDataRow:SoDataRow
{
    public GameObject assetGameObject;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/EntityDataTable",fileName = "EntityDataTable")]
public class EntityDataTable : SoDataTable<EntityDataRow>
{
    
}
