using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class BowWeaponAttack : RangeWeaponAttack
{
    

    public override void HandleAnimeEvent(string eventName, float radius)
    {
        base.HandleAnimeEvent(eventName,radius);
        
        if (eventName == "Skill0")
        {
            var m_WeaponData = Weapon.m_WeaponData;
            for (int i = -4; i < 5; i++)
            {
                GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId, m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed,5)
                {
                    Position = shootPoint.TransformPoint(i*0.6f,0,0),
                    Rotation = shootPoint.rotation,
                });
            }
            
            GameEntry.Sound.PlaySound(m_WeaponData.ShootSoundId,transform.position);
        }
    }
    
}
