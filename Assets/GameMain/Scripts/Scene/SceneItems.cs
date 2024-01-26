using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;



[Serializable]
public struct SceneItemConfig
{
    [Header("注释")]
    public string comment;
    [FormerlySerializedAs("target")] public Transform targetTransform;
    public int typeId;
    public bool pickAble;


}
public class SceneItems : MonoBehaviour
{
    public List<SceneItemConfig> toSpawn = new List<SceneItemConfig>();

}
