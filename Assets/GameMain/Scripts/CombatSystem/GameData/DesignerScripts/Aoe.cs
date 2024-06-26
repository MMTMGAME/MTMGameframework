using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

namespace DesignerScripts
{
    ///<summary>
    ///aoe的效果
    ///</summary>
    public class AoE{
        public static Dictionary<string, AoeOnCreate> onCreateFunc = new Dictionary<string, AoeOnCreate>(){
            {"CreateSightEffect", CreateSightEffect}
        };
        public static Dictionary<string, AoeOnRemoved> onRemovedFunc = new Dictionary<string, AoeOnRemoved>(){
            {"DoDamageOnRemoved", DoDamageOnRemoved},
            {"CreateAoeOnRemoved", CreateAoeOnRemoved},
            {"BarrelExplosed", BarrelExplosed}
        };
        public static Dictionary<string, AoeOnTick> onTickFunc = new Dictionary<string, AoeOnTick>(){
            {"BlackHole", BlackHole}
        };
        public static Dictionary<string, AoeOnCharacterEnter> onChaEnterFunc = new Dictionary<string, AoeOnCharacterEnter>(){
            {"DoDamageToEnterCha", DoDamageToEnterCha},
            {"AddBuffToEnterCha", AddBuffToEnterCha}
        };
        public static Dictionary<string, AoeOnCharacterLeave> onChaLeaveFunc = new Dictionary<string, AoeOnCharacterLeave>(){
            
        };
        public static Dictionary<string, AoeOnBulletEnter> onBulletEnterFunc = new Dictionary<string, AoeOnBulletEnter>(){
            {"BlockBullets", BlockBullets},
            {"SpaceMonkeyBallHit", SpaceMonkeyBallHit},
            
        };
        public static Dictionary<string, AoeOnBulletLeave> onBulletLeaveFunc = new Dictionary<string, AoeOnBulletLeave>(){
            
        };
        public static Dictionary<string, AoeTween> aoeTweenFunc = new Dictionary<string, AoeTween>(){
            {"AroundCaster", AroundCaster},
            {"SpaceMonkeyBallRolling", SpaceMonkeyBallRolling}
        };


        ///<summary>
        ///aoeTween
        ///环绕施法者旋转，参数：
        ///[0]float：距离caster的距离（单位米）
        ///[1]float：移动速度（度/秒），正负的效果是方向
        ///</summary>
        private static AoeMoveInfo AroundCaster(GameObject aoe, float t){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (aoeState == null || aoeState.caster == null) return null;
            Vector3 b = aoeState.caster.transform.position;
            
            float dis = aoeState.tweenParam.Length > 0 ? (float)aoeState.tweenParam[0] : 0;
            float degPlus = aoeState.tweenParam.Length > 1 ? (float)aoeState.tweenParam[1] : 0;
            float cDeg = degPlus * t;
            float dr = cDeg * Mathf.PI / 180; 
            
            Vector3 targetP = new Vector3(
                b.x + Mathf.Sin(dr) * dis - aoe.transform.position.x, 
                0,
                b.z + Mathf.Cos(dr) * dis - aoe.transform.position.z
            );

            //Debug.Log("Around Caster " + aoeState.GetHashCode() + " // " + b);

            return new AoeMoveInfo(MoveType.fly, targetP, Quaternion.Euler(0,cDeg % 360,0));
        }

        ///<summary>
        ///onBulletEnter
        ///消灭所有进入的敌人的子弹，参数：
        ///[0]bool：是否有抵挡次数限制
        ///来自AoeState.aoeParam的参数：
        ///["times"]int：抵消多少次
        ///</summary>
        private static void BlockBullets(GameObject aoe, List<GameObject> bullets){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;
            AoeModel am = aoeState.model;
            bool countLimited = am.onBulletEnterParams.Length > 0 ? (bool)am.onBulletEnterParams[0] : false;
            int times = aoeState.param.ContainsKey("times") ? (int)aoeState.param["times"] : 1;

            CampType ccsCamp = CampType.Unknown;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                ccsCamp = ccs.Camp;
            }

