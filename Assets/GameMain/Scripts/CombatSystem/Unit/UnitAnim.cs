using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


public struct AnimOrder
{
    public string param;
    public object value;
    public UnitAnim.AnimOrderType animOrderType;
    

    public AnimOrder( UnitAnim.AnimOrderType animOrderType,string param,object value)
    {
        this.animOrderType = animOrderType;
        this.param = param;
        this.value = value;
    }
}
///<summary>
///动画播放的管理器，是典型的ARPG之类即时回合制使用的，用来管理当前应该播放那个动画
///不仅仅是角色，包括aoe、bullet等，只要需要管理播放什么动画（带有animator的gameobject）就应该用这个
///</summary>
public class UnitAnim : MonoBehaviour{
    private Animator animator;

    [ReadOnly]
    public float m_TimeScale=1;
    ///<summary>
    ///播放的倍速，作用于每个信息的duration减少速度
    ///</summary>
    public float TimeScale
    {
        get => m_TimeScale;
        set
        {
            m_TimeScale = value;
            animator.speed = m_TimeScale;
        }
    }

    public enum AnimOrderType
    {
        Trigger,
        Bool,
        Float,
        Int,
    }
    

    void Start() {
        animator = this.gameObject.GetComponent<Animator>();
    }

    public void HandleAnimOrder(AnimOrder animOrder)
    {
        if (animOrder.animOrderType == AnimOrderType.Trigger)
        {
            PlayTrigger(animOrder.param);
        }

        if (animOrder.animOrderType == AnimOrderType.Float)
        {
            SetFloat(animOrder.param, (float)animOrder.value);
        }
        
        if (animOrder.animOrderType == AnimOrderType.Bool)
        {
            SetBool(animOrder.param, (bool)animOrder.value);
        }
        
    }

    private void PlayTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    private void SetBool(string param, bool status)
    {
        animator.SetBool(param,status);
        //Debug.Log("SetBool!!");
    }

    void SetFloat(string param, float value)
    {
        animator.SetFloat(param,value);
    }
}