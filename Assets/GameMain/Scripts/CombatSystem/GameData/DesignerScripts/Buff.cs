using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameMain;
using UnityEngine;
using UnityEngine.XR;

namespace DesignerScripts
{
    ///<summary>
    ///buff的效果
    ///</summary>
    public class Buff{
        public static Dictionary<string, BuffOnCollide> onCollideFunc = new Dictionary<string, BuffOnCollide>(){
            {"CollideAnimHandle", CollideAnimHandle},
            {"KnockOffOtherOnMove", KnockOffOtherOnMove},
            {"ExplosionOnCollide", ExplosionOnCollide},//煤气罐爆炸
            {"TakeDamageOnGrounded", TakeDamageOnGrounded},//把小怪砸地时
            {"KnockOffOtherOnCollide", KnockOffOtherOnCollide},//小怪冲撞
            {"DamageOtherOnCollide", DamageOtherOnCollide},//小怪冲撞
            {"PickBallOnCollide", PickBallOnCollide},//捡起足球
        };
        public static Dictionary<string, BuffOnOccur> onOccurFunc = new Dictionary<string, BuffOnOccur>(){
            {"KnockOff", KnockOff},
            {"StartSitting", StartSitting},
            {"DropAll", DropAll},
            {"ResetVelocity", ResetVelocity},
        };
        public static Dictionary<string, BuffOnRemoved> onRemovedFunc = new Dictionary<string, BuffOnRemoved>(){
            {"TeleportCarrier", TeleportCarrier},
           
        };
        public static Dictionary<string, BuffOnTick> onTickFunc = new Dictionary<string, BuffOnTick>(){
            {"BarrelDurationLose", BarrelDurationLose},
            {"DoPercentDamageToCarrier", DoPercentDamageToCarrier},
            {"RemoveKnockedOffBuffOnGround",RemoveKnockedOffBuffOnGround},
            {"ReduceOxygen",ReduceOxygen},
        };
        public static Dictionary<string, BuffOnCast> onCastFunc = new Dictionary<string, BuffOnCast>(){
            {"ReloadAmmo", ReloadAmmo},
            {"FireTeleportBullet", FireTeleportBullet}
        };
        public static Dictionary<string, BuffOnHit> onHitFunc = new Dictionary<string, BuffOnHit>(){
            {"AirAttack",AirAttack},
        };
        public static Dictionary<string, BuffOnBeHurt> beHurtFunc = new Dictionary<string, BuffOnBeHurt>(){
            {"OnlyTakeOneDirectDamage", OnlyTakeOneDirectDamage},
            {"EndSitting", EndSitting},
            {"BasketballWeakness", BasketballWeakness},
            {"DamageOtherOnHurt", DamageOtherOnHurt},
            
        };
        public static Dictionary<string, BuffOnKill> onKillFunc = new Dictionary<string, BuffOnKill>(){
            
        };
        public static Dictionary<string, BuffOnBeKilled> beKilledFunc = new Dictionary<string, BuffOnBeKilled>(){
            {"BarrelExplosed", BarrelExplosed},
            {"ExplosionOnBeKilled", ExplosionOnBeKilled},
            {"ShowBattleUnitOnBeKilled", ShowBattleUnitOnBeKilled},
            {"DropAll", DropAll},
        };

        private static readonly int Speed = Animator.StringToHash("Speed");


        ///<summary>
        ///onCast
        ///如果子弹不够放技能，就会填装子弹
        ///no params
        ///</summary>
        private static TimelineObj ReloadAmmo(BuffObj buff, SkillObj skill, TimelineObj timeline){
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            return (cs.resource.Enough(skill.model.cost) == true) ? timeline : 
                new TimelineObj(DesingerTables.Timeline.data["skill_reload"], buff.carrier, new object[0]);
        }