            for (int i = 0; i < bullets.Count; i++){
                BulletState bs = bullets[i].GetComponent<BulletState>();
                CampType bCamp = CampType.Unknown;
                if (bs && bs.caster){
                    ChaState bcs = bs.caster.GetComponent<ChaState>();
                    if (bcs) bCamp = bcs.Camp;
                }
                if (CombatComponent.GetRelation(ccsCamp,bCamp)!=RelationType.Friendly){
                    GameEntry.Combat.RemoveBullet(bullets[i], false);
                    GameEntry.Combat.CreateSightEffect(70000, aoe.transform.position, aoe.transform.eulerAngles.y);
                }
            }

            times -= 1;
        }

        ///<summary>
        ///aoeTween
        ///小猴设计的滚球，往前滚动，受到攻击会略微转向。参数：
        ///[0]Vector3：原始的力量
        ///来自AoeState.aoeParam的参数：
        ///["forces"]List<Vector3>：被子弹施加的力
        ///</summary>
        private static AoeMoveInfo SpaceMonkeyBallRolling(GameObject aoe, float t){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return null;

            Vector3 velocity = aoeState.tweenParam.Length > 0 ? (Vector3)aoeState.tweenParam[0] : Vector3.zero;
            velocity *= Time.fixedDeltaTime; //算的是一个tick的，所以得在这里乘一下，回头再读取的地方除一下，这是因为设计者在设计这个函数时候思考环境不同所产生的必须要的“牺牲”
            List<Vector3> forces = aoeState.param.ContainsKey("forces") ? (List<Vector3>)aoeState.param["forces"] : null;
            if (forces != null){
                for (int i = 0; i < forces.Count; i++){
                    velocity += forces[i] * Time.fixedDeltaTime;
                }
            }
            
            return new AoeMoveInfo(MoveType.fly, velocity, Quaternion.LookRotation(velocity));
        }

        ///<summary>
        ///onBulletEnter
        ///小猴设计的滚球，挨打后会吃到来自子弹的力，参数：
        ///[0]float：力的大小，米/秒
        ///</summary>
        private static void SpaceMonkeyBallHit(GameObject aoe, List<GameObject> bullets){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            float baseForce = aoeState.model.onBulletEnterParams.Length > 0 ? (float)aoeState.model.onBulletEnterParams[0] : 0;
            if (baseForce == 0) return;

            CampType side = CampType.Unknown;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                side = ccs.Camp;
            }

            if (aoeState.param.ContainsKey("forces") == false){
                aoeState.param["forces"] = new List<Vector3>();   
            }
            for (int i = 0; i < bullets.Count; i++){
                BulletState bs = bullets[i].GetComponent<BulletState>();
                CampType bSide =CampType.Unknown;
                
                if (bs){
                    if (bs.caster){
                        ChaState bcs = bs.caster.GetComponent<ChaState>();
                        if (bcs) bSide = bcs.Camp;
                    }
                    if (CombatComponent.GetRelation(side,bSide)==RelationType.Friendly){
                        Vector3 bMove = bs.velocity * baseForce;    //算了，就直接乘把，凑合凑合
                        ((List<Vector3>)aoeState.param["forces"]).Add(bMove);
                        GameEntry.Combat.RemoveBullet(bullets[i]);
                    }
                }
            }

