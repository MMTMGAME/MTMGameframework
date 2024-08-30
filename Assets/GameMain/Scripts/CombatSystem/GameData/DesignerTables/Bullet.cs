using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///BulletModel
    ///</summary>
    public class Bullet{
        public static Dictionary<string, BulletModel> data = new Dictionary<string, BulletModel>(){
            {"normal0", new BulletModel(
                "normal0", 50000, 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,70000,"Body"},
                "CommonBulletRemoved", new object[]{71000}
            )},
            {"normal1", new BulletModel(
                "normal1", 50001, 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,71000}, 
                "CommonBulletRemoved", new object[]{70000}
                
            )},
            {"surgeBullet", new BulletModel(
                "surgeBullet", 50002, 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,71002}, //伤害倍率，暴击率，特效Id，特效位置，音效Id,部分音效是跟随特效的，所以不需要Id
                "CommonBulletRemoved", new object[]{0},removeOnObstacle:false
            )},
            {"harpoonBullet", new BulletModel(
                "harpoonBullet", 50003, 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,70001,"Body",30005}, 
                "CommonBulletRemoved", new object[]{0}
            )},
            {"bubbleBullet", new BulletModel(
                "bubbleBullet", 50006, 
                "", new object[0],
                "BubbleBulletHit", new object[]{}, 
                "CommonBulletRemoved", new object[]{70008}
            )},
            {"torpedoBullet", new BulletModel(
                "torpedoBullet", 50004, 
                "", new object[0],
                "ExplosionOnHit", new object[]{3f}, 
                "ExplosionOnRemoved", new object[]{3f},
                useGravity:true
            )},
            {"mineBullet", new BulletModel(
                "torpedoBullet", 50005, 
                "", new object[0],
                "ExplosionOnHit", new object[]{3f,0.05f}, 
                "ExplosionOnRemoved", new object[]{},
                useGravity:true
            )},
            {"cloakBoomerang", new BulletModel(
                "cloakBoomerang", 50000, 
                "", new object[0],
                "CloakBoomerangHit", new object[]{1.0f,0.05f,71000}, 
                "", new object[0],
                MoveType.fly, false,  99999, 0.5f, true, true)
            },
            {"teleportBullet", new BulletModel(
                "teleportBullet", 50000,
                "RecordBullet", new object[0],
                "CommonBulletHit", new object[]{0.6f, 0.0f, 71000},
                "CommonBulletRemoved", new object[]{"70000"}, 
                MoveType.fly, true
            )},
            {"boomball", new BulletModel(
                "boomball", 50000,
                "SetBombBouncing", new object[0],
                "CreateAoEOnHit", new object[]{new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, Quaternion.identity,1)},
                "CreateAoEOnRemoved", new object[]{
                    new AoeLauncher(DesingerTables.AoE.data["StayingBoom"], null, Vector3.zero, 0.1f, Quaternion.identity),  //反正碰撞也没用，不如就直接…………
                    new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, Quaternion.identity)                    
                }
            )}
        };
    }
}