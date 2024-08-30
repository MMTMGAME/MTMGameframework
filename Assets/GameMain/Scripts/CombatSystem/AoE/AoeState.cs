using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;


///<summary>
///AoE的状态控制器
///</summary>
public class AoeState : MonoBehaviour{
    ///<summary>
    ///要释放的aoe
    ///</summary>
    public AoeModel model;

    ///<summary>
    ///是否被视作刚创建
    ///</summary>
    public bool justCreated = true;

    ///<summary>
    ///aoe的半径，单位：米
    ///目前这游戏的设计中，aoe只有圆形，所以只有一个半径，也不存在角度一说，如果需要可以扩展
    ///</summary>
    public float radius;

    ///<summary>
    ///aoe的施法者
    ///</summary>
    public GameObject caster;

    ///<summary>
    ///aoe存在的时间，单位：秒
    ///</summary>
    public float duration;

    ///<summary>
    ///aoe已经存在过的时间，单位：秒
    ///</summary>
    public float timeElapsed = 0;

    ///<summary>
    ///aoe移动轨迹函数
    ///</summary>
    public AoeTween tween;

    ///<summary>
    ///aoe的轨迹运行了多少时间了，单位：秒
    ///<summary>
    public float tweenRunnedTime = 0;

    ///<summary>
    ///创建时角色的属性
    ///</summary>
    public ChaProperty propWhileCreate;

    ///<summary>
    ///aoe的传入参数，比如可以吸收次数之类的
    ///</summary>
    public Dictionary<string, object> param = new Dictionary<string, object>();

    ///<summary>
    ///现在aoe范围内的所有角色的gameobject,
    ///</summary>
    public List<GameObject> characterInRange = new List<GameObject>();

    ///<summary>
    ///现在aoe范围内的所有子弹的gameobject
    ///</summary>
    public List<GameObject> bulletInRange = new List<GameObject>();

    ///<summary>
    ///Tween函数的参数
    ///</summary>
    public object[] tweenParam;

    ///<summary>
    ///移动信息
    ///</summary>
    public Vector3 velocity{
        get{ return this._velo;}
    }
    private Vector3 _velo = new Vector3();

    private UnitMove unitMove;
    private UnitRotate unitRotate;
    //private GameObject viewContainer;

    private bool inited=false;//异步加载的，所以要判端这个，不然直接报错了

    private void Start() {
        // this.unitMove = this.gameObject.GetComponent<UnitMove>();
        // this.unitRotate = this.gameObject.GetComponent<UnitRotate>();
        synchronizedUnits();
    }

    ///<summary>
    ///设置移动和旋转的信息，用于执行
    ///</summary>
    public void SetMoveAndRotate(AoeMoveInfo aoeMoveInfo){
        if (aoeMoveInfo != null){
            if (unitMove){
                _velo = aoeMoveInfo.velocity / Time.fixedDeltaTime;
                unitMove.MoveBy(_velo);
            }
            if (unitRotate){
                unitRotate.RotateTo(aoeMoveInfo.rotation);
            }
        }
    }

    public bool HitObstacle(){
        return unitMove == null ? false : unitMove.hitObstacle;
    }

    private void synchronizedUnits(){
        if (!unitMove) unitMove = this.gameObject.GetComponent<UnitMove>();
        if (!unitRotate) unitRotate = this.gameObject.GetComponent<UnitRotate>();
        //if (!viewContainer) viewContainer = this.gameObject.GetComponentInChildren<ViewContainer>().gameObject;
    }

    public void InitByAoeLauncher(AoeLauncher aoe){
        this.model = aoe.model;
        
        this.duration = aoe.duration;
        this.timeElapsed = 0;
        this.tween = aoe.tween;
        this.tweenParam = aoe.tweenParam;
        this.tweenRunnedTime = 0;
        this.param = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> kv in aoe.param){
            this.param[kv.Key] = kv.Value;
        }//aoe.param;
        this.caster = aoe.caster;
        if (aoe.caster == null)
        {
            Debug.Log("InitAoe时AoeCaster为null，Aoe信息"+aoe.model.id);
        }
        this.propWhileCreate = aoe.caster ? aoe.caster.GetComponent<ChaState>().property : ChaProperty.zero;
        
