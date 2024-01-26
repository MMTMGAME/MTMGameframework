using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RagdollController : MonoBehaviour
{
    // public Animator animator;
    //
    // public float transitionSpeed = 1;
    //
    // private static readonly int Speed = Animator.StringToHash("Speed");

    [Header("---右腿---")] 
    public ConfigurableJoint rightLeg;

    public Vector3 rightLegIdleEuler = new Vector3(0, 0, 0);
    public Vector3 rightLegRunEuler = new Vector3(-90, 0, 0);
    
    [Header("---右膝盖---")]
    public ConfigurableJoint rightKnee;

    public Vector3 rightKneeIdleEuler = new Vector3(0, 0, 0);
    public Vector3 rightKneeUpEuler = new Vector3(0, 0, 0);
    public Vector3 rightKneeRunEuler = new Vector3(-90, 0, 0);
    
    [Header("---左腿---")]
    public ConfigurableJoint leftLeg;
    public Vector3 leftLegIdleEuler = new Vector3(0, 0, 0);
    public Vector3 leftLegRunEuler = new Vector3(-90, 0, 0);
    
    [Header("---左膝盖---")]
    public ConfigurableJoint leftKnee;

    public Vector3 leftKneeIdleEuler = new Vector3(0, 0, 0);
    public Vector3 leftKneeUpEuler = new Vector3(0, 0, 0);
    public Vector3 leftKneeRunEuler = new Vector3(-90, 0, 0);

    [FormerlySerializedAs("hip")] public ConfigurableJoint spine;
    public Vector2 rotateXLimit;
    public float scrollMultiplier = 0.1f;
    
    // public Vector3 hipIdleEuler;
    //
    // public Vector3 hipRunEuler;

    
    //value
    [Header("修改以设置初始值")]
    private float hipEulerX=-3;
    
    private PlayerInputActions playerInputActions;


    [Header("右手臂")] public ConfigurableJoint rightArm;
    public Vector3 rightArmIdleEuler;
    public Vector3 rightArmLiftEuler;

    [Header("左手臂")] public ConfigurableJoint leftArm;
    public Vector3 leftArmIdleEuler;
    public Vector3 leftArmLiftEuler;
    
    
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        
        
    }

    void OnRightLegStarted(InputAction.CallbackContext context)
    {
        pressD = true;
    }
    
    void OnRightLegCanceled(InputAction.CallbackContext context)
    {
        pressD = false;
    }
    
    void OnLeftLegStarted(InputAction.CallbackContext context)
    {
        pressA = true;
    }
    
    void OnLeftLegCanceled(InputAction.CallbackContext context)
    {
        pressA = false;
    }

    void OnMouseLeftStarted(InputAction.CallbackContext context)
    {
        pressMouseLeft = true;
    }

    void OnMouseLeftCanceled(InputAction.CallbackContext context)
    {
        pressMouseLeft = false;
    }
    
    void OnMouseRightStarted(InputAction.CallbackContext context)
    {
        pressMouseRight = true;
    }

    void OnMouseRightCanceled(InputAction.CallbackContext context)
    {
        pressMouseRight = false;
    }

    private float curTurnYValue=0;
    
    void RagdollTurn180(InputAction.CallbackContext context)
    {
        spine.targetRotation = Quaternion.Euler(0, curTurnYValue + 180, 0);
        curTurnYValue += 180;
    }
  
    private void OnEnable()
    {
        playerInputActions.Player.RightLeg.started += OnRightLegStarted;
        playerInputActions.Player.RightLeg.canceled += OnRightLegCanceled;
        playerInputActions.Player.LeftLeg.started += OnLeftLegStarted;
        playerInputActions.Player.LeftLeg.canceled += OnLeftLegCanceled;


        playerInputActions.Player.MouseLeft.started += OnMouseLeftStarted;
        playerInputActions.Player.MouseLeft.canceled += OnMouseLeftCanceled;

        playerInputActions.Player.MouseRight.started += OnMouseRightStarted;
        playerInputActions.Player.MouseRight.canceled += OnMouseRightCanceled;

        playerInputActions.Player.Space.performed += RagdollTurn180;
        
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        
        playerInputActions.Player.RightLeg.started -= OnRightLegStarted;
        playerInputActions.Player.RightLeg.canceled -= OnRightLegCanceled;
        playerInputActions.Player.LeftLeg.started -= OnLeftLegStarted;
        playerInputActions.Player.LeftLeg.canceled -= OnLeftLegCanceled;

        
        playerInputActions.Player.MouseLeft.started -= OnMouseLeftStarted;
        playerInputActions.Player.MouseLeft.canceled -= OnMouseLeftCanceled;

        playerInputActions.Player.MouseRight.started -= OnMouseRightStarted;
        playerInputActions.Player.MouseRight.canceled -= OnMouseRightCanceled;
        
        playerInputActions.Player.Space.performed -= RagdollTurn180;
        
        playerInputActions.Disable();
    }

    private bool pressA;
    private bool pressD;
    private bool running;

    private bool pressMouseLeft;
    private bool pressMouseRight;
    
    
    private string rightLegTargetEuler;
    private string rightKneeTargetEuler;
    
    private string leftLegTargetEuler;
    private string leftKneeTargetEuler;

   

    // Update is called once per frame
    void Update()
    {
        #region 手臂

        if (pressMouseLeft)
        {
            leftArm.targetRotation = Quaternion.Euler(leftArmLiftEuler);
        }
        else
        {
            leftArm.targetRotation = Quaternion.Euler(leftArmIdleEuler);
        }
        
        if (pressMouseRight)
        {
            rightArm.targetRotation = Quaternion.Euler(rightArmLiftEuler);
        }
        else
        {
            rightArm.targetRotation = Quaternion.Euler(rightArmIdleEuler);
        }
        

        #endregion
        
        #region 腰部
        var hipRotateAction = playerInputActions.Player.HipRotate;
        var hipRotateAxisValue = hipRotateAction.ReadValue<Vector2>().y;
        // 获取触发动作的控件
        var control = hipRotateAction.activeControl;
        

        if (control != null)
        {
            if (control.device is Mouse)
            {
                // 如果是鼠标，处理鼠标滚轮
                hipEulerX += hipRotateAxisValue*scrollMultiplier;
            }
            else if (control.device is Keyboard)
            {
                // 如果是键盘，处理键盘按键
                hipEulerX += hipRotateAxisValue; 
            }
        }

        hipEulerX = Mathf.Clamp(hipEulerX, rotateXLimit.x, rotateXLimit.y);
        spine.targetRotation=Quaternion.Euler(new Vector3(hipEulerX,0,0));
        

        #endregion

        #region 腿部
        running=false;
        if (pressD)
        {
            rightLeg.targetRotation = Quaternion.Euler(rightLegRunEuler);
            rightKnee.targetRotation = Quaternion.Euler(rightKneeRunEuler);
            running = true;

            //Debug
            rightLegTargetEuler = nameof(rightLegRunEuler);
            rightKneeTargetEuler = nameof(rightKneeRunEuler);
        }
        else
        {
            rightLeg.targetRotation=Quaternion.Euler(rightLegIdleEuler);

            //Debug
            rightLegTargetEuler = nameof(rightLegIdleEuler);
            
            if (!pressA)
            {
                rightKnee.targetRotation = Quaternion.Euler(rightKneeIdleEuler);
                rightKneeTargetEuler = nameof(rightKneeIdleEuler);
            }
            else
            {
                rightKnee.targetRotation=Quaternion.Euler(rightKneeUpEuler);
                rightKneeTargetEuler = nameof(rightKneeUpEuler);
            }
            
           
            
        }
        
        if (pressA)
        {
            leftLeg.targetRotation = Quaternion.Euler(leftLegRunEuler);
            leftKnee.targetRotation = Quaternion.Euler(leftKneeRunEuler);
            running = true;
            
            //Debug
            leftLegTargetEuler = nameof(leftLegRunEuler);
            leftKneeTargetEuler = nameof(leftKneeRunEuler);
        }
        else
        {
            leftLeg.targetRotation=Quaternion.Euler(leftLegIdleEuler);
            leftLegTargetEuler = nameof(leftLegIdleEuler);
            
            if (!pressD)
            {
                leftKnee.targetRotation = Quaternion.Euler(leftKneeIdleEuler);
                leftKneeTargetEuler = nameof(leftKneeIdleEuler);
            }
            else
            {
                leftKnee.targetRotation = Quaternion.Euler(leftKneeUpEuler);
                leftKneeTargetEuler = nameof(leftKneeUpEuler);
            }
            
        }
        

        #endregion
        

    }

    private void OnGUI()
    {
        string ret=rightLegTargetEuler+"\n" + rightKneeTargetEuler + "\n \n" + leftLegTargetEuler + "\n" +
            leftKneeTargetEuler;
        GUI.Label(new Rect(500,500,300,300),ret);
    }
}
