using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class AnimEventListener : MonoBehaviour
{
    private BattleUnit battleUnit;

    private ChaState chaState;

    private Rigidbody rb;
    public void Init(BattleUnit battleUnit)
    {
        this.battleUnit = battleUnit;
        chaState = this.battleUnit.chaState;
        rb = GetComponent<Rigidbody>();
    }
    
    
    public void TriggerAnimationEvent(string actionInfoStr)
    {
        var arr = actionInfoStr.Split(",");

        var command = arr[0].ToLower();
        switch (command)
        {
            case "createaoe"://第一个参数为Id,第二个参数为pointKey，第三个参数为持续时间，第四个参数为缩放
                var aoeId = arr[1];
                var pointTrans = chaState.GetBindManager().GetBindPointByKey(arr[2]);
                var aoeLauncher = new AoeLauncher(DesingerTables.AoE.data[aoeId], gameObject,
                    pointTrans.transform.position, float.Parse(arr[3]), pointTrans.transform.rotation,float.Parse(arr[4]));
                if (arr.Length > 5)
                {
                    var paramStr=arr[5].Split("|");//额外参数
                    foreach (var kv in paramStr)
                    {
                        var kvArr = kv.Split(":");
                        if (kvArr.Length == 2)
                        {
                            var k = kvArr[0];
                            var v = kvArr[1];
                            aoeLauncher.param.Add(k,v);
                        }
                    }
                }
               
                GameEntry.Combat.CreateAoE(aoeLauncher);
            break;
            
            case "addbuff":
                var buffId = arr[1];
                var addBuffInfo = new AddBuffInfo();
                addBuffInfo.caster = gameObject;
                addBuffInfo.target = gameObject;
                addBuffInfo.buffModel = DesingerTables.Buff.data[buffId];
                addBuffInfo.duration = float.Parse(arr[2]);
                addBuffInfo.durationSetTo = bool.Parse(arr[3]);
                addBuffInfo.addStack = 1;
                chaState.AddBuff(addBuffInfo);
            break;
            
            case "removebuff":
                var toRemoveBuffId = arr[1];
                var abi = new AddBuffInfo
                {
                    //移除时Caster会被作为筛选条件寻找由Caster释放的buff进行移除，所以caster一般不设置，否则很可能找不到要移除的buff
                    target = gameObject,
                    buffModel = DesingerTables.Buff.data[toRemoveBuffId],
                    duration = 0,
                    durationSetTo = true,
                    addStack = 1
                };
                if(chaState==null)//可能还没来得及初始化
                    break;
                chaState.AddBuff(abi);
                //Debug.LogError("移除Buff:"+toRemoveBuffId);
            break;
            
            case "jump":
                //注意，角色精灵看着朝右的时候角色坐标实际是朝向z轴的。
                bool resetVelocity = arr.Length > 3 ? bool.Parse(arr[3]) : false;
                if (resetVelocity)
                {
                    rb.velocity=new Vector3(0,0,0);//重置刚体速度
                    //rb.velocity=new Vector3(0,Physics.gravity.y ,0);//重置刚体速度
                }
                bool addBuff=arr.Length>4? bool.Parse(arr[4]) : false;
                string lieDownDurationParam = arr.Length > 5 ? arr[5] : null;
                CombatUtility.KnockOff(rb, gameObject,gameObject,transform.right,float.Parse(arr[1]),float.Parse(arr[2]),false,false,addBuff,null,new Dictionary<string,object>()
                {
                    {"LieDownDuration",lieDownDurationParam}
                });
                break;
            
            case "addtimeline":
                var timelineId=arr[1];
                var timeline = DesingerTables.Timeline.data[timelineId];
                GameEntry.Combat.timelineManager.AddTimeline(timeline,gameObject,null);
                break;
            
            case "playsound":
                var soundId=arr[1];
                var soundTrans = arr.Length>2? arr[2]:"Body";

                GameEntry.Sound.PlaySound(int.Parse(soundId), chaState.GetBindManager().GetBindPointByKey(soundTrans).transform.position);
                break;
            
            case "playuisound":
                var uisoundId=arr[1];
                
                GameEntry.Sound.PlayUISound(int.Parse(uisoundId));
                break;
            
            case "playrandomsound":
                // 将音效 ID 分割成数组
                var soundIds = arr[1].Split('|');
    
                // 解析绑定点的键
                var bindPointKey = arr.Length>2?arr[2]:"Body";
    
                // 获取绑定点的 Transform
                var soundTrans1 = chaState.GetBindManager().GetBindPointByKey(bindPointKey);
    
                // 确保绑定点存在
                if (soundTrans1 != null)
                {
                    // 随机选择一个音效 ID
                    var random = new System.Random();
                    var randomIndex = random.Next(soundIds.Length);
                    var randomSoundId = int.Parse(soundIds[randomIndex]);
        
                    // 播放随机选择的音效
                    GameEntry.Sound.PlaySound(randomSoundId, soundTrans1.transform.position);
                }
                else
                {
                    Debug.LogWarning("Sound bind point not found: " + bindPointKey);
                }
                break;
            
            case "playrandomeffect":
                // 将特效 ID 分割成数组
                var effectIds = arr[1].Split('|');

                // 解析绑定点的键
                var effectBindPointKey = arr.Length > 2 ? arr[2] : "Pivot";

                // 获取绑定点的 Transform
                var effectTrans = chaState.GetBindManager().GetBindPointByKey(effectBindPointKey);

                // 确保绑定点存在
                if (effectTrans != null)
                {
                    // 随机选择一个特效 ID
                    var random = new System.Random();
                    var randomEffectIndex = random.Next(effectIds.Length);
                    var randomEffectId = int.Parse(effectIds[randomEffectIndex]);

                    // 播放随机选择的特效
                    //GameEntry.Entity.ShowEffect(randomEffectId, effectTrans.transform.position);
                    if(randomEffectId==7001){Debug.LogError("7001"+gameObject.name);}
                    GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(),randomEffectId)
                    {
                        Position = effectTrans.transform.position,
                        Rotation = effectTrans.transform.rotation,
                    });
                }
                else
                {
                    Debug.LogWarning("Effect bind point not found: " + effectBindPointKey);
                }
                break;

            
        }
    }
}
