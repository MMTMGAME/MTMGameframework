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
            )},
            {"AttackAoe", new AoeModel(
                "AttackAoe", 80000, new string[0], 0, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "", new object[0],  //tick
                "DoDamageToEnterCha", new object[]{new Damage(1),1f,true,false,true,"71000|71001","30000|30001|30002|30003|30004"},  //chaEnter
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            
            {"KnockOffAoe", new AoeModel(
                "KnockOffAoe", 80000, new string[0], 0, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "", new object[0],  //tick
                "KnockOffEnterCha", new object[]{20,75f,false,true,true,false},  //击飞效果
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            
            {"BubbleAoe", new AoeModel(
                "BubbleAoe", 80004, new string[0], 0.05f, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "Vortex", new object[]{2f,0.15f,false},  //tick ，vortex填写ontick参数和Damage参数执行效果不一样
                "", new object[0],  //击飞效果
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"WhirlWindAoe", new AoeModel(
                "WhirlWindAoe", 80002, new string[0], 0.2f, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "Vortex", new object[]{1f,1f,true,false},  //tick
                "", new object[]{new Damage(1,0,0),0.1f,true,false,true,"70001|70002|70003","30005|30006|30007","Body",false},  //击飞效果
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"VortexAoe", new AoeModel(
                "VortexAoe", 80003, new string[0], 0.2f, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "Vortex", new object[0],  //tick
                "", new object[]{new Damage(1,0,0),1f,true,false,true,"70001|70002|70003","30005|30006|30007","Body",false},  //砸地击飞效果，把方向改为负的就可以了
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
            {"VortexAoeOnlyTraction", new AoeModel(//摇摇马的外层，不造成伤害，只牵引
                "VortexAoe", 80000, new string[0], 0.2f, true,
                "", new object[0],  //create
                "", new object[0],  //remove
                "Vortex", new object[]{0.5f,1f,false,true},  //tick
                "", new object[]{new Damage(1,0,0),1f,true,false,true,"70001|70002|70003","30005|30006|30007","Body",false},  //砸地击飞效果，把方向改为负的就可以了
                "", new object[0],  //chaLeave
                "", new object[0],  //bulletEnter
                "", new object[0]   //bulletLeave
            )},
        };
    }
}