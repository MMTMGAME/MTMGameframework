using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class RangeWeaponAttack : WeaponAttack
{
    public Transform shootPoint;

    public override void Init(Weapon weapon)
    {
        base.Init(weapon);
        shootPoint = transform.Find(weapon.m_WeaponData.shootPointPath);
        if (shootPoint == null)
            shootPoint = transform;
    }

    public override void HandleAnimeEvent(string eventName)
    {
        // if (eventName == "Shoot")
        // {
        //     var m_WeaponData = Weapon.m_WeaponData;
        //     GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId, m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed,5)
        //     {
        //         Position = shootPoint.position,
        //         Rotation = shootPoint.rotation,
        //     });
        //     GameEntry.Sound.PlaySound(m_WeaponData.ShootSoundId,transform.position);
        // }
    }

   
}
