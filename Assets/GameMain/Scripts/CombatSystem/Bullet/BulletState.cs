using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.Serialization;
using Log = UnityGameFramework.Runtime.Log;


///<summary>
///子弹的“状态”，用来管理当前应该怎么移动、应该怎么旋转、应该怎么播放动画的。
///是一个角色的总的“调控中心”。
///</summary>
public class BulletState:MonoBehaviour{
    ///<summary>
    ///这是一颗怎样的子弹
    ///</summary>
    public BulletModel model;
    
    ///<summary>
    ///要发射子弹的这个人的gameObject，这里就认角色（拥有ChaState的）
    ///当然可以是null发射的，但是写效果逻辑的时候得小心caster是null的情况
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///子弹发射时候，caster的属性，如果caster不存在，就会是一个ChaProperty.zero
    ///在一些设计中，比如wow的技能中，技能效果是跟发出时候的角色状态有关的，之后即使获得或者取消了buff，更换了装备，数值一样不会受到影响，所以得记录这个释放当时的值
    ///</summary>
    public ChaProperty propWhileCast = ChaProperty.zero;

    ///<summary>
    ///发射的角度，单位：角度，如果useFireDegreeForever == true，那就得用这个角度来获得当前飞行路线了
    ///</summary>
    public Quaternion fireRotation;

    ///<summary>
    ///子弹的初速度，单位：米/秒，跟Tween结合获得Tween得到当前移动速度
    ///</summary>
    public float speed;

    ///<summary>
    ///子弹的生命周期，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///子弹已经存在了多久了，单位：秒
    ///毕竟duration是可以被重设的，比如经过一个aoe，生命周期减半了
    ///</summary>
    public float timeElapsed = 0;

    ///<summary>
    ///子弹的轨迹函数
    ///<param name="t">子弹飞行了多久的时间点，单位秒。</param>
    ///<return>返回这一时间点上的速度和偏移，Vector3就是正常速度正常前进</return>
    ///</summary>
    public BulletTween tween = null;

    ///<summary>
    ///本帧的移动
    ///</summary>
    private Vector3 moveForce = new Vector3();

    ///<summary>
    ///本帧的移动信息
    ///</summary>
    public Vector3 velocity{
        get{return moveForce;}
    }

    ///<summary>
    ///子弹的移动轨迹是否严格遵循发射出来的角度,为true的话子弹按照初始方向移动，否则子弹根据子弹当前朝向移动
    ///</summary>
    public bool useFireDegreeForever = false;

    ///<summary>
    ///子弹命中纪录
    ///</summary>
    public List<BulletHitRecord> hitRecords = new List<BulletHitRecord>();

    ///<summary>
    ///子弹创建后多久是没有碰撞的，这样比如子母弹之类的，不会在创建后立即命中目标，但绝大多子弹还应该是0的
    ///单位：秒
    ///</summary>
    public float canHitAfterCreated = 0;

    ///<summary>
    ///子弹正在追踪的目标，不太建议使用这个，最好保持null
    ///</summary>
    public GameObject followingTarget = null;

    ///<summary>
    ///子弹传入的参数，逻辑用的到的临时记录
    ///</summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();
    


    ///<summary>
    ///还能命中几次
    ///</summary>
    public int hp = 1;

    private MoveType moveType;
    private bool smoothMove;

    private UnitRotate unitRotate;
    private UnitMove unitMove;
    //private GameObject viewContainer;

    private Rigidbody rb;
    private Collider[] colliders;

    private bool hitCaster;
    private void Start() {
        
        synchronizedUnits();
    }


    private bool m_HitedObstacle;
    ///<summary>
    ///子弹是否碰到了碰撞
    ///</summary>
    public bool HitedObstacle{
        get
        {
            return m_HitedObstacle;
        }
        set
        {
            m_HitedObstacle = value;
        }
    }