        ///<summary>
        ///onCast
        ///判断自己的param的"firedBullet"，如果firedBullet已经不存在了，或者压根不存在，就发射子弹，否则，就传送过去，参数：
        ///["firedBullet"]GameObject：firedBullet，理论上也可以是别的东西
        ///</summary>
        private static TimelineObj FireTeleportBullet(BuffObj buff, SkillObj skill, TimelineObj timeline){
            if (skill.model.id != "teleportBullet") return timeline;
            GameObject firedBullet = buff.buffParam.ContainsKey("firedBullet") ? (GameObject)buff.buffParam["firedBullet"] : null;
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            
            if (firedBullet == null){
                buff.buffParam["firedBullet"] = null;
                return timeline;
            }else{
                if (cs == null ){
                    GameEntry.FlyText.FlyText(buff.carrier.transform.position,"<color=red>无法传送</color>",Color.red);
                    return null;    //如果没有角色了，或者说飞弹的位置不能传送，那么就返回一个空，也就是不让放技能
                }
                return new TimelineObj(DesingerTables.Timeline.data["skill_teleportBullet_tele"], timeline.caster, null);
            }
        }

        public static void StartSitting(BuffObj buffObj,int occurCount)
        {
            if (occurCount == 1)
            {
                AnimOrder animOrder = (AnimOrder)buffObj.model.onOccurParams[0];
                var cha = buffObj.carrier.GetComponent<ChaState>();
                cha.AddAnimOrder(animOrder);
            }
            
        }
        
        
        //OnHurt
        public static void EndSitting(BuffObj buffObj, ref DamageInfo damageInfo, GameObject attacker)
        {
            AnimOrder animOrder = (AnimOrder)buffObj.model.onBeHurtParams[0];
            var cha = buffObj.carrier.GetComponent<ChaState>();
            cha.AddAnimOrder(animOrder);
            buffObj.duration = 0;
            //Debug.LogError("EndSitting");
        }
        
        public static void DamageOtherOnHurt(BuffObj buffObj, ref DamageInfo damageInfo, GameObject attacker)
        {
            //应该添加更多参数，之后用到再说
            var targetCha = buffObj.carrier.GetComponentInParent<ChaState>();
            if (targetCha && attacker!=null)
            {
                GameEntry.Combat.CreateDamage(buffObj.carrier,attacker,new Damage(0,0,0,1));
                var attackerCha = attacker.GetComponent<ChaState>();
                if (attackerCha)
                {
                    attackerCha.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"Hurt");
                }
            }
        }
        
        public static void BasketballWeakness(BuffObj buffObj, ref DamageInfo damageInfo, GameObject attacker)
        {
            try
            {
                if (damageInfo.attacker.GetComponent<ChaState>().tags.Contains("Basketball")) //如果攻击者是篮球，伤害10倍
                {
                    damageInfo.damage *= 10f;
                }
            }
            catch (Exception e)//有异常不执行就行了，也不用管，不然写null判断太麻烦了
            {
                
            }
            
        }

        ///<summary>
        ///onRemoved
        ///把buff的carrier传送到记录的子弹的世界坐标（非常危险，因为那个坐标未必能站立），并且移除掉那个子弹
        ///</summary>
        private static void TeleportCarrier(BuffObj buff){
            ChaState cs = buff.carrier.GetComponent<ChaState>();
            if (cs.dead) return;
            List<BuffObj> fireRec = cs.GetBuffById("TeleportBulletPassive", new List<GameObject>(){buff.caster});
            if (fireRec.Count <= 0) return;
            GameObject bullet = fireRec[0].buffParam.ContainsKey("firedBullet") ? (GameObject)fireRec[0].buffParam["firedBullet"] : null;
            if (bullet == null) return;
            buff.carrier.transform.position = new Vector3(bullet.transform.position.x, 0, bullet.transform.position.z);
            GameEntry.Combat.RemoveBullet(bullet);
        }

