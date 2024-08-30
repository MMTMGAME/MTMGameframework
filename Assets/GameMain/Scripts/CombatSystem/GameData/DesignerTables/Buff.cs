using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    ///<summary>
    ///buff的效果
    ///</summary>
    public class Buff{
        public static Dictionary<string, BuffModel> data = new Dictionary<string, BuffModel>(){
            { "AutoCheckReload", new BuffModel( "AutoCheckReload", "自动填装", new string[]{"Passive"}, 0, 1, 0,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "ReloadAmmo", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null
            )},
            { "TeleportBulletPassive", new BuffModel("TeleportBulletPassive", "传送弹技能被动效果", new string[]{"Passive"}, 0, 1, 0,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "FireTeleportBullet", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null
            )},
            { "TeleportTo", new BuffModel("TeleportTo", "直接把GameObject传送到某个世界坐标（非常危险）", new string[]{"Dangerous"}, 0, 1, 0,
                "",new object[0],
                "", new object[0],  //occur
                "TeleportCarrier", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.stun, null 
            )},
            { "ExplosionBarrel", new BuffModel("ExplosionBarrel", "爆炸的桶子用的", new string[]{"Passive"}, -1, 1, 5.0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "BarrelDurationLose", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "OnlyTakeOneDirectDamage", new object[0],  //hurt
                "", new object[0],  //kill
                "BarrelExplosed", new object[0],  //dead
                ChaControlState.stun, null  //桶子也是被昏迷的
            )},
            {
                "Poison",new BuffModel("Poison","毒",new string[]{"Poison"},0,1,1.0f,
                    "",new object[0],
                    "", new object[0],  //occur
                    "", new object[0],  //remove
                    "DoPercentDamageToCarrier", new object[3]{"CurrentHealth",1f,0.5f},  //tick
                    "", new object[0],  //cast
                    "", new object[0],  //hit
                    "", new object[0],  //hurt
                    "", new object[0],  //kill
                    "", new object[0],  //dead
                    ChaControlState.origin, null  
            )},
            {
                "ReduceOxygen",new BuffModel("ReduceOxygen","持续消耗氧气",new string[]{"Program"},0,1,1.0f,
                    "",new object[0],
                    "", new object[0],  //occur
                    "", new object[0],  //remove
                    "ReduceOxygen", new object[]{2f},  //tick
                    "", new object[0],  //cast
                    "", new object[0],  //hit
                    "", new object[0],  //hurt
                    "", new object[0],  //kill
                    "", new object[0],  //dead
                    ChaControlState.origin, null  
                )},
            { "Stun", new BuffModel("Stun", "眩晕", new string[]{"Control"}, 0, 1, 0f,
                "",new object[0],
                "DropAll", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.stun, null  
            )},
            { "KnockedOff", new BuffModel("KnockedOff", "被击飞", new string[]{"Control"}, 0, 1, 0f,
                "",new object[0],
                "DropAll", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.knockOff, null  
            )},
            { "RemoveKnockedOffBuffOnGround", new BuffModel("RemoveKnockedOffBuffOnGround", "自动处理击飞落地", new string[]{"Program"}, 0, 1, 0.2f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "RemoveKnockedOffBuffOnGround", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null  
            )},
           
            { "BeAttacked", new BuffModel("BeAttacked", "被攻击时无法移动", new string[]{"Program"}, 0, 1, 0,
                "",new object[0],
                "ResetVelocity", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.stun, null  
            )},
            
            { "LieDown", new BuffModel("LieDown", "倒下", new string[]{"Program"}, 0, 1, 0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.knockOff, null  
            )},
            
            { "ShowBattleUnitOnBeKilled", new BuffModel("ShowBattleUnitOnBeKilled", "死亡死生成新单位", new string[]{"Passive"}, 0, 1, 0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "ShowBattleUnitOnBeKilled", new object[]{20018},  //dead
                ChaControlState.origin, null  
            )},
            
            
           
            
            { "DamageOtherOnHurt", new BuffModel("DamageOtherOnHurt", "反伤", new string[]{"Passive"}, 0, 1, 0f,
                "",new object[0],
                "", new object[]{},  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "DamageOtherOnHurt", new object[]{},  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null  
            )},

            { "AgentEscape", new BuffModel("AgentEscape", "Agent逃跑", new string[]{""}, 0, 1, 0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, new []{new ChaProperty(500,0,0,0,0,0,0,0),ChaProperty.zero}  //桶子也是被昏迷的
            )},
            
            { "KnockOffOtherOnCollide", new BuffModel("KnockOffOtherOnCollide", "击飞碰到的角色", new string[]{"Passive"}, 0, 1, 0f,
                "KnockOffOtherOnCollide",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null  //桶子也是被昏迷的
            )},
            
            { "ExplosionOnCollide", new BuffModel("ExplosionOnCollide", "碰撞时爆炸", new string[]{"Passive"}, 0, 1, 0f,
                "ExplosionOnCollide",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null  //桶子也是被昏迷的
            )},
            { "ExplosionOnBeKilled", new BuffModel("ExplosionOnBeKilled", "死亡时爆炸", new string[]{"Passive"}, 0, 1, 0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "ExplosionOnBeKilled", new object[0],  //dead
                ChaControlState.origin, null  //桶子也是被昏迷的
            )},
            
            
            { "OverWeight", new BuffModel("OverWeight", "超重时判断是否有这个buff，如果超重则不能执行某些动作", new string[]{"Passive"}, 0, 1, 0f,
                "",new object[0],
                "", new object[0],  //occur
                "", new object[0],  //remove
                "", new object[0],  //tick
                "", new object[0],  //cast
                "", new object[0],  //hit
                "", new object[0],  //hurt
                "", new object[0],  //kill
                "", new object[0],  //dead
                ChaControlState.origin, null  //桶子也是被昏迷的
            )},
            
            
        };
    }
}