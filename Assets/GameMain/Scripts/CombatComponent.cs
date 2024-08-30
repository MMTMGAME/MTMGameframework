using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameMain;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;


public class CombatComponent : GameFrameworkComponent
{


   
    private static Dictionary<CampPair, RelationType> s_CampPairToRelation = new Dictionary<CampPair, RelationType>();
    private static Dictionary<KeyValuePair<CampType, RelationType>, CampType[]> s_CampAndRelationToCamps = new Dictionary<KeyValuePair<CampType, RelationType>, CampType[]>();

    
    [StructLayout(LayoutKind.Auto)]
    private struct CampPair
    {
        private readonly CampType m_First;
        private readonly CampType m_Second;

        public CampPair(CampType first, CampType second)
        {
            m_First = first;
            m_Second = second;
        }

        public CampType First
        {
            get
            {
                return m_First;
            }
        }

        public CampType Second
        {
            get
            {
                return m_Second;
            }
        }
    }
    
    static CombatComponent()
    {
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player), RelationType.Friendly);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player2), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy2), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral2), RelationType.Friendly);
        s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Unknown), RelationType.Neutral);

        s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy), RelationType.Friendly);
        s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral), RelationType.Neutral);
        s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Player2), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy2), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral2), RelationType.Hostile);

        s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral), RelationType.Neutral);
        s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Player2), RelationType.Neutral);
        s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Enemy2), RelationType.Neutral);
        s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral2), RelationType.Hostile);

        s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Player2), RelationType.Friendly);
        s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Enemy2), RelationType.Hostile);
        s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Neutral2), RelationType.Neutral);

        s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Enemy2), RelationType.Friendly);
        s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Neutral2), RelationType.Neutral);

        s_CampPairToRelation.Add(new CampPair(CampType.Neutral2, CampType.Neutral2), RelationType.Neutral);
    }
    
    public DamageManager DamageManager;
    public TimelineManager timelineManager;
    void Start()
    {
        DamageManager = GetComponentInChildren<DamageManager>();
        timelineManager=GetComponentInChildren<TimelineManager>();
    }
    
    /// <summary>
    /// 获取阵营关系
    /// </summary>
    /// <returns></returns>
    public static RelationType GetRelation(CampType first, CampType second)
    {
        if (first > second)
        {
            (first, second) = (second, first);
        }

        RelationType relationType;
        if (s_CampPairToRelation.TryGetValue(new CampPair(first, second), out relationType))
        {
            return relationType;
        }

        Log.Warning("Unknown relation between '{0}' and '{1}'.", first.ToString(), second.ToString());
        return RelationType.None;
    }
    
    /// <summary>
    /// 获取和指定具有特定关系的所有阵营。
    /// </summary>
    /// <param name="camp">指定阵营。</param>
    /// <param name="relation">关系。</param>
    /// <returns>满足条件的阵营数组。</returns>
    public static CampType[] GetCamps(CampType camp, RelationType relation)
    {
        KeyValuePair<CampType, RelationType> key = new KeyValuePair<CampType, RelationType>(camp, relation);
        CampType[] result = null;
        if (s_CampAndRelationToCamps.TryGetValue(key, out result))
        {
            return result;
        }

        // TODO: GC Alloc
        List<CampType> camps = new List<CampType>();
        Array campTypes = Enum.GetValues(typeof(CampType));
        for (int i = 0; i < campTypes.Length; i++)
        {
            CampType campType = (CampType)campTypes.GetValue(i);
            if (GetRelation(camp, campType) == relation)
            {
                camps.Add(campType);
            }
        }

        // TODO: GC Alloc
        result = camps.ToArray();
        s_CampAndRelationToCamps[key] = result;

        return result;
    }

    public void CreateDamage(GameObject attacker,GameObject victim,Damage damage,float damageDegree=0f, float criticalRate=0f, DamageInfoTag[] tags=null)
    {
        Debug.Log($"{attacker}对{victim}造成伤害：{damage.melee},{damage.bullet}，{damage.explosion},{damage.mental}",attacker.gameObject);
        DamageManager.DoDamage(attacker,victim,damage,damageDegree,criticalRate,tags);
    }
    
    /// <summary>
    /// 获取实体间的距离。
    /// </summary>
    /// <returns>实体间的距离。</returns>
    public static float GetDistance(Entity fromEntity, Entity toEntity)
    {
        Transform fromTransform = fromEntity.CachedTransform;
        Transform toTransform = toEntity.CachedTransform;
        return (toTransform.position - fromTransform.position).magnitude;
    }
    
    
    //功能函数
    

    //特效管理器
    private Dictionary<string, GameObject> sightEffect = new Dictionary<string, GameObject>();
    

    private void FixedUpdate() {
        //管理一下视觉特效，看哪些需要清楚了
        List<string> toRemoveKey = new List<string>();
        foreach(KeyValuePair<string, GameObject> se in sightEffect){
            if (se.Value == null) toRemoveKey.Add(se.Key);
        }
        for (int i = 0; i < toRemoveKey.Count; i++) sightEffect.Remove(toRemoveKey[i]);
        toRemoveKey = null;
        
    }
    

    ///<summary>
    ///创建一个子弹对象在场景上
    ///<param name="bulletLauncher">子弹发射器</param>
    ///</summary>
    public void CreateBullet(BulletLauncher bulletLauncher,GameObject[] targets=null){
        
        GameEntry.Entity.ShowBulletObj(bulletLauncher, (bulletObj) =>
        {
            //处理bulletObj的数据
            //bulletObj.transform.rotation= bulletLauncher.localRotation;
        
            bulletObj.gameObject.GetOrAddComponent<BulletState>().InitByBulletLauncher(
                bulletLauncher, 
                targets
            );
        });
    }

    ///<summary>
    ///删除一个存在的子弹Object
    ///<param name="aoe">子弹的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public void RemoveBullet(GameObject bullet, bool immediately = false){
        if (!bullet) return;
        BulletState bulletState = bullet.GetComponent<BulletState>();
        if (!bulletState) return;
        bulletState.duration = 0;
        if (immediately == true){
            if (bulletState.model.onRemoved != null){
                bulletState.model.onRemoved(bullet.gameObject);
            }
            GameEntry.Entity.HideEntity(bullet.GetComponent<Entity>());
        }
    }

    ///<summary>
    ///创建一个aoe对象在场景上
    ///<param name="aoeLauncher">aoe的创建信息</param>
    ///</summary>
    public int CreateAoE(AoeLauncher aoeLauncher){
        
        
        Debug.Log($"CreatAoe:{aoeLauncher.model.id},{aoeLauncher.caster}");
        return GameEntry.Entity.ShowAoeObj(aoeLauncher, (aoeObj) =>
        {
            aoeObj.GetComponent<AoeState>().InitByAoeLauncher(aoeLauncher);
        });
    }

  

    ///<summary>
    ///删除一个存在的aoeObject
    ///<param name="aoe">aoe的GameObject</param>
    ///<param name="immediately">是否当场清除，如果false，就是把时间变成0</param>
    ///</summary>
    public void RemoveAoE(GameObject aoe, bool immediately = false){
        if (!aoe) return;
        AoeState aoeState = aoe.GetComponent<AoeState>();
        if (!aoeState) return;
        aoeState.duration = 0;
        if (immediately == true){    
            if (aoeState.model.onRemoved != null){
                aoeState.model.onRemoved(aoe);
            }
            GameEntry.Entity.HideEntity(aoe.GetComponent<Entity>());
        }
    }

    ///<summary>
    ///创建一个视觉特效在场景上
    ///<param name="typeId">typeId</param>
    ///<param name="pos">创建的位置</param>
    ///<param name="degree">角度</param>
    ///<param name="key">特效的key，如果重复则无法创建，删除的时候也有用，空字符串的话不加入管理</param>
    ///<param name="loop">是否循环，循环的得手动remove</param>
    ///</summary>
    public void CreateSightEffect(int typeId, Vector3 pos, float degree, string key = "", bool loop = false,float duration=1){
        if (sightEffect.ContainsKey(key) == true) return;    //已经存在，加不成
        
        Debug.Log("CreateSightEffect");
        GameEntry.Entity.ShowModelObj(typeId,pos,Quaternion.identity, (effectGO) =>
        {
            
            effectGO.transform.SetParent(transform);
            
            effectGO.transform.RotateAround(effectGO.transform.position, Vector3.up, degree);
            if (!effectGO) return;
            SightEffect se = effectGO.AddComponent<SightEffect>();
            if (!se){
                GameEntry.Entity.HideEntity(effectGO);
                return;
            }

            se.duration = duration;
            if (loop == false){
                effectGO.AddComponent<EntityRemover>().duration = se.duration;
            }

            if (key != "")  sightEffect.Add(key, effectGO.gameObject);
        });
    }

    ///<summary>
    ///删除一个视觉特效在场景上
    ///<param name="key">特效的key</param>
    ///</summary>
    public void RemoveSightEffect(string key){
        if (sightEffect.ContainsKey(key) == false) return;
        //Destroy(sightEffect[key]);
            GameEntry.Entity.HideEntity(sightEffect[key].GetComponent<Entity>());
        sightEffect.Remove(key);
    }

    // ///<summary>
    // ///创建一个角色到场上
    // ///<param name="prefab">特效的prefab文件夹，约定就在Prefabs/Character/下，所以路径不应该加这段</param>
    // ///<param name="unitAnimInfo">角色的动画信息</param>
    // ///<param name="side">所属阵营</param>
    // ///<param name="pos">创建的位置</param>
    // ///<param name="degree">角度</param>
    // ///<param name="baseProp">初期的基础属性</param>
    // ///<param name="tags">角色的标签，分类角色用的</param>
    // ///</summary>
    // public GameObject CreateCharacter(string prefab, int side, Vector3 pos, ChaProperty baseProp, float degree, string unitAnimInfo = "Default_Gunner", string[] tags = null){
    //     GameObject chaObj = CreateFromPrefab("Character/CharacterObj");
    //     //Vector3 playerPos = SceneVariants.map.GetRandomPosForCharacter(new RectInt(0, 0, SceneVariants.map.MapWidth(), SceneVariants.map.MapHeight()));
    //     //cha.AddComponent<PlayerController>().mainCamera = Camera.main; //敌人没有controller
    //     ChaState cs = chaObj.GetComponent<ChaState>();
    //     if (cs){
    //         cs.InitBaseProp(baseProp);
    //         cs.side = side;
    //         Dictionary<string, AnimInfo> aInfo = new Dictionary<string, AnimInfo>();
    //         if (unitAnimInfo != "" && DesingerTables.UnitAnimInfo.data.ContainsKey(unitAnimInfo)){
    //             aInfo = DesingerTables.UnitAnimInfo.data[unitAnimInfo];
    //         }
    //         cs.SetView(CreateFromPrefab("Character/" + prefab), aInfo);
    //         if (tags != null) cs.tags = tags;
    //     }
    //     
    //     chaObj.transform.position = pos;
    //     chaObj.transform.RotateAround(chaObj.transform.position, Vector3.up, degree);
    //     return chaObj;
    // }

    public void CreateCharacter(int typeId,CampType campType,Vector3 pos,ChaProperty baseProp,string[] tags = null,AddBuffInfo[] addBuffs=null,GameObject caster=null)
    {
        GameEntry.Entity.ShowBattleUnit(new BattleUnitData(GameEntry.Entity.GenerateSerialId(),typeId,campType)
        {
            Position = pos,
            OnShowCallBack = (c) =>
            {
                if (c is BattleUnit battleUnit)
                {
                    battleUnit.chaState.InitBaseProp(baseProp);
                    battleUnit.chaState.tags = tags;
                    
                    for (int i = 0; i < addBuffs.Length; i++){
                        addBuffs[i].caster = caster;
                        addBuffs[i].target = battleUnit.gameObject;
                        battleUnit.chaState.AddBuff(addBuffs[i]);
                    }
                }
            }
        });
    }
    
    public void CreateTimeline(TimelineModel timelineModel, GameObject caster, object source){
        timelineManager.AddTimeline(timelineModel, caster, source);
    }
    public void CreateTimeline(TimelineObj timeline){
        timelineManager.AddTimeline(timeline);
    }
}