    ///<summary>
    ///传入的mf是偏移量，函数中现根据是否使用初始角度和elpasedTime计算初始移动方向，然后把mf作为偏移加载初始移动方向上，最后再进行移动，
    /// 我推测这个偏移量可以在之后用来制作散弹之类的地方派上用场。
    ///</summary>
    public void SetMoveForce(Vector3 mf){
        this.moveForce = mf;
        
       
        
        Quaternion baseMoveVec=(useFireDegreeForever == true ||
                            timeElapsed <= 0     //还是那个问题，unity的动画走的是update，所以慢了，旋转没转到预设角度，fireRotation
            ) ? fireRotation : transform.rotation; //欧拉获得的是角度

        moveForce = baseMoveVec * this.moveForce;
            
        moveForce *= speed;
        Debug.DrawRay(transform.position,moveForce*10,Color.red);
        unitMove.MoveBy(moveForce);
        if(moveForce!=Vector3.zero)
            unitRotate.RotateTo(Quaternion.LookRotation(moveForce));
    }

   

    ///<summary>
    ///根据BulletLauncher初始化这个数据
    ///<param name="bullet">bulletLauncher</param>
    ///<param name="targets">子弹允许跟踪的全部目标，在这里根据脚本筛选</param>
    ///</summary>
    public void InitByBulletLauncher(BulletLauncher bullet, GameObject[] targets){
        this.model = bullet.model;
        this.caster = bullet.caster;
        if (this.caster && caster.GetComponent<ChaState>()){
            this.propWhileCast = caster.GetComponent<ChaState>().property;
        }
        this.fireRotation = caster.transform.rotation * bullet.localRotation;
        transform.rotation = this.fireRotation;
        this.speed = bullet.speed;
        this.duration = bullet.duration;
        this.timeElapsed = 0;
        this.tween = bullet.tween;
        this.useFireDegreeForever = bullet.useFireDegreeForever;
        this.canHitAfterCreated = bullet.canHitAfterCreated;
        this.hitCaster = bullet.hitCaster;
        this.smoothMove = !bullet.model.removeOnObstacle;
        this.moveType = bullet.model.moveType;
        this.hp = bullet.model.hitTimes;

        this.param = new Dictionary<string, object>();
        if (bullet.param != null){
            foreach(KeyValuePair<string, object> kv in bullet.param){
                this.param.Add(kv.Key, kv.Value);
            }
        }
        
        synchronizedUnits();

        this.rb.useGravity = bullet.model.useGravity;
        foreach (var c in this.colliders)
        {
            c.isTrigger = bullet.model.isTrigger;
        }
       
        

        //把视觉特效补充给bulletObj
        // if (bullet.model.entityTypeId != 0)
        // {
        //     GameEntry.Entity.ShowModelObj(bullet.model.entityTypeId,Vector3.zero, Quaternion.identity, (bulletEffect) =>
        //     {
        //         //bulletEffect.transform.SetParent(transform);
        //         GameEntry.Entity.AttachEntity(bulletEffect.Entity,GetComponent<Entity>().Entity,transform);
        //         bulletEffect.transform.localPosition =Vector3.zero;
        //         bulletEffect.transform.localRotation = Quaternion.identity;
        //     } );
        // }

        // this.gameObject.transform.position = new Vector3(
        //     this.gameObject.transform.position.x,
        //     0,
        //     this.gameObject.transform.position.z
        // );

        
        this.followingTarget = bullet.targetFunc == null ? null :
            bullet.targetFunc(this.gameObject, targets);

        this.HitedObstacle = false;//对象池状态重置
        
        
    }

    //同步一下unitMove和自己的一些状态
    private void synchronizedUnits(){
        if (!unitRotate) unitRotate = gameObject.GetOrAddComponent<UnitRotate>();
        if (!unitMove)  unitMove = gameObject.GetOrAddComponent<UnitMove>();
        

        colliders = gameObject.GetComponentsInChildren<Collider>();
        rb = gameObject.GetOrAddComponent<Rigidbody>();
        rb.collisionDetectionMode =CollisionDetectionMode.Continuous;

        if (colliders == null || colliders.Length==0)
        {
            Log.Warning($"注意,{gameObject.name}没有配置碰撞体");
        }

        if (rb == null)
        {
            Log.Warning($"注意,{gameObject.name}没有配置Rigidbody");
        }
    }

    public void SetMoveType(MoveType toType){
        this.moveType = toType;
        synchronizedUnits();
    }

