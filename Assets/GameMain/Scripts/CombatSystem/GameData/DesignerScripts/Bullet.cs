using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///子弹的效果
    ///</summary>
    public class Bullet{
        public static Dictionary<string, BulletOnCreate> onCreateFunc = new Dictionary<string, BulletOnCreate>(){
            {"RecordBullet", RecordBullet},
            {"SetBombBouncing", SetBombBouncing}
        };
        public static Dictionary<string, BulletOnHit> onHitFunc = new Dictionary<string, BulletOnHit>(){
            {"CommonBulletHit", CommonBulletHit},
            {"CreateAoEOnHit", CreateAoEOnHit},
            {"CloakBoomerangHit", CloakBoomerangHit},
            {"BubbleBulletHit", BubbleBulletHit},
            {"ExplosionOnHit", ExplosionOnHit}
        };
        public static Dictionary<string, BulletOnRemoved> onRemovedFunc = new Dictionary<string, BulletOnRemoved>(){
            {"CommonBulletRemoved", CommonBulletRemoved},
            {"CreateAoEOnRemoved", CreateAoEOnRemoved},
            {"ExplosionOnRemoved", ExplosionOnRemoved},
        };
        public static Dictionary<string, BulletTween> bulletTween = new Dictionary<string, BulletTween>(){
            {"FollowingTarget", FollowingTarget},
            {"CloakBoomerangTween", CloakBoomerangTween},
            {"SlowlyFaster", SlowlyFaster},
            {"BoomBallRolling", BoomBallRolling}
        };
        public static Dictionary<string, BulletTargettingFunction> targettingFunc = new Dictionary<string, BulletTargettingFunction>(){
            {"GetNearestEnemy", GetNearestEnemy},
            {"BulletCasterSelf", BulletCasterSelf}
        };

        ///<summary>
        ///onHit
        ///普通子弹命中效果，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效Id
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CommonBulletHit(GameObject bullet, GameObject target){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParam = bulletState.model.onHitParams;
            float damageTimes = onHitParam.Length > 0 ? (float)onHitParam[0] : 1.00f;
            float critRate = onHitParam.Length > 1 ? (float) onHitParam[1] : 0.00f;
            int sightEffectId = onHitParam.Length > 2 ? (int) onHitParam[2] : 0;
            string bpName = onHitParam.Length > 3 ? (string) onHitParam[3] : "Body";
            int audioId= onHitParam.Length > 4 ? (int) onHitParam[4] :0;
            if (sightEffectId != 0){
                UnitBindManager ubm = target.GetComponent<UnitBindManager>();
                if (ubm){
                    Debug.Log($"播放受击特效，部位{bpName}");
                    ubm.AddBindEntity(bpName, sightEffectId, "", false);
                }
            }

            if (audioId != 0)
            {
                GameEntry.Sound.PlaySound(audioId);
            }
            
            Debug.Log($"CreateDamage");
            GameEntry.Combat.CreateDamage(
                bulletState.caster, 
                target,
                new Damage(Mathf.CeilToInt(damageTimes * bulletState.propWhileCast.attack)), 
                bullet.transform.eulerAngles.y,
                critRate,
                new DamageInfoTag[]{DamageInfoTag.directDamage, }
            );
        }

        private static void BubbleBulletHit(GameObject bullet, GameObject target)
        {
            CombatUtility.CreateAoe("BubbleAoe",bullet.GetComponent<BulletState>().caster,bullet.transform.position,3f,Quaternion.identity, 8,"FloatUp",new object[0]);
        }
        
            
        private static void ExplosionOnHit(GameObject bullet, GameObject target)
        {
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParam = bulletState.model.onHitParams;
            float damageTimes = onHitParam.Length > 0 ? (float)onHitParam[0] : 1.00f;
            CombatUtility.CreateAoe("ExplosionAoe",bullet.GetComponent<BulletState>().caster,bullet.transform.position,0.05f,Quaternion.identity, 11f,"",new object[0],new Dictionary<string, object>()
            {
                {"EffectIds","70001|70002|70003"},{"SoundIds","30005|30006|30007"},{"HitAlly","False"},{"DamageTimes",damageTimes.ToString()}
            });
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),70004)
            {
                Position = bullet.transform.position,
            });
            GameEntry.Sound.PlaySound(31016, bullet.transform.position);
            CombatUtility.CreateAoe("KnockOffAoe",bullet.GetComponent<BulletState>().caster,bullet.transform.position,0.05f,Quaternion.identity, 11f,"",new object[0],new Dictionary<string, object>()
            {
                {"Force","12"},{"Degree","35"}
            });
            
        }
        private static void ExplosionOnRemoved(GameObject bullet)
        {
            CombatUtility.CreateAoe("ExplosionAoe",bullet.GetComponent<BulletState>().caster,bullet.transform.position,0.05f,Quaternion.identity, 11f,"",new object[0],new Dictionary<string, object>()
            {
                {"EffectIds","70001|70002|70003"},{"SoundIds","30005|30006|30007"}
            });
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),70004)
            {
                Position = bullet.transform.position,
            });
            GameEntry.Sound.PlaySound(31016, bullet.transform.position);
            CombatUtility.CreateAoe("KnockOffAoe",bullet.GetComponent<BulletState>().caster,bullet.transform.position,0.05f,Quaternion.identity, 11f,"",new object[0],new Dictionary<string, object>()
            {
                {"Force","12"},{"Degree","35"}
            });
            
        }

        ///<summary>
        ///onRemoved
        ///普通子结束，参数：
        ///[0]命中视觉特效
        ///</summary>
        private static void CommonBulletRemoved(GameObject bullet){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onRemovedParams = bulletState.model.onRemovedParams;
            int sightEffect = onRemovedParams.Length > 0 ? (int)onRemovedParams[0] : 0;
            if (sightEffect !=0){
                GameEntry.Combat.CreateSightEffect(
                    sightEffect, 
                    bullet.transform.position, 
                    bullet.transform.rotation.eulerAngles.y
                );      
            }
        }

        ///<summary>
        ///targetting
        ///选择最近的敌人作为目标
        ///</summary>
        private static GameObject GetNearestEnemy(GameObject bullet, GameObject[] targets){
            BulletState bs = bullet.GetComponent<BulletState>();
            CampType casterCamp = CampType.Unknown;
            if (bs.caster){
                ChaState ccs = bs.caster.GetComponent<ChaState>();
                if (ccs) casterCamp = ccs.Camp;
            }

            GameObject bestTarget = null;
            float bestDis = float.MaxValue;
            for (int i = 0; i < targets.Length; i++){
                ChaState tcs = targets[i].GetComponent<ChaState>();
                if (!tcs || CombatComponent.GetRelation(casterCamp,tcs.Camp)==RelationType.Friendly || tcs.dead == true) continue;
                float dis2 = (
                    Mathf.Pow(bullet.transform.position.x - targets[i].transform.position.x, 2) +
                    Mathf.Pow(bullet.transform.position.z - targets[i].transform.position.z, 2)
                );
                if (bestDis > dis2 || bestTarget == null){
                    bestTarget = targets[i];
                    bestDis = dis2;
                }
            }

            return bestTarget;
        }
        ///<summary>
        ///tween
        ///跟踪目标
        ///</summary>
        private static Vector3 FollowingTarget(float t, GameObject bullet, GameObject target){
            Vector3 res = Vector3.forward;
            if (target!=null){
                Vector3 tarDir = target.transform.position - bullet.transform.position;
                float flyingRad = (Mathf.Atan2(tarDir.x,  tarDir.z) * 180 / Mathf.PI - bullet.transform.eulerAngles.y) * Mathf.PI / 180;

                res.x = Mathf.Sin(flyingRad);
                res.z = Mathf.Cos(flyingRad);
                
            }
            return res;
        }

        ///<summary>
        ///targetting
        ///选择子弹的施法者作为跟踪的目标
        ///</summary>
        private static GameObject BulletCasterSelf(GameObject bullet, GameObject[] targets){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return null;
            return bulletState.caster;
        }

        ///<summary>
        ///Tween
        ///氪漏氪回力标的轨迹，向前丢出去以后，会开始飞回到丢出去的人手里，bulletObj.param
        ///["backTime"]多少秒以后回头，在这个时间内移动速度呈sin函数
        ///</summary>
        private static Vector3 CloakBoomerangTween(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.forward;
            float backTime = bs.param.ContainsKey("backTime") ? (float)bs.param["backTime"] : 1.0f; //默认1秒 
            if (t < backTime){
                //飞出去的过程
                float rad = t / backTime * Mathf.PI;
                return Vector3.forward * (Mathf.Sin(rad) + 0.100f);
            }else{
                float rad = (bs.duration- t) / backTime * Mathf.PI;
                return Vector3.forward * (Mathf.Sin(rad) + 0.100f);
            }
        }
        ///<summary>
        ///onHit
        ///氪漏氪回力标命中效果，除了普通效果，就是命中自己的时候移除子弹，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CloakBoomerangHit(GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return;

            ChaState ccs = bs.caster.GetComponent<ChaState>();
            ChaState tcs = target.GetComponent<ChaState>();
            if (ccs != null && tcs != null && CombatComponent.GetRelation(ccs.Camp,tcs.Camp)!=RelationType.Friendly){
                CommonBulletHit(bullet, target);
            }else{
                float backTime = bs.param.ContainsKey("backTime") ? (float)bs.param["backTime"] : 1.0f; //默认1秒 
                if (bs.timeElapsed > backTime && target.Equals(bs.caster)){
                    GameEntry.Combat.RemoveBullet(bullet);
                    if (ccs) ccs.PlaySightEffect("Body",70000);
                }
            }
        }

        ///<summary>
        ///Tween
        ///逐渐加速的子弹，bulletObj参数：
        ///["turningPoint"]float：在第几秒达到预设的速度（100%），并且逐渐减缓增速。
        ///</summary>
        private static Vector3 SlowlyFaster(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs) return Vector3.forward;
            float tp = 5.0f; //默认5秒后达到100%速度
            if (bs.param.ContainsKey("turningPoint")) tp = (float)bs.param["turningPoint"];
            if (tp < 1.0f) tp = 1.0f;
            return Vector3.forward * (2 * t / (t + tp));
        }

        ///<summary>
        ///onCreate
        ///记录一下这个子弹，作为最后发射的子弹
        ///</summary>
        private static void RecordBullet(GameObject bullet){
            BulletState bs = bullet.GetComponent<BulletState>();
            if (!bs || bs.caster == null) return;
            ChaState cs = bs.caster.GetComponent<ChaState>();
            if (!cs) return;
            List<BuffObj> bos = cs.GetBuffById("TeleportBulletPassive", new List<GameObject>(){bs.caster});
            if (bos.Count <= 0){
                cs.AddBuff(new AddBuffInfo(
                    DesingerTables.Buff.data["TeleportBulletPassive"], bs.caster, bs.caster, 1, 10, true, true, new Dictionary<string, object>(){{"firedBullet", bullet}}
                ));
            }else{
                bos[0].buffParam["firedBullet"] = bullet;
            }
        }

        ///<summary>
        ///onCreate
        ///手雷丢出去，要设置一下动画那个
        ///</summary>
        private static void SetBombBouncing(GameObject bullet){
            BulletState bs = bullet.GetComponent<BulletState>();
            BouncingBallY bb = bullet.GetComponentInChildren<BouncingBallY>();
            if (!bs || !bb) return;
            float totalTime = bs.duration;
            if (totalTime <= 0){
                Debug.Log("Boom Explosed immeditly");
                return;
            }
            float[] dTime = new float[]{
                totalTime * 3.000f / 6.000f,
                totalTime * 5.000f / 6.000f,
                totalTime - 0.001f
            };
            float highest = 2.2f;
            bb.ResetTo(highest, dTime);           
        }

        ///<summary>
        ///Tween
        ///手雷的轨迹，在这里要做的是修改bullet的移动模式，一般不推荐这么干
        ///</summary>
        private static Vector3 BoomBallRolling(float t, GameObject bullet, GameObject target){
            BulletState bs = bullet.GetComponent<BulletState>();
            BouncingBallY bb = bullet.GetComponentInChildren<BouncingBallY>();
            if (!bs || !bb) return Vector3.forward;
            MoveType toType = MoveType.fly;
            if (bb.hitGroundAt.Length <= 0 || t > bb.hitGroundAt[bb.hitGroundAt.Length - 1]){
                toType = MoveType.ground;
            }else{
                float tt = Time.fixedDeltaTime;
                for (int i = 0; i < bb.hitGroundAt.Length; i++){
                    if (bb.hitGroundAt[i] - tt <= t && t <= bb.hitGroundAt[i] + tt){
                        toType = MoveType.ground;
                        break;
                    }
                }
            }
            
            bs.SetMoveType(toType);
            return Vector3.forward;
        }

        ///<summary>
        ///onRemoved
        ///在子弹位置创建一个aoe，所以aoe的始作俑者肯定是caster了，位置也是子弹位置，填写什么都无效，角度也是子弹角度，参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///[1]AoeLauncher：如果bullet移除时后duration>0或者是obstacled，则会创建这个，如果有这个的话
        ///</summary>
        private static void CreateAoEOnRemoved(GameObject bullet){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onRemovedParams = bulletState.model.onRemovedParams;
            if (onRemovedParams.Length <= 0)    return;
            AoeLauncher al = (AoeLauncher)onRemovedParams[0];
            if (onRemovedParams.Length > 1 && (bulletState.duration > 0 || bulletState.HitedObstacle == true)) {
                al = (AoeLauncher)onRemovedParams[1];
            }
            al.caster = bulletState.caster;
            al.position = bullet.transform.position;
            al.rotation = bullet.transform.rotation;
            Debug.Log("to create aoe effect " + al.model.entityTypeId);
            GameEntry.Combat.CreateAoE(al);
        }

        ///<summary>
        ///onHit
        ///在子弹位置创建一个aoe，所以aoe的始作俑者肯定是caster了，位置也是子弹位置，填写什么都无效，角度也是子弹角度，参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///</summary>
        private static void CreateAoEOnHit(GameObject bullet, GameObject target){
            BulletState bulletState = bullet.GetComponent<BulletState>();
            if (!bulletState) return;
            object[] onHitParams = bulletState.model.onHitParams;
            if (onHitParams.Length <= 0)    return;
            AoeLauncher al = (AoeLauncher)onHitParams[0];
            al.caster = bulletState.caster;
            al.position = bullet.transform.position;
            al.rotation = bullet.transform.rotation;
            GameEntry.Combat.CreateAoE(al);
        }

    }
}