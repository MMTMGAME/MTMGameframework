using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HandIkMove : MonoBehaviour,IUseRayPoint
{
    private IkRagdollController ikRagdollController;
    public Transform ikTopTrans;
    [FormerlySerializedAs("ikTipTrans")] public Transform ikMidTrans;

    [Header("用于判断是否转向")]
    public Transform spine;

   
    public float LastDownTime
    {
        get;
        private set;
    }
    public void Init(IkRagdollController ikRagdollController)
    {
        this.ikRagdollController = ikRagdollController;
    }

    private Vector3 initialForward;//记录模型初始方向
    public float xOffset;

    [Header("结束控制室操作柄该回到的位置，yoffset表示沿着手肘关节的长度")]
    public float yOffSet=1;
    // Start is called before the first frame update
    void Start()
    {
        initialForward = spine.transform.forward;
    }

    private void OnMouseDown()
    {
        LastDownTime = Time.time;
    }

    private void OnMouseDrag()
    {
        //Debug.Log("Dragging");

        var targetOffset = xOffset;
        var turnCross = Vector3.Cross(spine.transform.right, initialForward);//判断朝向
        if (turnCross.y > 0)//说明朝向-z轴，反转
        {
            targetOffset = -xOffset;
        }
        
        transform.position = ikRagdollController.RayPoint+new Vector3(targetOffset,0,0);
        transform.rotation = ikMidTrans.rotation;
    }

   

    

    private void OnMouseUp()
    {
        var dir = transform.position - ikTopTrans.position;
        if (dir.magnitude > yOffSet)
        {
            transform.position = ikTopTrans.position + dir.normalized * yOffSet;
        }
        
        // transform.localPosition = ikMidTrans.TransformPoint(0, yOffSet, 0);
        // transform.rotation = ikMidTrans.rotation;
    }
}
