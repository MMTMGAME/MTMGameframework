using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitAnimatorEvents : MonoBehaviour
{
    private BattleUnit selfUnit;
    public void Init(BattleUnit selfUnit)
    {
        this.selfUnit = selfUnit;
    }
    //动画事件
    public void HitRight(float radius)
    {
        selfUnit.GetWeaponByIndex(0).Hit(radius);   
    }
    
    public void HitLeft(float radius)
    {
        selfUnit.GetWeaponByIndex(1).Hit(radius);   
    }

    public void Shoot(int index)
    {
        selfUnit.GetWeaponByIndex(index).Shoot(); 
    }


}