        ///<summary>
        ///beHurt
        ///buff的Carrier只能受到1点直接伤害，免疫其他一切，桶子就是这样的
        ///</summary>
        private static void OnlyTakeOneDirectDamage(BuffObj buff, ref DamageInfo damageInfo, GameObject attacker){
            bool isDirectDamage = false;
            for (int i = 0; i < damageInfo.tags.Length; i++){
                if (damageInfo.tags[i] == DamageInfoTag.directDamage){
                    isDirectDamage = true;
                    break;
                }
            }
            if (isDirectDamage == true && damageInfo.DamageValue(false) > 0){
                int finalDV = 1;
                if (attacker != null){
                    ChaState cs = attacker.GetComponent<ChaState>();
                    //来自另外一个桶子（不包含自己）的伤害为9999，其他的都是1
                    if (cs != null && cs.HasTag("Barrel") == true && attacker.Equals(buff.carrier) == false){
                        finalDV = 9999;
                    }
                }
                damageInfo.damage = new Damage(0, finalDV);
            }else{
                damageInfo.damage = new Damage(0);
            }
        }
        ///<summary>
        ///onTick
        ///桶子每5秒对自己伤害，其实可以写一个公用的dot，不过这里表达的是，不公用也没问题
        ///</summary>
        private static void BarrelDurationLose(BuffObj buff){
            GameEntry.Combat.CreateDamage(buff.carrier, buff.carrier, new Damage(0,1), 0, 0, new DamageInfoTag[]{DamageInfoTag.directDamage});
        }

        private static void DropAll(BuffObj buff, DamageInfo damageInfo, GameObject attacker)
        {
            DropAll(buff,1);
        }

        private static void DropAll(BuffObj buffObj,int stack)
        {
            var cha = buffObj.carrier.GetComponent<ChaState>();
           
            
            var bindManager = cha.GetBindManager();

            if(bindManager==null)
                return;
            
            var snatchPos = bindManager.GetBindPointByKey("SnatchPos");
            if (snatchPos!=null && snatchPos.transform!=null && snatchPos.transform.childCount > 0)
            {
                var childEntity = snatchPos.transform.GetChild(0).GetComponent<Entity>();
                GameEntry.Entity.DetachEntity(childEntity.Entity);//只负责接触就行了，其余的由OnDetached回调处理
            }
            
            var holdPos = bindManager.GetBindPointByKey("HoldEnemyPos");
            if (holdPos!=null && holdPos.transform!=null && holdPos.transform.childCount > 0)
            {
                var childEntity = holdPos.transform.GetChild(0).GetComponent<Entity>();
                GameEntry.Entity.DetachEntity(childEntity.Entity);
                
            }
            
            //足球和篮球需要在Battleunit的OnAttached和OnDetached中处理添加和接触后操作。比如忽略碰撞和恢复碰撞等。
            
            var footballPos = bindManager.GetBindPointByKey("FootballPos");
            if (footballPos!=null && footballPos.transform!=null && footballPos.transform.childCount > 0)
            {
                var childEntity = footballPos.transform.GetChild(0).GetComponent<Entity>();
                GameEntry.Entity.DetachEntity(childEntity.Entity);
            }
            
            var basketballPos = bindManager.GetBindPointByKey("BasketballPos");
            if (basketballPos!=null && basketballPos.transform!=null && basketballPos.transform.childCount > 0)
            {
                var childEntity = basketballPos.transform.GetChild(0).GetComponent<Entity>();
                
                GameEntry.Entity.DetachEntity(childEntity.Entity);
            }
        }

        private static void ResetVelocity(BuffObj buffObj, int stack)
        {
            var rb = buffObj.carrier.GetComponent<Rigidbody>();
            if(rb)
                rb.velocity=Vector3.zero;
            
        }


