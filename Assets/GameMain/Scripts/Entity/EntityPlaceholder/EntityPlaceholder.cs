using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public abstract class EntityPlaceholder : MonoBehaviour
{
    [SerializeField]
    protected int typeId;
    public abstract void SpawnEntity();
}
