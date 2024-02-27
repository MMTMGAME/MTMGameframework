using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;

public class PlayerMove : MonoBehaviour
{

    private Rigidbody rb;
    private Animator animator;
    private PlayerInputActions playerInputActions;
    
    [Header("基础移速，匹配动画基础速度")]
    public float baseSpeed = 5.457f;
    
    [Space(2)] public float jumpForce;

    [Header("移动的三条线,中间0,左边-1,右边1")]
    public int curLine;
    [Header("线的间隔")]
    public float lineGap = 0.5f;
    public float switchLineDuration = 0.3f;

    [Header("地面检测长度")] 
    public float rayDistance = 0.13f;

    [Header("SphereCast半径")]
    public float sphereRadius = 0.15f;  // SphereCast的半径

    [SerializeField]
    private float speed = 5.457f;

    private bool isGrounded;

    private Road curRoad;
    private RoadConfig curRoadConfig;
    private RoadConfig lastTurnedRoad;

    public LayerMask groundCheckLayer;

    public void SetSpeed(float multiplier)
    {
        speed = baseSpeed * (1 + multiplier);
        animator.speed = 1 + multiplier;
    }
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        speed = baseSpeed;
        
        
        playerInputActions = new PlayerInputActions();
      
       
    }

    private void OnEnable()
    {
        playerInputActions.Player.Space.performed += Jump;
        playerInputActions.Player.Left.performed += Left;
        playerInputActions.Player.Right.performed += Right;
        playerInputActions.Enable();
        
    }

    private void OnDisable()
    {
        playerInputActions.Player.Space.performed -= Jump;
        playerInputActions.Player.Left.performed -= Left;
        playerInputActions.Player.Right.performed -= Right;
        playerInputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        SyncAnimatorParams();
        Move();
    }

    void GroundCheck()
    {
        // SphereCast的起始位置稍微提升，以避免从地面内部开始
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // 使用SphereCast进行地面检测
        if (Physics.SphereCast(origin, sphereRadius, -Vector3.up, out var hit, rayDistance, groundCheckLayer))
        {
            var go = hit.transform.gameObject;
            if (go.CompareTag("Road"))
            {
                var road = go.GetComponentInParent<Road>();
                curRoadConfig = go.GetComponentInParent<RoadConfig>();
                if (curRoadConfig != null)
                {
                    // 可以在这里添加额外的逻辑0
                    // Log.Info($"curRoadConfig:{go.gameObject.name},isBranch:{curRoadConfig.isBranch}");
                }
                curRoad = road;
            }
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void SyncAnimatorParams()
    {
     animator.SetBool(IsGrounded,isGrounded);   
    }
    
    void Move()
    {
        if(isSwitchingLine)
            return;
        transform.Translate(Vector3.forward * (speed * Time.deltaTime),Space.Self);
    }


    private bool isSwitchingLine;
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

    void Left(InputAction.CallbackContext callbackContext)
    {
        // TurnLeft();
        // return;
        
        if(isSwitchingLine /*|| isGrounded==false*/)
            return;
        
        curLine--;

        if ( curRoadConfig!=null && curRoadConfig.isTurn)
        {
            TurnLeft();
        }
        else
        {
            curLine = GetCurLineIndex();
            var targetPos = CalTargetPosInLine(--curLine);
            StartCoroutine(SwitchLineCoroutine(targetPos));
        }
    }

    void Right(InputAction.CallbackContext callbackContext)
    {
        // TurnRight();
        // return;
        
        if(isSwitchingLine /*|| isGrounded==false*/)
            return;
        
        curLine++;
        
        if (curRoadConfig!=null && curRoadConfig.isTurn)
        {
            TurnRight();
        }
        else
        {
            
            curLine = GetCurLineIndex();
            var targetPos = CalTargetPosInLine(++curLine);
            StartCoroutine(SwitchLineCoroutine(targetPos));
        }
    }

    void TurnRight()
    {
        if (curRoadConfig ==lastTurnedRoad)
            return;
        transform.DORotateQuaternion(Quaternion.LookRotation(transform.right), 0.1f);
        lastTurnedRoad = curRoadConfig;
    }

    void TurnLeft()
    {
        if (curRoadConfig ==lastTurnedRoad)
            return;
        transform.DORotateQuaternion(Quaternion.LookRotation(-transform.right), 0.1f);
        lastTurnedRoad = curRoadConfig;
    }

    int GetCurLineIndex()
    {
        Vector3 playerPos = transform.position; // 玩家位置

        // 定义三条直线
        Vector3 pN1 = curRoadConfig.transform.position- curRoadConfig.transform.right* lineGap, dN1 = curRoadConfig.transform.forward; // 第一条直线的点和方向向量
        Vector3 p0 = curRoadConfig.transform.position, d0 = curRoadConfig.transform.forward; // 第二条直线的点和方向向量
        Vector3 p1 = curRoadConfig.transform.position+ curRoadConfig.transform.right* lineGap, d1 = curRoadConfig.transform.forward; // 第三条直线的点和方向向量

        // 计算到每条直线的距离
        float distance1 = Vector3.Cross(playerPos - pN1, dN1).magnitude / dN1.magnitude;
        float distance2 = Vector3.Cross(playerPos- p0, d0).magnitude / d0.magnitude;
        float distance3 = Vector3.Cross(playerPos - p1, d1).magnitude / d1.magnitude;

        // 找出最小距离
        float minDistance = Mathf.Min(distance1, distance2, distance3);

        // 确定最近的直线
        int nearestLine;
        if (minDistance == distance1) {
            nearestLine = -1; // 第一条直线最近
        } else if (minDistance == distance2) {
            nearestLine = 0; // 第二条直线最近
        } else {
            nearestLine = 1; // 第三条直线最近
        }

        return nearestLine;
    }
    
    float CalLineDistance(int lineIndex)
    {
        var linePoint = curRoadConfig.transform.position +  curRoadConfig.transform.right * lineIndex * lineGap;
        var lineDir = curRoadConfig.transform.forward;
        var playerPos = transform.position;
        
        float distance = Vector3.Cross(playerPos - linePoint, lineDir).magnitude / lineDir.magnitude;
        return distance;
    }
    Vector3 CalTargetPosInLine(int lineIndex)
    {
        var linePoint = curRoadConfig.transform.position +  curRoadConfig.transform.right * lineIndex * lineGap;
        var lineDir = curRoadConfig.transform.forward;
        var playerPos = transform.position;
        
        var movedPos=transform.position + transform.forward * (speed * switchLineDuration);//当前线上移动后的位置
        
        // 计算移动后的点在目标线上的投影长度
        float projectionLength = Vector3.Dot(movedPos-linePoint, lineDir.normalized);
        
        // 计算投影点
        Vector3 targetPoint = linePoint + lineDir.normalized * projectionLength;
        targetPoint.y = transform.position.y;//不要重置y坐标
        return targetPoint;
    }
    
    IEnumerator SwitchLineCoroutine(Vector3 targetPosition)
    {
        isSwitchingLine = true;

        float startTime = Time.time;
        Vector3 startPosition = transform.position;
        
        
        

        while (Time.time < startTime + switchLineDuration)
        {
            // 确保每帧移动的是正确的距离，使得总移动距离等于 moveDistance
            float t = (Time.time - startTime) / switchLineDuration;
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, t);
            Vector3 moveVector = newPosition - transform.position;
            moveVector.y = 0;//忽略y轴
            // 使用 Translate 移动，确保移动是相对于局部空间的
            transform.Translate(moveVector, Space.World);

            yield return null;
        }

        // 确保角色最终位于精确的目标位置
        Vector3 finalMoveVector = targetPosition - transform.position;
        transform.Translate(finalMoveVector, Space.World);

        isSwitchingLine = false;
    }
    
    void Jump(InputAction.CallbackContext callbackContext)
    {
        if (isGrounded)
        {
            animator.SetTrigger("Jump");
        }
      
    }

    public void OnJumpStart()
    {
        rb.AddForce(jumpForce*Vector3.up);
    }

    public void OnJumpEnd()
    {
        
    }


    // private void OnCollisionEnter(Collision collision)
    // {
    //     
    // }
    //
    // private void OnTriggerEnter(Collider other)
    // {
    //     Debug.Log("TriggerEnter:"+other.gameObject.name);
    //     if (other.gameObject.CompareTag("Road"))
    //     {
    //         var road = other.gameObject.GetComponentInParent<Road>();
    //         curRoad = road;
    //     }
    // }
}
