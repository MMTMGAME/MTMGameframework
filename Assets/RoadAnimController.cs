using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class RoadAnimController : MonoBehaviour
{

    private Player player;

    private Animator animator;

    private static readonly int Collapse = Animator.StringToHash("Collapse");

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool(Collapse,true);
        return;
        try
        {
            player = (GameEntry.Procedure.CurrentProcedure as ProcedureLevel)?.GetGameBase().Player;
        }
        catch (Exception e)
        {
            
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if(player==null)
    //         return;
    //     var localDistance = transform.InverseTransformPoint(player.transform.position);
    //     if (localDistance is { x: < 2, z: < 12 })
    //     {
    //         animator.SetBool(Collapse,true);
    //     } 
    // }

    #region MyRegion

    public void PlayCollapseAudio()
    {
        GameEntry.Sound.PlaySound(30000);
    }
    
    public void PlayCollideAudio()
    {
        GameEntry.Sound.PlaySound(30001);
        
        float maxShakeIntensity = 2.0f;
        // 震屏强度随距离减小的速率（你可以根据需要调整这个值）
        float shakeDecayRate = 0.2f;
        // 计算相机和对象之间的距离
        var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        // 根据距离计算震屏强度，距离越小震屏强度越大
        float shakeIntensity = Mathf.Clamp(maxShakeIntensity - (distance * shakeDecayRate), 0.4f, maxShakeIntensity);
        // 使用计算出的震屏强度来震动相机
        GameEntry.CameraShake.ShakeCamera(shakeIntensity,1,0.45f);
    }


    #endregion 动画事件
}