        private static void CollideAnimHandle(BuffObj buff, GameObject collide)
        {
            //改为海底版后的移动后角色的来不及碰撞就会开始新的移动，地面检测先于碰撞检测导致碰撞检测无法触发，所以先不用这个代码了
            return;
            var rb = buff.carrier.GetComponent<Rigidbody>();
            if(rb==null)
                return;

            if (collide.gameObject.CompareTag("Ground")==false)
            {
                return;
            }
            
           
            var cha = buff.carrier.GetComponent<ChaState>();
            if (cha)
            {
                cha.AddAnimOrder(UnitAnim.AnimOrderType.Float,"BounceSpeed",rb.velocity.magnitude);
                cha.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"Bounce");
                //Debug.Log("碰撞速度"+rb.velocity.magnitude);
                
                
                
            }
        }
        
        /// <summary>
        /// 碰撞到别人时如果速度够快就击飞别人,这个基本是玩家专用
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="collide"></param>
        private static void KnockOffOtherOnMove(BuffObj buff, GameObject collide)
        {

            var carrierCha = buff.carrier.GetComponent<ChaState>();
            var targetCha = collide.GetComponentInParent<ChaState>();
            if (targetCha)
            {
                var carrierRb = carrierCha.GetComponent<Rigidbody>();
                if (carrierRb)
                {
                    //先不实现这个了，可能都不要。
                    //CombatUtility.KnockOff();
                    if (carrierCha.HasTag("Player"))
                    {
                        var animator = carrierCha.GetAnimator();
                        bool isMove = animator.GetCurrentAnimatorStateInfo(0).IsName("Move");
                        if(!isMove)
                            return;
                        if (animator.GetFloat(Speed) < 1.1f)
                        {
                            //把人令起来
                            var bindManager = carrierCha.GetBindManager();
                            var basketballTrans = bindManager.GetBindPointByKey("BasketballPos");
                            if(basketballTrans.transform.childCount>0)
                                return;
                            var holdPosTrans = bindManager.GetBindPointByKey("HoldEnemyPos");
                            if(holdPosTrans.transform.childCount>0)
                                return;
                            var targetEntity = targetCha.GetComponent<BattleUnit>();
                            if(targetEntity && targetEntity.GetBattleUnitData().tags.Contains("HoldAble"))//21000以上的实体是道具实体，不能被拎起来。
                                GameEntry.Entity.AttachEntity(targetEntity.Entity,carrierCha.GetComponent<Entity>().Entity,holdPosTrans.transform);
                        }
                        else
                        {
                            var targetEntity = targetCha.GetComponent<BattleUnit>();
                            if (targetEntity &&
                                targetEntity.GetBattleUnitData().tags.Contains("PushAble")) //21000以上的实体是道具实体，不能被撞飞。
                            {
                                //奔跑时碰撞敌人并造成伤害
                                //CombatUtility.KnockOff(targetCha.GetComponent<Rigidbody>(),carrierCha.transform.right,70,-targetCha.transform.forward,30f);
                                GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["KnockOffAoe"],buff.carrier,buff.carrier.transform.TransformPoint(1,1,0),0.05f,Quaternion.identity,4f));

                                //伤害Aoe
                                GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["AttackAoe"],buff.carrier,buff.carrier.transform.TransformPoint(1,1,0),0.05f,Quaternion.identity,4f));

                            }
                        }
                    }
                }
                
            }
        }

        private static void PickBallOnCollide(BuffObj buff, GameObject collide)
        {
            
            var targetCha = collide.GetComponentInParent<ChaState>();
            if (targetCha && !targetCha.dead)
            {
                bool holding = false;
                
                
                if (targetCha.tags.Contains("Football"))
                {
                    var carrierCha = buff.carrier.GetComponent<ChaState>();
                    if(!carrierCha.tags.Contains("FootballMob"))
                        return;
                    
                    var footballPosTrans = carrierCha.GetBindManager().GetBindPointByKey("FootballPos").transform;
                    GameEntry.Entity.AttachEntity(targetCha.GetComponent<Entity>().Entity,carrierCha.GetComponent<Entity>().Entity,footballPosTrans);
                   
                }
                if (targetCha.tags.Contains("Basketball"))
                {
                    var carrierCha = buff.carrier.GetComponent<ChaState>();

                    // if(!carrierCha.tags.Contains("BasketballMob"))
                    //     return;

                    var bindManager = carrierCha.GetBindManager();
                   
                    var holdingTrans = bindManager.GetBindPointByKey("HoldEnemyPos");
                    if( holdingTrans!=null &&holdingTrans.transform.childCount>0)
                        return;
                    var basketballTrans = bindManager.GetBindPointByKey("BasketballPos").transform;
                    GameEntry.Entity.AttachEntity(targetCha.GetComponent<Entity>().Entity,carrierCha.GetComponent<Entity>().Entity,basketballTrans);
                    //在OnAtted和OnDetached添加持有足球状态buff
                }
            }
        }
        
        /// <summary>
        /// 碰撞到别人时击飞别人
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="collide"></param>
        private static void KnockOffOtherOnCollide(BuffObj buff, GameObject collide)
        {

            //Debug.LogError("撞到collide"+collide.gameObject.name);
            //var carrierCha = buff.carrier.GetComponent<ChaState>();
            var targetCha = collide.GetComponentInParent<ChaState>();
            //buff.buffParam.TryGetValue("hittedGameObjects", out var hittedGameObjects);
            
            //Debug.LogError(buff.carrier.name+" KnockOffOtherOnCollider碰撞到"+collide);
            if (targetCha)
            {

                    var scale = 6f;
                    
                    buff.buffParam.TryGetValue("Force", out var forceParam);
                    buff.buffParam.TryGetValue("Degree", out var degreeParam);
                    buff.buffParam.TryGetValue("Scale", out var scaleParam);
                    buff.buffParam.TryGetValue("ForceDamageIgnoreBehindCheck", out var forceDamageIgnoreBehindCheckParam);
                    if (scaleParam != null)
                    {
                        scale = float.Parse((string)scaleParam);
                    }
                    buff.buffParam.TryGetValue("DamageTimes", out var damageTimesParam);
            
                    Dictionary<string, object> knockOffAoeParam = new Dictionary<string, object>();
                    knockOffAoeParam.Add("Force",forceParam);
                    knockOffAoeParam.Add("Degree",degreeParam);
                   
                    
                    Dictionary<string, object> attackAoeParam = new Dictionary<string, object>();
                    attackAoeParam.Add("DamageTimes",damageTimesParam);
                    attackAoeParam.Add("ForceDamageIgnoreBehindCheck",forceDamageIgnoreBehindCheckParam);
                    
            
                
                    var targetEntity = targetCha.GetComponent<BattleUnit>();
                    if (targetEntity &&
                        (targetEntity.GetBattleUnitData().tags.Contains("PushAble") || (forceDamageIgnoreBehindCheckParam!=null && bool.Parse((string)forceDamageIgnoreBehindCheckParam) ))) 
                    {
                        
                        //Debug.LogError($"{buff.carrier.gameObject.name} collide {targetEntity.gameObject.name},Aoe位置{buff.carrier.transform.position}");
                        //奔跑时碰撞敌人并造成伤害
                        //CombatUtility.KnockOff(targetCha.GetComponent<Rigidbody>(),carrierCha.transform.right,70,-targetCha.transform.forward,30f);
                        GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["KnockOffAoe"],buff.carrier,buff.carrier.transform.position,0.05f,Quaternion.identity,scale,null,null,knockOffAoeParam));

                        //伤害Aoe
                        GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["AttackAoe"],buff.carrier,buff.carrier.transform.position,0.05f,Quaternion.identity,scale,null,null,attackAoeParam));

                    }

            }
        }
        
        
        private static void DamageOtherOnCollide(BuffObj buff, GameObject collide)
        {

            //var carrierCha = buff.carrier.GetComponent<ChaState>();
            var targetCha = collide.GetComponentInParent<ChaState>();
            if (targetCha)
            {
                buff.buffParam.TryGetValue("MeleeDamage", out var meleeDamageParam);
                buff.buffParam.TryGetValue("DamageTimes", out var damageTimesParam);
                buff.buffParam.TryGetValue("ToFoe", out var toFoeParam);
                buff.buffParam.TryGetValue("ToAlly", out var toAllyParam);
                buff.buffParam.TryGetValue("Scale", out var scaleParam);

                if (scaleParam == null)
                    scaleParam = 5;
            
                Dictionary<string, object> aoeParam = new Dictionary<string, object>();
                aoeParam.Add("MeleeDamage",meleeDamageParam);
                aoeParam.Add("DamageTimes",damageTimesParam);
                aoeParam.Add("ToFoe",toFoeParam);
                aoeParam.Add("ToAlly",toAllyParam);
               
            
                
                var targetEntity = targetCha.GetComponent<BattleUnit>();
                if (targetEntity  && targetEntity.Visible) 
                {
                    
                    //伤害Aoe
                    GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["AttackAoe"],buff.carrier,buff.carrier.transform.position,0.05f,Quaternion.identity,float.Parse((string)scaleParam),null,null,aoeParam));

                }

            }
        }
        
            
        /// <summary>
        /// 碰撞到别人时爆炸
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="collide"></param>
        private static void ExplosionOnCollide(BuffObj buff, GameObject collide)
        {
           
            GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["ExplosionAoe"],buff.carrier,buff.carrier.transform.position,0.1f,Quaternion.identity,12f));
            GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["KnockOffAoe"],buff.caster,buff.carrier.transform.position,0.1f,Quaternion.identity,12f));
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),70004)
            {
                Position = buff.carrier.transform.position,
                
            });
            var entity = buff.carrier.GetComponent<Entity>();
            if (entity.Visible)
            {
                GameEntry.Entity.HideEntity(entity);
            }
            
        }
        
        /// <summary>
        /// 落地时受伤，然后删除此Buff
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="collide"></param>
        private static void TakeDamageOnGrounded(BuffObj buff, GameObject collide)
        {
            if (collide.gameObject.CompareTag("Ground"))
            {
                GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["AttackAoe"],buff.caster,buff.carrier.transform.position,0.1f,Quaternion.identity,8f));
                buff.duration = 0;
            }
        }

        private static void AirAttack(BuffObj buff, ref DamageInfo damageInfo, GameObject target)
        {
            
            var owner = buff.carrier;
            
            var rb = owner.GetComponent<Rigidbody>();
            if (rb)
            {
               // var cha = buff.carrier.GetComponent<ChaState>();
              
            }
        }

        private static void KnockOff(BuffObj buff,int stack)
        {
            if (stack != 1)
            {
                return;
            }
            
            
            
            return;
            var carrierTrans = buff.carrier.transform;
            var dir = (carrierTrans.position - buff.caster.transform.position);
            dir.z = 0;
            dir.y = 0;
            var targetPos = carrierTrans.position + dir * 2.7f;
            var targetPos1=carrierTrans.position + dir * 3.5f;

            float firstJumpHeight = 4f;
            float secondJumpHeight = 1f;
            float firstJumpDuration = 0.5f;
            float secondJumpDuration = 0.5f;

            // 第一次跳跃
            carrierTrans.DOJump(targetPos, firstJumpHeight, 1, firstJumpDuration).OnComplete(() => {
                OnFirstLanding();
            
                // 第二次跳跃
                carrierTrans.DOJump(targetPos1, secondJumpHeight, 1, secondJumpDuration).OnComplete(() => {
                    buff.duration = 0;
                });
            }).SetEase(Ease.Linear);
            
            
            void OnFirstLanding() {
                // 第一次落地时的处理逻辑
                Debug.Log("第一次落地");
            }
        }

        private static void ReduceOxygen(BuffObj buff)
        {
            
            var cha = buff.carrier.GetComponent<ChaState>();
            if(cha==null)
                return;
           
            if (cha.resource.oxygen <= 0)
            {
                cha.resource.oxygen = 0;
                var damageValue = cha.property.hp* (5)*0.01f ;
                GameEntry.Combat.CreateDamage(buff.caster, buff.carrier, new Damage(0,0,0,Mathf.CeilToInt(damageValue)), 0, 0, new DamageInfoTag[]{DamageInfoTag.periodDamage});
                cha.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"Hurt");
                var bindManager = cha.GetBindManager();
                if (bindManager != null)
                {
                    var bindPoint = bindManager.GetBindPointByKey("DashBubbleFxPoint");
                    if (bindPoint != null)
                    {
                        GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),70015)
                        {
                            Position = bindPoint.transform.position,
                            Rotation = bindPoint.transform.rotation,
                        });
                        GameEntry.Sound.PlaySound(31028);
                    }
                }
            }
            else
            {
                var onTickParams = buff.model.onTickParams;
                var ratio = onTickParams.Length > 0 ? (float)onTickParams[0] : 1f;
                var toReduce =  ratio;
                cha.resource -= new ChaResource(0, 0, 0, toReduce);
            }
            
        }

        private static void DoPercentDamageToCarrier(BuffObj buff)
        {
            var cha = buff.carrier.GetComponent<ChaState>();
            if(cha==null)
                return;
            var onTickParams = buff.model.onTickParams;
            var damageType = onTickParams.Length > 0 ? (string)onTickParams[0] : "MaxHealth";
            var basePercent = onTickParams.Length > 1 ? (float)onTickParams[1] : 1;
            var stackPercent = onTickParams.Length > 2 ? (float)onTickParams[2] : 0;

            float damageValue = 0;
            if (damageType.ToLower().Contains("max"))
            {
                damageValue = cha.property.hp* (basePercent+stackPercent*buff.stack)*0.01f ;
            }
            else if(damageType.ToLower().Contains("current"))
            {
                damageValue = cha.resource.hp* (basePercent+stackPercent*buff.stack)*0.01f ;

            }
            GameEntry.Combat.CreateDamage(buff.caster, buff.carrier, new Damage(0,0,0,Mathf.CeilToInt(damageValue)), 0, 0, new DamageInfoTag[]{DamageInfoTag.periodDamage});

        }
        
        

        private static void RemoveKnockedOffBuffOnGround(BuffObj buff)
        {
            var cha = buff.carrier.GetComponent<ChaState>();
            if (cha == null)
                return;

            var lieDownDuration = 0.7f;
            buff.buffParam.TryGetValue("LieDownDuration", out var lieDownDurationParam);
            if (lieDownDurationParam != null)
            {
                lieDownDuration = float.Parse((string)lieDownDurationParam);
            }

            var knockedOffBuffs = cha.GetBuffById("KnockedOff");
            if (true)
            {
                var groundChecker = cha.GetComponent<GroundChecker>();
                if (groundChecker)
                {
                    if (groundChecker.grounded)
                    {
                        //移除击飞Buff
                        //Debug.LogError("移除击非，RemoveKnockedOffBuffOnGround");
                        cha.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"Bounce");

                        foreach (var knockedOffBuff in knockedOffBuffs)
                        {
                            knockedOffBuff.duration = 0;//移除所有击飞buff而不是移除单个击飞，因为buff是根据不同caster可以存在多个
                        }
                        // cha.AddBuff(new AddBuffInfo(DesingerTables.Buff.data["KnockedOff"], null, buff.carrier, 1,
                        //     0, true));
                        cha.AddBuff(new AddBuffInfo(DesingerTables.Buff.data["LieDown"], null, buff.carrier, 1,
                            lieDownDuration, true));
                        cha.AddBuff(new AddBuffInfo(DesingerTables.Buff.data["CollideAnimHandle"], null,
                            buff.carrier, 1, 0, true));
                        buff.duration = 0;
                        

                    }
                }
            }
            
        }

        ///<summary>
        ///beKilled
        ///死亡后爆炸，对敌人造成伤害，其他桶子也是其他敌人，所以不必特殊处理，beHurt已经特殊处理了，当然还要立即清除掉这个桶子。
        ///</summary>
        private static void BarrelExplosed(BuffObj buff, DamageInfo damageInfo, GameObject attacker){
            GameObject aoeCaster = buff.caster != null ? buff.caster : buff.carrier;
            //AoeModel是可以动态生成的
            GameEntry.Combat.CreateAoE(new AoeLauncher(
                new AoeModel(
                    "BoomExplosive", 0, new string[0], 0, false,
                    "CreateSightEffect", new object[]{"Effect/Explosion_A"},
                    "BarrelExplosed", new object[0], 
                    "", new object[0],  //tick
                    "", new object[0],  //chaEnter
                    "", new object[0],  //chaLeave
                    "", new object[0],  //bulletEnter
                    "", new object[0]   //bulletLeave
                ), 
                aoeCaster, buff.carrier.transform.position, 2.2f, Quaternion.identity,
                1,null, null, new Dictionary<string, object>(){
                    {"Barrel", buff.carrier}
                }
            ));
            //隐藏自己，反正后面会被Remover移走
            buff.carrier.transform.localScale = Vector3.zero;
        }


        private static void ShowBattleUnitOnBeKilled(BuffObj buff, DamageInfo damageInfo, GameObject attacker)
        {
            var onBeKilledParams = buff.model.onBeKilledParams;
            var typeId = onBeKilledParams.Length > 0 ? (int)onBeKilledParams[0] : 0;
            if (buff.buffParam.TryGetValue("TypeId", out var newTypeId))
            {
                if (newTypeId != null)
                {
                    typeId = int.Parse((string)newTypeId);
                }
            }
            if (typeId > 0)
            {
                var pos = buff.carrier.transform.position ;
                var rotation =buff.carrier.transform.rotation;
                var camp = buff.carrier.GetComponent<ChaState>().Camp;

                
                GameEntry.Timer.AddOnceTimer(1000, () =>
                {
                    int seralId = GameEntry.Entity.GenerateSerialId();
                   
                    GameEntry.Entity.ShowBattleUnit(new BattleUnitData(seralId, typeId,
                        camp)
                    {
                        Position = pos,
                        Rotation = rotation,
                    });
                });

            }
        }

        private static void ExplosionOnBeKilled(BuffObj buff, DamageInfo damageInfo, GameObject attacker)
        {
            
            buff.buffParam.TryGetValue("ExplosionAoeId", out var explosionAoeId);
            buff.buffParam.TryGetValue("PlayExplosionFx", out var playExplosionFx);
            
            buff.buffParam.TryGetValue("ExplosionDamage", out var explosionDamage);
            buff.buffParam.TryGetValue("DamageTimes", out var damageTimes);
            buff.buffParam.TryGetValue("ToFoe", out var toFoe);
            buff.buffParam.TryGetValue("ToAlly", out var toAlly);
            buff.buffParam.TryGetValue("HurtAction", out var hurtAction);
            buff.buffParam.TryGetValue("EffectIds", out var effectIds);
            buff.buffParam.TryGetValue("SoundIds", out var soundIds);
            buff.buffParam.TryGetValue("OnlyDamage", out var onlyDamageParams);
            buff.buffParam.TryGetValue("Force", out var forceParams);

            bool hidEntity = true;
            if (buff.buffParam.TryGetValue("HideOnExplosion", out var hideEntityParams))
            {
                hidEntity=Boolean.Parse((string)hideEntityParams);
            }

            
            Dictionary<string, object> aoeParam = new Dictionary<string, object>();
            
            if (explosionAoeId == null)
                explosionAoeId = "ExplosionAoe";
            
            if (playExplosionFx == null)
                playExplosionFx = "True";


            if (forceParams != null)
            {
                aoeParam.Add("Force",forceParams);
            }
            
            //只有参数有变更才加到aoe的参数中
            if (explosionDamage != null)
            {
                aoeParam.Add("ExplosionDamage",explosionDamage);
            }
            if (damageTimes != null)
            {
                aoeParam.Add("DamageTimes",damageTimes);
            }
            if (toFoe != null)
            {
                aoeParam.Add("ToFoe",toFoe);
            }
            if (toAlly != null)
            {
                aoeParam.Add("ToAlly",toAlly);
            }
            if (hurtAction != null)
            {
                aoeParam.Add("HurtAction",hurtAction);
            }
            if (effectIds != null)
            {
                aoeParam.Add("EffectIds",effectIds);
            }
            if (soundIds != null)
            {
                aoeParam.Add("SoundIds",soundIds);
            }

            //传递阵营用于死亡后阵营判定
            aoeParam.Add("Camp",buff.carrier.GetComponent<ChaState>().Camp);

            GameEntry.Sound.PlaySound(31016);

            //演示触发，避免把同帧把自己炸死多次，导致出发多次这个函数
            GameEntry.Timer.AddFrameTimer(() =>
            {
                if (onlyDamageParams != null && bool.Parse((string)onlyDamageParams) == true)
                {
                    GameEntry.Combat.CreateAoE(new AoeLauncher(
                        DesingerTables.AoE.data[(string)explosionAoeId ?? string.Empty], buff.carrier,
                        buff.carrier.transform.position, 0.1f, Quaternion.identity, 12f, null, null, aoeParam));

                }
                else
                {
                    GameEntry.Combat.CreateAoE(new AoeLauncher(DesingerTables.AoE.data["KnockOffAoe"], buff.carrier,
                        buff.carrier.transform.position, 0.1f, Quaternion.identity, 12f));
                    GameEntry.Combat.CreateAoE(new AoeLauncher(
                        DesingerTables.AoE.data[(string)explosionAoeId ?? string.Empty], buff.carrier,
                        buff.carrier.transform.position, 0.1f, Quaternion.identity, 12f, null, null, aoeParam));

                }
            });
            
           
            if (bool.Parse((string)playExplosionFx))
            {
                GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),70004)
                {
                    Position = buff.carrier.transform.position,
                        
                });
            }
            
            var entity = buff.carrier.GetComponent<Entity>();
            if (entity.Visible && hidEntity)
            {
                GameEntry.Entity.HideEntity(entity);
            }
        }
    }
}