using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class SpineJointControl : MonoBehaviour,IUseRayPoint
{


    public GameObject mark;
    private IkRagdollController ikRagdollController;

    public LineRenderer lineRenderer;

    private Vector3 initialForward;//记录模型初始方向
    public void Init(IkRagdollController ikRagdollController)
    {
        this.ikRagdollController = ikRagdollController;
    }
    
    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();


        initialForward = transform.forward;
    }

    private bool dragging = false;

    private void OnMouseEnter()
    {
        mark.gameObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        if(!dragging)
            mark.gameObject.SetActive(false);
    }

    private void OnMouseUp()
    {
        mark.gameObject.SetActive(false);
        dragging = false;
    }

    public ConfigurableJoint joint;

   
    

    private void OnMouseDrag()
    {
        UpdateTargetRotation();
        //UpdateTargetRotationByQuaternion();
        dragging = true;

    }

    private float eulerX = 0;
    [Header("骨骼偏差角度")]
    public float eulerXBias = 10;
    public Vector2 eulerXLimit = new Vector2(-60, 100);
    public float rotateSpeed =10;
    void UpdateTargetRotation()
    {
        var lookDir = ikRagdollController.RayPoint - transform.position;
       
        
        var up = transform.up;

        var parentUp = transform.parent.up;
        // 创建表示旋转的四元数,由于人物骨骼可能有旋转偏移，所以要考虑骨骼的旋转
        Quaternion rotation = Quaternion.AngleAxis( eulerXBias,transform.right);
        // 应用旋转偏移
        up =  rotation*up;
        
        Debug.DrawRay(transform.position,lookDir,Color.red);
        Debug.DrawRay(transform.position,up,Color.green);
        
        
        Vector3 cross=Vector3.Cross(lookDir,up);//鼠标向量和人物躯干的叉积
        var angle = Vector3.Angle(lookDir, up);

        var realAngle = angle;
        float dot = Vector3.Dot(cross, -transform.right);  //根据叉积将angle从180度转为360度
        if (dot < 0)
        {
            angle *= -1;
            angle += 360;
        }
        var parentCross = Vector3.Cross(lookDir, parentUp);
       
        
        //求角度
        //Debug.Log(angle);
        
        var targetSpeed = rotateSpeed;
        if (Mathf.Abs(angle-180)<90 && parentCross.x*cross.x<0)
        {
            //Debug.Log("反了");
            targetSpeed = -targetSpeed;//当鼠标向量和实际向量异侧时并且
        }

        var turnCross = Vector3.Cross(transform.right, initialForward);
        if (turnCross.y > 0)//说明朝向-z轴
        {
            targetSpeed = -targetSpeed;
        }
        
        if (cross.x > 0)//顺时针
        {
            eulerX += targetSpeed * Time.deltaTime * (0.1f+((realAngle/90)>1 ?1 :(realAngle/90)));
        }else if (cross.x < 0)
        {
            eulerX -= targetSpeed * Time.deltaTime *  (0.1f+((realAngle/90)>1 ?1 :(realAngle/90)));
        }

        eulerX = Mathf.Clamp(eulerX, eulerXLimit.x, eulerXLimit.y);

        joint.targetRotation = Quaternion.Euler(eulerX, 0, 0);
        //Debug.Log(eulerX);

       
    }


    void UpdateTargetRotationByQuaternion()
    {
        
        // 计算目标方向（世界坐标）
        Vector3 targetDirWorld = ikRagdollController.RayPoint - transform.position;

        // 计算世界空间中的旋转
        Quaternion worldRotation = Quaternion.LookRotation(targetDirWorld);

        // 将世界空间中的旋转转换为本地空间
        Quaternion localRotation = Quaternion.Inverse(transform.rotation) * worldRotation;

        // 应用旋转到关节
        joint.targetRotation = localRotation;
        

    }
    private void LateUpdate()
    {
        if (dragging)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[]{transform.position,ikRagdollController.RayPoint});
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