        this.transform.position = aoe.position;
        this.transform.rotation = aoe.rotation;

        this.transform.localScale = aoe.scale * Vector3.one;

        synchronizedUnits();
        inited = true;

        // //把视觉特效给aoe
        // if (aoe.model.entityTypeId != 0){
        //     GameEntry.Entity.ShowModelObj(aoe.model.entityTypeId,Vector3.zero, Quaternion.identity, (aoeEffect) =>
        //     {
        //         
        //         GameEntry.Entity.AttachEntity(aoeEffect.Entity,GetComponent<Entity>().Entity,transform);
        //         aoeEffect.transform.localPosition =Vector3.zero;
        //         aoeEffect.transform.localRotation = Quaternion.identity;
        //     } );
        // }
        // this.gameObject.transform.position = new Vector3(
        //     this.gameObject.transform.position.x,
        //     0,
        //     this.gameObject.transform.position.z
        // );
    }

    ///<summary>
    ///改变aoe视觉的尺寸
    ///</summary>
    // public void SetViewScale(float scaleX = 1, float scaleY = 1, float scaleZ = 1){
    //     synchronizedUnits();
    //     
    // }

    // ///<summary>
    // ///改变图形的y高度
    // ///</summary>
    // public void ModViewY(float toY){
    //     this.viewContainer.transform.position = new Vector3(
    //         viewContainer.transform.position.x,
    //         toY,
    //         viewContainer.transform.position.z
    //     );
    // }

    
   
    
    private void FixedUpdate()
    {
        if(!inited)
            return;
        float timePassed = Time.fixedDeltaTime;
        
        //首先是aoe的移动
        if (duration > 0 && tween != null){
            AoeMoveInfo aoeMoveInfo = tween(gameObject, tweenRunnedTime);
            tweenRunnedTime += timePassed;
            SetMoveAndRotate(aoeMoveInfo);
        }

        if (justCreated == true)
        {
            //刚创建的，走onCreate
            justCreated = false;
            
            if(model.onCreate!=null)
                model.onCreate(gameObject);
        }

        duration -= timePassed;
        timeElapsed += timePassed;
        if (duration <= 0)
        {
            if (model.onRemoved != null)
            {
                if(model.onRemoved!=null)
                    model.onRemoved(gameObject);
            }
            GameEntry.Entity.HideEntity(GetComponent<Entity>());
        }
        else
        {
            if (
                model.tickTime > 0 && model.onTick != null &&
                Mathf.RoundToInt(duration * 1000) % Mathf.RoundToInt(model.tickTime * 1000) == 0
            ){
                if(model.onTick!=null)
                    model.onTick(gameObject);
            }
        }
    }

    
    
    private void OnTriggerEnter(Collider other)
    {
        var cha = other.GetComponentInParent<ChaState>();
        if (cha!=null)
        {
            characterInRange.Add(cha.gameObject);
            if(model.onChaEnter!=null)
                model.onChaEnter(gameObject, new List<GameObject>() { cha.gameObject });
        }

        var bulletState = other.GetComponentInParent<BulletState>();
        if (bulletState)
        {
            bulletInRange.Add(bulletState.gameObject);
            if(model.onBulletEnter!=null)
                model.onBulletEnter(gameObject, new List<GameObject>() { bulletState.gameObject });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var cha = other.GetComponentInParent<ChaState>();
        if (cha != null)
        {
            if (characterInRange.Contains(cha.gameObject))
            {
                characterInRange.Remove(cha.gameObject);
                if(model.onChaLeave!=null)
                    model.onChaLeave(gameObject, new List<GameObject>() { cha.gameObject });
            }
        }
        
        
        var bulletState = other.GetComponentInParent<BulletState>();
        if (bulletState)
        {
            if (bulletInRange.Contains(bulletState.gameObject))
            {
                bulletInRange.Remove(bulletState.gameObject);
                if(model.onBulletLeave!=null)
                    model.onBulletLeave(gameObject, new List<GameObject>() { bulletState.gameObject });
            }
        }
    }
}