using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using GameMain;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityGameFramework.Runtime;

using Entity = UnityGameFramework.Runtime.Entity;
using GameEntry = GameMain.GameEntry;

public class PlayerMove : MonoBehaviour
{

    private Rigidbody rb;
    private Animator animator;
    private PlayerInputActions playerInputActions;

    public float distance { get; private set; }

    [Header("基础移速，匹配动画基础速度")]
    public float baseSpeed = 5.457f;

    [Header("弹幕路段左右移动速度")] public float shiftSpeed = 5.457f;
    
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

    private SwipeDetection swipeDetection;

    private CapsuleCollider capsuleCollider;
    private Rig constraintRig;
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    //private AutoRunDirection curAutoRunDirection;
    private bool autoRun;


    private float showTime;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        speed = baseSpeed;
        
        
        playerInputActions = new PlayerInputActions();
        swipeDetection = GetComponent<SwipeDetection>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        constraintRig = GetComponentInChildren<Rig>();

        originalColliderCenter = capsuleCollider.center;
        originalColliderHeight = capsuleCollider.height;

        showTime = Time.time;

        autoRun = false;

        curRoad = null;

    }


    public bool isBarrageMode;
    private void OnEnable()
    {
        playerInputActions.Player.Space.performed += Jump;
        playerInputActions.Player.Left.performed += Left;
        playerInputActions.Player.Right.performed += Right;
        playerInputActions.Player.Down.performed += Down;
        playerInputActions.Player.F.performed += UseSkill;

        swipeDetection.swipeUp += Jump;
        swipeDetection.swipeLeft += Left;
        swipeDetection.swipeRight += Right;
        swipeDetection.swipeDown += Down;
        
        GameEntry.Event.Subscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
        GameEntry.Event.Subscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
        
        playerInputActions.Enable();
        
    }

    private void OnDisable()
    {
        playerInputActions.Player.Space.performed -= Jump;
        playerInputActions.Player.Left.performed -= Left;
        playerInputActions.Player.Right.performed -= Right;
        playerInputActions.Player.Down.performed -= Down;
        playerInputActions.Player.F.performed -= UseSkill;
        
        swipeDetection.swipeUp -= Jump;
        swipeDetection.swipeLeft -= Left;
        swipeDetection.swipeRight -= Right;
        swipeDetection.swipeDown -= Down;
        
        
        GameEntry.Event.Unsubscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
        GameEntry.Event.Unsubscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
        
        playerInputActions.Disable();
    }

    void OnPlayerEnterBarrageRoad(object sender, GameEventArgs args)
    {
        isBarrageMode = true;
    }

    void OnPlayerExitBarrageRoad(object sender, GameEventArgs args)
    {
        isBarrageMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        SyncAnimatorParams();
        Move();
        GroundCheck();
        
        AutoRunCheck();
        
        DelayTurnCheck();
    }

    // private void FixedUpdate()
    // {
    //     AutoRunCheck();
    // }

    void GroundCheck()
    {
        // SphereCast的起始位置稍微提升，以避免从地面内部开始
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        Debug.DrawLine(origin, origin - Vector3.up * rayDistance, Color.red);

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
     //animator.SetBool(IsGrounded,true);   
    }
    
    void Move()
    {
        if(isSwitchingLine)
            return;
        var delta = Vector3.forward * (speed * Time.deltaTime);
        transform.Translate(delta,Space.Self);
        distance += speed * Time.deltaTime;

        if (isBarrageMode)
        {
            var vector2 = playerInputActions.Player.Move.ReadValue<Vector2>();
            transform.Translate(transform.right * (vector2.x * shiftSpeed * Time.deltaTime),Space.World);
        }

    }


    private bool isSwitchingLine;
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Slide = Animator.StringToHash("Slide");
    private static readonly int Stumbled = Animator.StringToHash("Stumbled");


    private float lastTurnLeftKeyTime = 0;
    private float lastTurnRightKeyTime = 0;


    void UseSkill(InputAction.CallbackContext callbackContext)
    {
        try
        {
            (GameEntry.Procedure.CurrentProcedure as ProcedureLevel)?.GetGameBase().UseSkill();
        }
        catch (Exception e)
        {
           //释放技能
        }
        
    }
    void Down(InputAction.CallbackContext callbackContext)
    {
        animator.SetTrigger(Slide);
    }
    void Left(InputAction.CallbackContext callbackContext)
    {
        // TurnLeft();
        // return;
        
        if(isSwitchingLine /*|| isGrounded==false*/)
            return;
        
        if(curRoadConfig==null)
            return;
        
        curLine--;

        if ( curRoadConfig!=null && curRoadConfig.isTurn /*&& (curAutoRunDirection.direction==AutoRunDirection.Direction.Left || curAutoRunDirection.direction == AutoRunDirection.Direction.Right)*/)
        {
            lastTurnLeftKeyTime = Time.time;
            //TurnLeft();
        }
        else
        {
            if (isBarrageMode)
            {
                //DOnothing

                if (true || UnityEngine.Device.Application.isMobilePlatform)
                {
                    var targetPos = transform.TransformPoint(-lineGap , 0, speed * switchLineDuration);
                    StartCoroutine(SwitchLineCoroutine(targetPos));
                    Log.Debug("左滑");
                }
            }
            else
            {
                curLine = GetCurLineIndex();
                var targetLine = --curLine;
                if (targetLine < -1)
                {
                    targetLine = -1;
                }
                var targetPos = CalTargetPosInLine(targetLine);
                StartCoroutine(SwitchLineCoroutine(targetPos));
            }
           
        }
    }

    void SwitchToLine(int lineIndex)
    {
        if(isSwitchingLine)
            return;
        var targetPos = CalTargetPosInLine(lineIndex);
        StartCoroutine(SwitchLineCoroutine(targetPos));
    }

    void SwitchToLineForcibly(int lineIndex)
    {
        var targetPos = CalTargetPosInLine(lineIndex);
        StartCoroutine(SwitchLineCoroutine(targetPos));
    }

    
    void Right(InputAction.CallbackContext callbackContext)
    {
        // TurnRight();
        // return;
        
        if(isSwitchingLine /*|| isGrounded==false*/)
            return;
        
        if(curRoadConfig==null)
            return;
        
        curLine++;
        
        if (curRoadConfig.isTurn /*&& (curAutoRunDirection.direction==AutoRunDirection.Direction.Left || curAutoRunDirection.direction == AutoRunDirection.Direction.Right)*/)
        {
            lastTurnRightKeyTime = Time.time;
            //TurnRight();
        }
        else
        {
            if (isBarrageMode)
            {
                //DOnothing
                if (true||  UnityEngine.Device.Application.isMobilePlatform)
                {

                    var targetPos = transform.TransformPoint(lineGap, 0, speed * switchLineDuration);
                    StartCoroutine(SwitchLineCoroutine(targetPos));
                    
                }
            }
            else
            {
                curLine = GetCurLineIndex();
                var targetLine = ++curLine;
                if (targetLine > 1)
                {
                    targetLine = 1;
                }

                var targetPos = CalTargetPosInLine(targetLine);
                StartCoroutine(SwitchLineCoroutine(targetPos));
            }
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

        if (curRoadConfig == null)
            return 0;
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
        var linePoint = curRoadConfig.transform.position +  curRoadConfig.transform.right * (lineIndex * lineGap);
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
            //Debug.Log("Jump!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            animator.SetTrigger("Jump");
        }
      
    }

    #region 动画事件
    
    public void OnJumpStart()
    {
        rb.AddForce(jumpForce*Vector3.up,ForceMode.Impulse);
    }

    

    //由动画状态机SendMessage触发
    public void OnSlideStart()
    {
        capsuleCollider.center =
            new Vector3(originalColliderCenter.x, originalColliderCenter.y / 2, originalColliderCenter.z);
        capsuleCollider.height = originalColliderHeight / 2;
    }

    public void OnSlideEnd()
    {
        capsuleCollider.center = originalColliderCenter;
        capsuleCollider.height = originalColliderHeight;
    }
    #endregion

    #region 自动模式触发器



    public void SwitchAutoRun(bool status)
    {
        autoRun = status;
        if (autoRun)
        {
            if(isBarrageMode)
                return;//弹幕模式不禁用控制。
            playerInputActions.Disable();
            swipeDetection.Disable();
        }
        else
        {
            playerInputActions.Enable();
            swipeDetection.Enable();
        }
    }
    
  
    private AutoRunDirection lastAutoRunDirection; 
    void AutoRunCheck()
    {
        //因为实体实体生成时扎堆在0，0，0坐标生成，所以刚开始生成时不要执行以下方法
            if(Time.time<showTime+1)
                return;
            
            float radius = 0.5f; // 球体的半径
            Vector3 center = transform.position+Vector3.up; // 球体中心设为物体当前位置
            
            // 获取球形区域内的所有碰撞体
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            
            int i = 0;
            
            while (i < hitColliders.Length)
            {
                // 碰撞体处理，例如检查标签来识别特定对象
                if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("AutoRunTrigger"))
                {
                    
                    // 执行某些操作，比如触发事件
                    var curAutoRunDirection = hitColliders[i].gameObject.GetComponent<AutoRunDirection>();
                    //Log.Debug("检测到RunTrigger"+hitColliders[i].gameObject.name+"其父物体为:"+hitColliders[i].gameObject.transform.parent.name);

                    if (curAutoRunDirection != null)
                    {
                        //GameEntry.Entity.ShowDebug3DText(transform.position,transform.rotation,"Direction"+curAutoRunDirection.direction.ToString());
                        if (curAutoRunDirection.GetComponentInParent<RoadConfig>()==curRoadConfig &&   autoRun && curAutoRunDirection!=lastAutoRunDirection)
                        {
                            lastAutoRunDirection = curAutoRunDirection;
                        
                            Turn(curAutoRunDirection);
                            //GameEntry.Entity.ShowDebug3DText(transform.position+Vector3.up,transform.rotation,"Turn"+curAutoRunDirection.direction.ToString(),50);
                        }
                    }
                    
                    
                }
                i++;
            }
    }
    
    
    private AutoRunDirection lastDelayTurnDirection; 
    //延迟转弯，和AutoRunCheck差不多，只不过这个只判断左右方向，并且会缓冲0.5秒的转弯输入以降低转弯难度
    void DelayTurnCheck()
    {
        //因为实体实体生成时扎堆在0，0，0坐标生成，所以刚开始生成时不要执行以下方法
        if(Time.time<showTime+1 || autoRun)
            return;
            
        float radius = 0.5f; // 球体的半径
        Vector3 center = transform.position+Vector3.up; // 球体中心设为物体当前位置
            
        // 获取球形区域内的所有碰撞体
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            
        int i = 0;
            
        while (i < hitColliders.Length)
        {
            // 碰撞体处理，例如检查标签来识别特定对象
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("AutoRunTrigger"))
            {
                    
                // 执行某些操作，比如触发事件
                var curAutoRunDirection = hitColliders[i].gameObject.GetComponent<AutoRunDirection>();
                //Log.Debug("检测到RunTrigger"+hitColliders[i].gameObject.name+"其父物体为:"+hitColliders[i].gameObject.transform.parent.name);

                if (curAutoRunDirection != null)
                {
                    // bool isTurnRoad = curAutoRunDirection.direction == AutoRunDirection.Direction.Left ||
                    //                   curAutoRunDirection.direction == AutoRunDirection.Direction.Right;
                    bool delayTurnCheck = curAutoRunDirection.direction == AutoRunDirection.Direction.Left ||
                                          curAutoRunDirection.direction == AutoRunDirection.Direction.Right;

                    bool delayLeftTimeCheck = lastTurnLeftKeyTime + 0.5f >= Time.time;
                    bool delayRightTimeCheck  =  lastTurnRightKeyTime + 0.5f >= Time.time;
                    
                    if (delayTurnCheck && (delayLeftTimeCheck|| delayRightTimeCheck) &&  curAutoRunDirection.GetComponentInParent<RoadConfig>()==curRoadConfig  && curAutoRunDirection!=lastDelayTurnDirection)
                    {
                        lastDelayTurnDirection = curAutoRunDirection;
                        
                        if(delayLeftTimeCheck&& curAutoRunDirection.direction==AutoRunDirection.Direction.Left)
                            TurnLeft();
                        if(delayRightTimeCheck && curAutoRunDirection.direction==AutoRunDirection.Direction.Right)
                            TurnRight();
                        //GameEntry.Entity.ShowDebug3DText(transform.position+Vector3.up,transform.rotation,"Turn"+curAutoRunDirection.direction.ToString(),50);
                    }
                }
                    
                    
            }
            i++;
        }
    }


    private int lastTurnFrameCount;
    void Turn(AutoRunDirection  curAutoRunDirection)
    {
        if(Time.frameCount==lastTurnFrameCount)
            return;
        lastTurnFrameCount = Time.frameCount;
        if (curAutoRunDirection.direction == AutoRunDirection.Direction.Up)
        {
            Jump(default);
        }

        if (curAutoRunDirection.direction == AutoRunDirection.Direction.Down)
        {
            Down(default);
        }

        if (curAutoRunDirection.direction == AutoRunDirection.Direction.Left)
        {
            TurnLeft();
        }
                
        if (curAutoRunDirection.direction == AutoRunDirection.Direction.Right)
        {
            TurnRight();
        }

        if (curAutoRunDirection.direction == AutoRunDirection.Direction.SwitchLine)
        {
            SwitchToLine(curAutoRunDirection.lineIndex);
        }
    }
    #endregion

    #region 被绊倒

    public float lastStumbleTime { get; private set; }

    public void OnStumble()
    {
        animator.SetTrigger(Stumbled);
        StartCoroutine(SlowDownC());
        GameEntry.CameraShake.ShakeCamera(1f,1,0.35f);
        GameEntry.Sound.PlaySound(10030);
        lastStumbleTime = Time.time;
    }

    
    IEnumerator SlowDownC()
    {
        var originalSpeed = speed;
        speed *= 0.4f;
        yield return new WaitForSeconds(0.7f);
        speed = originalSpeed;
    }

    #endregion

    
}
