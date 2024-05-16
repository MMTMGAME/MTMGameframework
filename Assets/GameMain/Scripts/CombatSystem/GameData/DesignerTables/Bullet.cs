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
            ){hitSelf = false}},
            {"normal1", new BulletModel(
                "normal1", 50001, 
                "", new object[0],
                "CommonBulletHit", new object[]{1.0f,0.05f,71000}, 
                "CommonBulletRemoved", new object[]{70000}
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
                "CreateAoEOnHit", new object[]{new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, Quaternion.identity)},
                "CreateAoEOnRemoved", new object[]{
                    new AoeLauncher(DesingerTables.AoE.data["StayingBoom"], null, Vector3.zero, 0.1f, Quaternion.identity),  //反正碰撞也没用，不如就直接…………
                    new AoeLauncher(DesingerTables.AoE.data["BoomExplosive"], null, Vector3.zero, 1.5f, Quaternion.identity)                    
                }
            )}
        };
    }
}