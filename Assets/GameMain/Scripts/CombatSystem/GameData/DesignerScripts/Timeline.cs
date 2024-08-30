using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DG.Tweening;
using GameMain;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;
using Vector3 = UnityEngine.Vector3;

namespace DesignerScripts
{
    public class Timeline{
        public static Dictionary<string, TimelineEvent> functions = new Dictionary<string, TimelineEvent>(){
            {"CasterPlayAnim", CasterPlayAnim},
            {"CasterForceMove", CasterForceMove},
            {"SetCasterControlState", SetCasterControlState},
            {"PlaySightEffectOnCaster", PlaySightEffectOnCaster},
            {"PlayEffectInWorld", PlayEffectInWorld},
            {"StopSightEffectOnCaster", StopSightEffectOnCaster},
            {"SetBindPointChildrenActive", SetBindPointChildrenActive},
            {"Play2DSound", Play2DSound},
            {"Play3DSound", Play3DSound},
            {"FireBullet", FireBullet},
            {"Hiraijin", Hiraijin},
           
            {"CasterImmune", CasterImmune},
            {"CreateAoE", CreateAoE},
            {"AddBuffToCaster", AddBuffToCaster},
            {"CasterAddAmmo", CasterAddAmmo},
            {"SummonCharacter", SummonCharacter},
            {"LightAttack", LightAttack},
            {"HeavyAttack", HeavyAttack},
           
          
            
        };

        private static readonly int LightAttackTriggerAble = Animator.StringToHash("LightAttackTriggerAble");
        private static readonly int HeavyAttackTriggerAble = Animator.StringToHash("HeavyAttackTriggerAble");
        private static readonly int Speed = Animator.StringToHash("Speed");

