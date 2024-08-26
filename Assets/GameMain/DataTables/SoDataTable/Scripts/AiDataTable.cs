using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class AiDataRow : SoDataRow
{
    public StateGraphAsset stateGraph;
    public float radius;
    public float attackDistance;
    public bool addAiMove;
    public bool addAiRotate;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/AiDataTable",fileName = "AiDataTable")]
public class AiDataTable : SoDataTable<AiDataRow>
{
  
}
