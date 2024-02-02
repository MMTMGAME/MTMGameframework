using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public abstract class WeaponAttack : MonoBehaviour
{
    protected Weapon Weapon
    {
        get;
        set;
    }
    public virtual void Init(Weapon weapon)
    {
        Weapon = weapon;
    }
    public abstract void Attack();
}
