using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;

public class BarrageWeaponAttack : WeaponAttack
{
    public Transform shootPoint;

    private WeaponData m_WeaponData;
    public override void Init(Weapon weapon)
    {
        base.Init(weapon);
        shootPoint = transform.FindDeep(weapon.m_WeaponData.shootPointPath);
        if (shootPoint == null)
            shootPoint = transform;
       
    }


    private Coroutine coroutine;
    public override void Attack()
    {
        m_WeaponData = Weapon.m_WeaponData;
        if (coroutine == null )
        {
            int rand = UnityEngine.Random.Range(0, 3);
            if (rand == 0)
                coroutine=StartCoroutine(AttackSimple());
            if (rand == 1)
                coroutine=StartCoroutine(AttackArc());
            if (rand == 2)
                coroutine=StartCoroutine(AttackRandom());
        }
        
    }

    IEnumerator AttackSimple()
    {
        Log.Info("开始简单弹幕");
        while (true)
        {
            
            GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId,
                m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed, 8)
            {
                Position = shootPoint.position,
                Rotation = shootPoint.rotation,
            });
            GameEntry.Sound.PlaySound(m_WeaponData.BulletSoundId);
            
            yield return new WaitForSeconds(0.5f);
            
        }
    }

    IEnumerator AttackArc()
    {
        Log.Info("开始扇形弹幕");
        while(true){
            
            for (int i = -6; i < 6; i++)
            {
                GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId,
                    m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed, 8)
                {
                    Position = shootPoint.position,
                    Rotation = Quaternion.Euler(0, i * 10, 0) * shootPoint.rotation,
                });
               
            }
            GameEntry.Sound.PlaySound(m_WeaponData.BulletSoundId);
            yield return new WaitForSeconds(1);
        }
        
    }

    IEnumerator AttackRandom()
    {
        Log.Info("开始随机弹幕");
        while(true){
            GameEntry.Entity.ShowBullet(new BulletData(GameEntry.Entity.GenerateSerialId(), m_WeaponData.BulletId,
                m_WeaponData.OwnerId, m_WeaponData.OwnerCamp, m_WeaponData.Attack, m_WeaponData.BulletSpeed, 8)
            {
                Position = shootPoint.position,
                Rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-22.5f,23f), 0) * shootPoint.rotation,
            });

            if (UnityEngine.Random.Range(0, 5) <2)
            {
                GameEntry.Sound.PlaySound(m_WeaponData.BulletSoundId);
            }
            
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void StopAttack()
    {
        StopAllCoroutines();
        coroutine = null;
    }
}
