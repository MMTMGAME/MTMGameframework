using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoadConfig : MonoBehaviour
{
    
    [FormerlySerializedAs("isBranch")] public bool isTurn;
    public Transform[] tails;

    [Header("原石生成")]
    public Transform[] primogems;

    [Header("障碍物位置")] public Transform[] obstacles;
}