            float scaleTo = 1 + ((List<Vector3>)aoeState.param["forces"]).Count * 0.5f;
            aoeState.radius =  scaleTo;
            aoeState.duration += 2.5f;
            aoeState.transform.localScale =Vector3.one* aoeState.radius;
        }

        ///<summary>
        ///onChaEnter
        ///对于范围内的人造成伤害（治疗得另写一个，这是严肃的），参数：
        ///[0]Damage：基础伤害
        ///[1]float：施法者攻击倍率
        ///[2]bool：对敌人有效
        ///[3]bool：对盟军有效
        ///[4]bool：挨打的人是否受伤动作
        ///[5]string：挨打者身上特效
        ///[6]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void DoDamageToEnterCha(GameObject aoe, List<GameObject> characters){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onChaEnterParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            int effect = p.Length > 5 ? (int)p[5] : 0;
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            CampType camp = CampType.Unknown;
            ChaState ccs=null;
            if (aoeState.caster){
                ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) camp = ccs.Camp;
            }

            for (int i = 0; i < characters.Count; i++){
                ChaState cs = characters[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && CombatComponent.GetRelation(camp,cs.Camp)!=RelationType.Friendly) || (toAlly == true && CombatComponent.GetRelation(camp,cs.Camp)==RelationType.Friendly))){
                    Vector3 chaToAoe = characters[i].transform.position - aoe.transform.position;
                    GameEntry.Combat.CreateDamage(
                        aoeState.caster, characters[i], 
                        damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                        0.05f, new DamageInfoTag[]{DamageInfoTag.directDamage}
                    );
                    //cs.AddBuff(new AddBuffInfo(DesingerTables.Buff.data["Poison"],aoeState.caster,cs.gameObject,1,10));
                    if (hurtAction == true) cs.AddAnimOrder(UnitAnim.AnimOrderType.Trigger, "Hurt");
                    if (effect !=0) cs.PlaySightEffect(bp, effect);
                }
            }
        }
        
        
        ///<summary>
        ///AddBuffToEnterCha
        ///对于范围内的人添加buff，参数：
        ///[0]buffId：buffId
        ///[1]stack：buff层数
        ///[2]stack：持续时间
        ///[3]SetTo：设置buff时间还是增加
        ///[4]bool：对敌人有效
        ///[5]bool：对盟军有效
        ///[6]int：挨打者身上特效Id
        ///[7]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void AddBuffToEnterCha(GameObject aoe, List<GameObject> characters){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onChaEnterParams;
            string buffId = p.Length > 0 ? (string)p[0] : "";
            if (buffId == "")
            {
                Debug.LogError("Aoe添加buff参数错误");
                return;
            }

            int stack = p.Length > 1 ? (int)p[1] : 1;
            float duration = p.Length > 2 ? (float)p[2] : 5;
            var durationSetTo = p.Length > 3 ? (bool)p[3] : true;
            bool toFoe = p.Length > 4 ? (bool)p[4] : true;
            bool toAlly = p.Length > 5 ? (bool)p[5] : false;
            int effect = p.Length > 6 ? (int)p[6] : 0;
            string bp = p.Length > 7 ? (string)p[7] : "Body";
            
            CampType camp = CampType.Unknown;
            if (aoeState.caster)
            {
                var ccs=aoeState.caster.GetComponent<ChaState>();
                if (ccs) camp = ccs.Camp;
            }

            for (int i = 0; i < characters.Count; i++){
                ChaState cs = characters[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && CombatComponent.GetRelation(camp,cs.Camp)!=RelationType.Friendly) || (toAlly == true && CombatComponent.GetRelation(camp,cs.Camp)==RelationType.Friendly))){
                    
                    cs.AddBuff(new AddBuffInfo(DesingerTables.Buff.data[buffId],aoeState.caster,cs.gameObject,stack,duration,durationSetTo));
                    if (effect !=0) cs.PlaySightEffect(bp, effect);
                }
            }
        }
        

        ///<summary>
        ///onRemoved
        ///对于范围内的人造成伤害（治疗得另写一个，这是严肃的），参数：
        ///[0]Damage：基础伤害
        ///[1]float：施法者攻击倍率
        ///[2]bool：对敌人有效
        ///[3]bool：对盟军有效
        ///[4]bool：挨打的人是否受伤动作
        ///[5]string：挨打者身上特效
        ///[6]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void DoDamageOnRemoved(GameObject aoe){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onRemovedParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            int effect = p.Length > 5 ? (int)p[5] : 0;
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            CampType ccsCamp = CampType.Unknown;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) ccsCamp = ccs.Camp;
            }

            for (int i = 0; i < aoeState.characterInRange.Count; i++){
                ChaState cs = aoeState.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && CombatComponent.GetRelation(ccsCamp,cs.Camp)!=RelationType.Friendly) || (toAlly == true && CombatComponent.GetRelation(ccsCamp,cs.Camp)==RelationType.Friendly))){
                    Vector3 chaToAoe = aoeState.characterInRange[i].transform.position - aoe.transform.position;
                    GameEntry.Combat.CreateDamage(
                        aoeState.caster, aoeState.characterInRange[i], 
                        damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                        0.05f, new DamageInfoTag[]{DamageInfoTag.directDamage}
                    );
                    if (hurtAction == true) cs.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"Hurt");
                    if (effect != 0) cs.PlaySightEffect(bp, effect);
                }
            }
        }


        ///<summary>
        ///onChaEnter
        ///鲁大师的黑洞效果
        ///</summary>
        private static void BlackHole(GameObject aoe){
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            for (int i = 0; i < ast.characterInRange.Count; i++){
                ChaState cs = ast.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false){
                    Vector3 disV = aoe.transform.position - ast.characterInRange[i].transform.position;
                    float distance = Mathf.Sqrt(Mathf.Pow(disV.x, 2) + Mathf.Pow(disV.z, 2));
                    float inTime = distance / (distance + 1.00f);   //1米是0.5秒，之后越来越大，但增幅是变小的
                    cs.AddForceMove(new MovePreorder(
                        disV * inTime, 1.00f
                    ));
                }
            }
        }

        ///<summary>
        ///OnCreate
        ///在aoe的位置上放一个视觉特效
        ///[0]string：特效的prefab，Prefab/下的路径，因为是特效。必定是一次性的特效，如果要循环播放完全可以绑定在aoe上，创建时开始播放，结束时停止。
        ///</summary>
        private static void CreateSightEffect(GameObject aoe){
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            object[] p = ast.model.onCreateParams;
            int fxTypeId = p.Length > 0 ? (int)p[0] : 0;
            GameEntry.Combat.CreateSightEffect(
                fxTypeId, aoe.transform.position, aoe.transform.eulerAngles.y
            );
        }

        ///<summary>
        ///onRemoved
        ///aoe移除的时候创建另外一个aoe
        ///[0]string: aoe的model的id
        ///[1]float：aoe的半径（米）
        ///[2]float：aoe持续时间（秒）
        ///[3]string：aoe的Tween函数名
        ///[4]object[]：aoe的Tween函数的参数
        ///[5]Dictionary(string, object)：aoeObj的参数
        ///</summary>
        private static void CreateAoeOnRemoved(GameObject aoe){
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            object[] p = ast.model.onRemovedParams;
            if (p.Length <= 0) return;
            string id = (string)p[0];
            if (id == "" || DesingerTables.AoE.data.ContainsKey(id) == false) return;
            AoeModel model = DesingerTables.AoE.data[id];
            //float radius = p.Length > 1 ? (float)p[1] : 0.01f;
            float duration = p.Length > 1 ? (float)p[1] : 0;
            string aoeTweenId = p.Length > 2 ? (string)p[2] : "";
            AoeTween tween = null;
            if (aoeTweenId != "" && DesignerScripts.AoE.aoeTweenFunc.ContainsKey(aoeTweenId)){
                tween = DesignerScripts.AoE.aoeTweenFunc[aoeTweenId];
            }
            object[] tp = new object[0];
            if (p.Length > 3) tp = (object[])p[3];
            Dictionary<string,object> ap = null;
            if (p.Length > 4) ap = (Dictionary<string, object>)p[4];
            AoeLauncher al = new AoeLauncher(
                model, ast.caster, aoe.transform.position, 
                duration, aoe.transform.rotation, tween, tp, ap
            );
            GameEntry.Combat.CreateAoE(al);
        }

        ///<summary>
        ///onRemoved
        ///炸药桶爆炸了
        ///</summary>
        private static void BarrelExplosed(GameObject aoe){
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            //new Damage(0, 50), 0.15f, true, false, true, "Effect/HitEffect_A", "Body"
            Damage baseDamage = new Damage(0, 50);
            float damageTimes = 0.15f;
            int effect = 70000;
            string bp = "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            CampType ccsCamp = CampType.Unknown;
            if (aoeState.caster){
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) ccsCamp = ccs.Camp;
            }

            for (int i = 0; i < aoeState.characterInRange.Count; i++){
                ChaState cs = aoeState.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && CombatComponent.GetRelation(ccsCamp,cs.Camp)!=RelationType.Friendly){
                    if (cs.HasTag("Barrel") == true){
                        GameEntry.Combat.CreateDamage(
                            (GameObject)aoeState.param["Barrel"], aoeState.characterInRange[i],
                            new Damage(0, 9999), 0f, 0f, new DamageInfoTag[]{DamageInfoTag.directDamage}
                        );
                    }else{
                        Vector3 chaToAoe = aoeState.characterInRange[i].transform.position - aoe.transform.position;
                        GameEntry.Combat.CreateDamage(
                            aoeState.caster, aoeState.characterInRange[i], 
                            damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                            0.05f, new DamageInfoTag[]{DamageInfoTag.directDamage}
                        );
                        cs.AddAnimOrder(UnitAnim.AnimOrderType.Trigger, "Hurt");
                        cs.PlaySightEffect(bp, effect);
                    }
                    
                }
            }
        }
    }

    
}