        ///<summary>
        ///在Caster的某个绑点(Muzzle/Head/Body)上发射一个子弹出来
        ///<param name="args">总共3个参数：
        ///[0]BulletLauncher：子弹发射信息，其中caster和position是需要获得后该写的，degree则需要加上角色的转向
        ///[1]string：角色身上绑点位置，默认Muzzle
        ///</param>
        ///</summary>
        private static void FireBullet(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                BulletLauncher bLauncher = (BulletLauncher)args[0];
                UnitBindPoint ubp = ubm.GetBindPointByKey(args.Length > 1 ? (string)args[1] : "Muzzle");
                int muzzleId = args.Length > 2 ? (int)args[2] : 0;
                int soundId = args.Length > 3 ? (int)args[3] : 0;
                if (!ubp) return;

                bLauncher.caster = tlo.caster;
                
                
                bLauncher.firePosition = ubp.gameObject.transform.position;
                
                GameEntry.Combat.CreateBullet(bLauncher);
                if (muzzleId > 0)
                {
                    GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),muzzleId)
                    {
                        Position = ubp.transform.position,
                        Rotation = ubp.transform.rotation,
                    });
                }

                if (soundId > 0)
                {
                    GameEntry.Sound.PlaySound(soundId, ubp.transform.position);
                }
                
            }
        }

        private static void Hiraijin(TimelineObj tlo, params object[] args)
        {
            if (tlo.caster)
            {
                var variables = tlo.caster.GetComponent<Variables>();
                var target = variables.declarations.Get<BattleUnit>("Target");

                var fxId = args.Length > 0 ? (int)args[0] : 70000;
                var pointId = args.Length > 1 ? (string)args[1] : "Body";
                if (target != null)
                {
                    var pointTrans = tlo.caster.GetComponent<UnitBindManager>().GetBindPointByKey(pointId);
                    GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),fxId)
                    {
                        Position = pointTrans.transform.position,
                        KeepTime = 1f
                    });
                    //提前决定好位置便于玩家躲避
                    var teleportPos=AIUtility.RandomNavMeshPos(target.transform.TransformPoint(-2, 0, 0), 0.5f)+Vector3.up*2f;

                    GameEntry.Timer.AddOnceTimer(500, () =>
                    {
                        if (tlo.caster)
                        {
                            var rb = tlo.caster.GetComponent<Rigidbody>();
                            if (rb)
                            {
                                rb.velocity=Vector3.zero;
                            }
                            
                            
                            tlo.caster.transform.position = teleportPos;
                            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),fxId)
                            {
                                Position = teleportPos,
                                KeepTime = 1f
                            });
                        }
                      
                    });
                }
            }

        }

        

      
        ///<summary>
        ///在caster=timeline.caster的面前位置aoe
        ///<param name="args">总共3个参数：
        ///[0]AoeLauncher：aoe的发射器，caster在这里被重新赋值，position则作为增量加给现在的角色坐标
        ///[1]bool：true=局部坐标，false=世界坐标
        ///</param>
        ///</summary>
        private static void CreateAoE(TimelineObj tlo, params object[] args){
            if (args.Length <= 0) return;
            
            if (tlo.caster){
                UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                if (!ubm) return;

                AoeLauncher aLauncher = ((AoeLauncher)args[0]).Clone(); //必须克隆出来，去掉ref属性，使之变成临时的属性
                bool relativePos = args.Length > 1 ? (bool)args[1] : true;
                
                aLauncher.caster = tlo.caster;
                aLauncher.rotation = tlo.caster.transform.rotation * aLauncher.rotation;

               
                if(relativePos)
                    aLauncher.position = aLauncher.caster.transform.TransformPoint( aLauncher.position);
                
                aLauncher.tweenParam = new object[]{
                    aLauncher.caster.transform.forward
                };

                GameEntry.Combat.CreateAoE(aLauncher);
            }
        }

        ///<summary>
        ///timelien的焦点角色播放某个动作，是否是跳转到那个动作一直播放还是会回到站立，这取决于animator里面做的，我也无能为力
        ///<param name="args">总共3个参数：
        ///[0]string：是要播放的动画
        ///[1]bool：是否要取得动画的方向，如果不要就直接用预设的值了
        ///[2]bool：是否启用当前正在进行的面向和移动角度，如果false或者缺省了，就代表启用timelineObj中储存的（开始时的）
        ///</param>
        ///</summary>
        private static void CasterPlayAnim(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                UnitAnim.AnimOrderType orderType =
                    args.Length > 1 ? (UnitAnim.AnimOrderType)args[0] : UnitAnim.AnimOrderType.Trigger;
                
                string animName = args.Length >= 2 ? (string)(args[1]) : "";

                if (animName == "") return;
                
                object value=args.Length>=3 ? (object)args[2] : null;

                
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                   
                    
                    cs.AddAnimOrder(orderType,animName,value); 
                }
            }
        }

        //抓举物体
       

        private static void LightAttack(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs)
                {
                    var animator = cs.GetAnimator();

                    if (animator.GetBool(LightAttackTriggerAble))//仅在可输入阶段才能Trigger
                    {
                        cs.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"LightAttack");
                    }
                }
            }
        }
        
        private static void HeavyAttack(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs)
                {
                    var animator = cs.GetAnimator();
                    
                    
                    if (animator.GetBool(HeavyAttackTriggerAble))//仅在可输入阶段才能Trigger
                    {
                        
                        cs.AddAnimOrder(UnitAnim.AnimOrderType.Trigger,"HeavyAttack");
                    }
                }
            }
        }

        ///<summary>
        ///timeline的焦点角色强制进行移动
        ///<param name="args">总共4个参数：
        ///[0]float：想要强行移动的距离，单位：米。
        ///[1]float：在多久内完成这个移动，单位：秒。这是匀速直线移动的。
        ///[2]float：基于角色移动方向或者面向（取决于[2]），获得一个基础的移动角度偏移量。
        ///[3]bool：是否要基于角色移动方向，如果不是，就是基于角色的面朝方向。
        ///[4]bool：如果启用面向，是否启用正在进行的，而非timeline创建时的，缺省或者false代表启用timeline创建时产生的
        ///</param>
        ///</summary>
        private static void CasterForceMove(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
               
                float dis = args.Length >= 1 ? (float)args[0] : 0.00f;
                float inSec = (args.Length >= 2 ? (float)args[1] : 0.00f) / tlo.timeScale;  //移动速度可得手动设置倍速
                Quaternion rotation=(args.Length >= 3 ? (Quaternion)args[2] : Quaternion.identity);
                bool local=(args.Length >= 4 ? (bool)args[3] : true);
                if (cs)
                {
                    var velocity = ((cs.transform.rotation* rotation) * Vector3.forward).normalized * dis;
                    //Debug.LogError("SwimDash技能Velo:"+velocity);
                    if(!local)
                        // 世界坐标移动
                        velocity = (rotation * Vector3.forward).normalized * dis;
                    cs.AddForceMove(new MovePreorder(velocity, inSec));
                }
            }
        }
        
        
        private static void Play2DSound(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                var soundId = args[0];
                GameEntry.Sound.PlaySound((int)soundId);
            }
        }
        
        private static void Play3DSound(TimelineObj tlo, params object[] args){
            if (tlo.caster)
            {
                var soundId = args[0];
                var pointKey = args.Length > 1 ? args[1] : "Body";

                var bindManager = tlo.caster.gameObject.GetComponent<UnitBindManager>();
                if (bindManager)
                {
                    var trans = bindManager.GetBindPointByKey((string)pointKey).transform;
                    GameEntry.Sound.PlaySound((int)soundId, trans.position);
                }

            }
        }
        

        ///<summary>
        ///设置timeline的焦点角色的ChaControlState
        ///<param name="args">总共3个参数：
        ///[0]bool：可否移动，如果得不到参数，就保持原值。
        ///[1]bool：可否转身，如果得不到参数，就保持原值。
        ///[2]bool：可否释放技能，如果得不到参数，就保持原值。
        ///</param>
        ///</summary>
        private static void SetCasterControlState(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    if (args.Length >= 1) cs.timelineControlState.canMove = (bool)args[0];
                    if (args.Length >= 2) cs.timelineControlState.canRotate = (bool)args[1];
                    if (args.Length >= 3) cs.timelineControlState.canUseSkill = (bool)args[2];
                }
            }
        }

        ///<summary>
        ///在timeline焦点角色身上播放一个视觉特效
        ///<param name="args">总共4个参数：
        ///[0]string：要播放特效的绑点
        ///[1]string：特效的文件名，位于Prafabs/下
        ///[2]string：特效的key，用于之后删除的
        ///[3]bool：是否循环播放特效（循环就要手动删除）
        ///</param>
        ///</summary>
        private static void PlaySightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    int effectTypeId = args.Length >= 2 ? (int)args[1] : 0;
                    string effectKey = args.Length >= 3 ? (string)args[2] : Random.value.ToString();
                    bool loop = args.Length >= 4 ? (bool)args[3] : false;
                    cs.PlaySightEffect(bindPointKey, effectTypeId, effectKey, loop);
                }
            }
        }
        
        private static void PlayEffectInWorld(TimelineObj tlo, params object[] args)
        {
            var typeId = args[0];
            var bindManager = tlo.caster.GetComponent<UnitBindManager>();
            var pos = ((string)args[1]==""?tlo.caster.transform.position:bindManager.GetBindPointByKey((string)args[1]).transform.position);
            var rotation = args[2];
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),(int)typeId)
            {
                Position = (Vector3)pos,
                Rotation = (Quaternion)rotation
            });
        }

        ///<summary>
        ///在timeline焦点角色身上关闭一个视觉特效
        ///<param name="args">总共2个参数：
        ///[0]string：要关闭的特效所处绑点
        ///[1]string：特效的key，创建时产生的
        ///</param>
        ///</summary>
        private static void StopSightEffectOnCaster(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                    string effectKey = args.Length >= 2 ? (string)args[1] : "";
                    if (effectKey == "") return;
                    cs.StopSightEffect(bindPointKey, effectKey);
                }
            }
        }
        
        /// <summary>
        /// 由于BindPoints是通过GetComponents
        /// </summary>
        /// <param name="tlo"></param>
        /// <param name="args"></param>
        private static void SetBindPointChildrenActive(TimelineObj tlo, params object[] args){
            if (tlo.caster){
                ChaState cs = tlo.caster.GetComponent<ChaState>();
                if (cs){
                    string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
                   
                    bool targetStatus = args.Length >= 2 ? (bool)args[1] : false;
                    cs.SetBindPointChildrenActive(bindPointKey,targetStatus);
                }
            }
        }

        ///<summary>
        ///设置timeline的caster身上的无敌时间
        ///<param name="args">总共1个参数：
        ///[0]float：无敌的时间，单位：秒
        ///</param>
        ///</summary>
        private static void CasterImmune(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    float immT = (float)args[0];
                    cs.SetImmuneTime(immT);
                }
            }
        }

        ///<summary>
        ///修改timeline的caster身上的子弹数量
        ///[0]int：需要添加的数量，负数就是减少了
        ///</summary>
        private static void CasterAddAmmo(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster){
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs && args.Length > 0){
                    int modCount = (int)args[0];
                    cs.ModResource(new ChaResource(cs.resource.hp, modCount + cs.resource.mp, cs.resource.stamina));
                }
            }
        }

        ///<summary>
        ///给timeline的caster添加一个buff
        ///[0]AddBuffInfo：如何添加一个buff，其中caster和carrier都会是timeline.caster本身
        ///</summary>
        private static void AddBuffToCaster(TimelineObj timelineObj, params object[] args){
            if (timelineObj.caster && args.Length > 0){
                AddBuffInfo abi = (AddBuffInfo)args[0];
                abi.caster = timelineObj.caster;
                abi.target = timelineObj.caster;
                ChaState cs = timelineObj.caster.GetComponent<ChaState>();
                if (cs){
                    cs.AddBuff(abi);
                }
            }
        }

        ///<summary>
        ///创建一个buff给角色，并且给他添加一系列buff
        ///[0]string： prefab, 
        ///[1]ChaProperty: baseProp, 
        ///[2]float: degree, 
        ///[3]string: unitAnimInfo = "Default_Gunner", 
        ///[4]string[]: tags = null
        ///[5]AddBuffInfo[]: 开始时候要添加的buff
        ///</summary>
        private static void SummonCharacter(TimelineObj timelineObj, params object[] args){
            if (!timelineObj.caster) return;

            int id = args.Length > 0 ? (int)args[0] : 0;
            CampType campType = args.Length > 1 ? (CampType)args[1] : CampType.Unknown;
            Vector3 pos = timelineObj.caster.transform.position;
            ChaProperty cp = args.Length >  2? (ChaProperty)args[2] : new ChaProperty(100, 1); 
           
            string[] tags = args.Length > 3 ? (string[])args[3] : null;
            AddBuffInfo[] addBuffs = args.Length > 4 ? (AddBuffInfo[])args[4] : new AddBuffInfo[0];

           GameEntry.Combat.CreateCharacter(id, campType, pos, cp,  tags,addBuffs,timelineObj.caster);
           
           
        }

    }
}