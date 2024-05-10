using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimEventListener : MonoBehaviour
{
    private BattleUnit battleUnit;

    public void Init(BattleUnit battleUnit)
    {
        this.battleUnit = battleUnit;
    }
    public void TriggerAnimationEvent(string actionInfoStr)
    {
        var arr = actionInfoStr.Split(",");
        
        var weapon = battleUnit.GetWeaponByIndex(arr.Length>1?int.Parse(arr[1]):0);
        if (weapon != null)
        {
            weapon.HandleAnimEvent(arr[0].ToString());
        }
    }
}
