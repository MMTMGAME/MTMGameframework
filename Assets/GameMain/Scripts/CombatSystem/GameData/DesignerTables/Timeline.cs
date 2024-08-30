using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesingerTables
{
    public class Timeline{
        public static Dictionary<string, TimelineModel> data = new Dictionary<string, TimelineModel>(){
            //发射普通子弹
            { "skill_fire", new TimelineModel("skill_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire" }),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle",71000,"",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["normal0"], null, Vector3.zero, Quaternion.identity, 1.0f, 10.0f,1f
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},
            
            { "skill_lightAttack", new TimelineModel("skill_lightAttack", new TimelineNode[]{
                new TimelineNode(0.00f, "LightAttack"),
                }, 0.01f, TimelineGoTo.Null)},
            
            { "skill_heavyAttack", new TimelineModel("skill_heavyAttack", new TimelineNode[]{
                new TimelineNode(0.00f, "HeavyAttack"),
            }, 0.01f, TimelineGoTo.Null)},
            
            //发射水波
            { "skill_shootSurge", new TimelineModel("skill_shootSurge", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim",new object[]
                {
                    UnitAnim.AnimOrderType.Trigger,"Fire" 
                }),
            }, 0.01f, TimelineGoTo.Null)},
            { "shootSurge", new TimelineModel("shootSurge", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.00f, "FireBullet",new object[]
                {
                    new BulletLauncher(
                        Bullet.data["surgeBullet"], null, Vector3.zero, Quaternion.Euler(0,90,0), 9.0f, 6.0f
                    ), "Muzzle",72000
                }),
                new TimelineNode(1.00f, "SetCasterControlState", new object[]{true, true, true}),
            }, 1.01f, TimelineGoTo.Null)},
            
           
            
            //玩家填装子弹
            { "skill_reload", new TimelineModel("skill_reload", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Reload"}),
                new TimelineNode(1.10f, "CasterAddAmmo", new object[]{99999}),
                new TimelineNode(1.12f, "SetCasterControlState", new object[]{true, true, true})
            }, 1.15f, TimelineGoTo.Null)},

            //发射氪漏氪回力标
            { "skill_cloakBoomerang", new TimelineModel("skill_cloakBoomerang", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Head","Effect/Heart","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["cloakBoomerang"], null, Vector3.zero, Quaternion.identity, 5.0f, 10.0f, 0,
                        false,DesignerScripts.Bullet.bulletTween["CloakBoomerangTween"],
                        DesignerScripts.Bullet.targettingFunc["BulletCasterSelf"],
                        true, new Dictionary<string, object>(){{"backTime", 1.0f}}
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射传送子弹
            { "skill_teleportBullet_fire", new TimelineModel("skill_teleportBullet_fire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["teleportBullet"], null, Vector3.zero, Quaternion.identity, 6.0f, 3.0f, 0,
                        false,DesignerScripts.Bullet.bulletTween["SlowlyFaster"]
                    ), "Muzzle"
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},
            //闪现过去吃掉传送子弹(直接交给buff去办)
            { "skill_teleportBullet_tele", new TimelineModel("skill_teleportBullet_tele", new TimelineNode[]{
                new TimelineNode(0.00f, "AddBuffToCaster", new object[]{
                    new AddBuffInfo(DesingerTables.Buff.data["TeleportTo"], null, null, 1, 0.0f, true, false)
                })
            }, 0.10f, TimelineGoTo.Null)},

            //发射跟踪子弹
            { "skill_followfire", new TimelineModel("skill_followfire", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle","Effect/MuzzleFlash","",false}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["normal0"], null, Vector3.zero, Quaternion.identity, 3.0f, 100.0f, 0,  
                        false,DesignerScripts.Bullet.bulletTween["FollowingTarget"], 
                            DesignerScripts.Bullet.targettingFunc["GetNearestEnemy"], false
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //丢手雷
            { "skill_grenade", new TimelineModel("skill_grenade", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["boomball"], null, Vector3.zero, Quaternion.identity, 3.0f, 2.0f, 0,  
                        false,DesignerScripts.Bullet.bulletTween["BoomBallRolling"]
                        ), "Muzzle"
                    }
                ),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //召唤炸药桶
            { "skill_exploseBarrel", new TimelineModel("skill_exploseBarrel", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "SummonCharacter", new object[]{
                    "Barrel", new ChaProperty(0, 5), 0f, "",
                    new string[]{"Barrel"},
                    new AddBuffInfo[]{
                        new AddBuffInfo(DesingerTables.Buff.data["ExplosionBarrel"], null, null, 1, 10, true, true)
                    }
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //发射小猴球
            { "skill_spaceMonkeyBall", new TimelineModel("skill_spaceMonkeyBall", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{true, true, false}),
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Fire"}),
                new TimelineNode(0.10f, "PlaySightEffectOnCaster", new object[]{"Muzzle",71000,"",false}),
                new TimelineNode(0.10f, "CreateAoE", new object[]{
                    new AoeLauncher(
                        AoE.data["SpaceMonkeyBall"], null, Vector3.zero, 2.5f,  Quaternion.identity,1,
                        DesignerScripts.AoE.aoeTweenFunc["SpaceMonkeyBallRolling"], new object[]{Vector3.forward * 0.1f} //小猴球原始滚动速度0.1米/秒
                    ), true
                }),
                new TimelineNode(0.50f, "SetCasterControlState", new object[]{true, true, true})
            }, 0.50f, TimelineGoTo.Null)},

            //角色向移动方向打滚的技能效果
            { "skill_roll", new TimelineModel("skill_roll", new TimelineNode[]{
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Roll"}),
                new TimelineNode(0.00f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/Fire_B","fire_following",true}),
                new TimelineNode(0.01f, "SetCasterControlState", new object[]{false, false, false}),
                new TimelineNode(0.10f, "CasterImmune", new object[]{0.70f}),
                new TimelineNode(0.20f, "CasterForceMove", new object[]{2.0f, 0.5f, 0.00f, true, false}),
                new TimelineNode(0.80f, "StopSightEffectOnCaster", new object[]{"Body", "fire_following"}),
                new TimelineNode(0.80f, "PlaySightEffectOnCaster", new object[]{"Body","Effect/ShockWave","shockWave",false}),
                new TimelineNode(0.80f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.90f, TimelineGoTo.Null) },


            #region 哥布林技能

            { "skill_goblinRAttack", new TimelineModel("skill_goblinRAttack", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}) ,
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"AttackR"}),
                
                new TimelineNode(0.1f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.2f, TimelineGoTo.Null) },
            
            { "skill_goblinSkill0", new TimelineModel("skill_goblinSkill0", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}) ,
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Skill0"}),
                
                new TimelineNode(0.1f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.2f, TimelineGoTo.Null) },

            #endregion
            
            
            #region 哥布林射手技能

            { "skill_goblinShooterShoot", new TimelineModel("skill_goblinShooterShoot", new TimelineNode[]{
                new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}) ,
                new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Shoot"}),
                new TimelineNode(0.10f, "FireBullet", new object[]{
                    new BulletLauncher(
                        Bullet.data["normal0"], null, Vector3.zero, Quaternion.identity, 6.0f, 10.0f,1f
                    ), "MuzzlePoint" //MuzzlePoint应该放在武器上而不是角色身上，放在武器上可以实时根据武器的muzzlePoint发射和播放特效
                }),
                new TimelineNode(0.1f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
            }, 0.2f, TimelineGoTo.Null) },
            
       
            
        { "skill_goblinShooterSkill0", new TimelineModel("skill_skill_goblinShooterSkill0", new TimelineNode[]{
            new TimelineNode(0.00f, "SetCasterControlState", new object[]{false, false, false}) ,
            new TimelineNode(0.00f, "CasterPlayAnim", new object[]{UnitAnim.AnimOrderType.Trigger,"Skill0"}),
            new TimelineNode(0.10f, "FireBullet", new object[]{
                new BulletLauncher(
                    Bullet.data["normal0"], null, Vector3.zero, Quaternion.identity, 6f, 10.0f,1f
                ), "MuzzlePoint" //MuzzlePoint应该放在武器上而不是角色身上，放在武器上可以实时根据武器的muzzlePoint发射和播放特效
            }),
            new TimelineNode(0.10f, "FireBullet", new object[]{
                new BulletLauncher(
                    Bullet.data["normal0"], null, Vector3.zero, Quaternion.Euler(0,-30,0), 6f, 10.0f,1f
                ), "MuzzlePoint" //MuzzlePoint应该放在武器上而不是角色身上，放在武器上可以实时根据武器的muzzlePoint发射和播放特效
            }),
            new TimelineNode(0.10f, "FireBullet", new object[]{
                new BulletLauncher(
                    Bullet.data["normal0"], null, Vector3.zero, Quaternion.Euler(0,30,0), 6f, 10.0f,1f
                ), "MuzzlePoint" //MuzzlePoint应该放在武器上而不是角色身上，放在武器上可以实时根据武器的muzzlePoint发射和播放特效
            }),
            new TimelineNode(0.1f, "SetCasterControlState", new object[]{true, true, true})    //早0.1秒恢复操作状态手感好点
        }, 0.2f, TimelineGoTo.Null) }

        #endregion

        };

        
       
    }    
}