    ///<summary>
    ///判断子弹是否还能击中某个GameObject
    ///<param name="target">目标gameObject</param>
    ///</summary>
    public bool CanHit(GameObject target){
        if (canHitAfterCreated > 0) return false;
        for (int i = 0; i < this.hitRecords.Count; i++){
            if (hitRecords[i].target == target){
                return false;
            }
        }
        
        ChaState cs = target.GetComponent<ChaState>();
        if (cs && cs.immuneTime > 0) return false;

        return true;
    }

    public bool CanHit(ChaState chaState)
    {
        
        if (!hitCaster && caster == chaState.gameObject)
        {
            Debug.Log("击中自己，跳过");
            return false;
        }
        
        for (int i = 0; i < this.hitRecords.Count; i++){
            if (hitRecords[i].target == chaState.gameObject){
                return false;
            }
        }
        
        if (chaState && chaState.immuneTime > 0) return false;

        return true;
    }

    

    ///<summary>
    ///添加命中纪录
    ///<param name="target">目标GameObject</param>
    ///</summary>
    public void AddHitRecord(GameObject target){
        hitRecords.Add(new BulletHitRecord(
            target,
            this.model.sameTargetDelay
        ));
    }

    

    private void FixedUpdate()
    {
        float timePassed = Time.fixedDeltaTime;
        
        
        if(hp<=0)//hp表示的是击中次数，小于等于0表示击中次数用完了
            return;
        
        //刚刚创建
        if (timeElapsed <= 0 && model.onCreate != null){
            model.onCreate(gameObject);
        }
        
        //处理子弹命中记录
        for (int i = 0; i < hitRecords.Count; i++)
        {
            hitRecords[i].timeToCanHit -= timePassed;//同一个单位子弹的命中间隔
            if (hitRecords[i].timeToCanHit <= 0 || hitRecords[i].target == null)
            {
                hitRecords.RemoveAt(i);
                i--;
            }
        }
        
        //处理子弹移动
        SetMoveForce(tween == null ? Vector3.forward : tween(timeElapsed, gameObject, followingTarget));
        
        //处理子弹诞生后多久可以碰撞
        if (canHitAfterCreated > 0) {
            canHitAfterCreated -= timePassed;  
        }
        
        //duration处理
        duration -= timePassed;
        timeElapsed += timePassed;
        if (duration <=0 || HitedObstacle)//到期或者碰到了障碍物就该销毁了，障碍物是指除了chaObj外的碰撞体
        {
            if (model.onRemoved != null)
            {
                model.onRemoved(gameObject);
            }
            GameEntry.Entity.HideEntity(GetComponent<Entity>());
        }
        
       
    }

    #region 碰撞处理,根据设置的trigger和rigidbody会触发不同的碰撞逻辑，但是碰撞处理是一致的

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }


    void HandleHit(Collider collider)
    {

        if(canHitAfterCreated>0)
            return;
        var targetChaState = collider.GetComponentInParent<ChaState>();
        if (targetChaState == null )
        {
            Debug.Log($"子弹碰到障碍物{collider.gameObject.name}",collider.gameObject);
            if (!smoothMove)
            {
                HitedObstacle = true;
            }
           
            return;
        }

        if (CanHit(targetChaState))
        {
            if (caster != null)
            {
                var casterChaState = caster.GetComponent<ChaState>();
                //阵营检测
                if ((model.hitAlly == false && CombatComponent.GetRelation(casterChaState.Camp, targetChaState.Camp) ==
                        RelationType.Friendly) ||
                    (model.hitFoe == false && CombatComponent.GetRelation(casterChaState.Camp, targetChaState.Camp) ==
                        RelationType.Hostile))
                {
                    return;
                }

            }

            hp -= 1;

            if (model.onHit != null)
            {
                Debug.Log("子弹OnHit"+targetChaState.gameObject.name);
                model.onHit(gameObject, targetChaState.gameObject);
            }

            if (hp > 0)//还能继续碰撞
            {
                AddHitRecord(gameObject);
            }
            else
            {
                GameEntry.Entity.HideEntity(GetComponent<Entity>());
            }
        }
    }
    

    #endregion
    
}