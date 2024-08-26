using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class UiFormDataRow:SoDataRow
{
    public GameObject asset;
    public string uiGroupName;
    public bool allowMultiInstance;
    [Header("是否暂停被其覆盖的界面")]
    public bool pauseCoveredUIForm;
}

[CreateAssetMenu(menuName = "GameMain/DataTable/UiFormDataTable",fileName = "UiFormDataTable")]
public class UiFormDataTable : SoDataTable<UiFormDataRow>
{
    
}
