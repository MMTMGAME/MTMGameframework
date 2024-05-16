using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    
    ///<summary>
    ///AoeModel
    ///</summary>
    public class AoE{
        public static Dictionary<string, AoeModel> data = new Dictionary<string, AoeModel>(){
            {"BulletShield", new AoeModel(
                "BulletShield",60000 , new string[0], 0, true, 
                "", new object[0],  //create
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //chaEnter
                "", new object[0],  //chaLeave
                "BlockBullets", new object[]{false}, //bulletEnter
                "", new object[0]  //bulletLeave
            )},
            {"SpaceMonkeyBall", new AoeModel(
                "SpaceMonkeyBall", 80000, new string[0], 0, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "", new object[0],  //tick
                "AddBuffToEnterCha", new object[]{"Poison", 1,10f,true, true, false},  //chaEnter
                "", new object[0],  //chaLeave
                "SpaceMonkeyBallHit", new object[]{0.05f},  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"BlackHole", new AoeModel(
                "BlackHole", 60001, new string[0], 0.02f, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "BlackHole", new object[0],  //tick
                "", new object[0],  //chaEnter
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"BoomExplosive", new AoeModel( //炸弹爆炸
                "BoomExplosive", 0, new string[0], 0, false,
                "CreateSightEffect", new object[]{"Effect/Explosion_A"},
                "DoDamageOnRemoved", new object[]{new Damage(0, 20), 0.1f, true, false, true, "Effect/HitEffect_A", "Body"},    //10%攻击力加成
                "", new object[0],  //tick
                "", new object[0],  //chaEnter
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"StayingBoom", new AoeModel(   //炸弹掉在地上的样子
                "StayingBoom", 60001, new string[0], 0, false,
                "", new object[0],
                "CreateAoeOnRemoved", new object[]{"BoomExplosive", 1.5f, 0f},
                "", new object[0],  //tick
                "", new object[0],  //chaEnter
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )}
        };
    }
}