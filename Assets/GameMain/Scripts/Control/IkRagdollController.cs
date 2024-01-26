using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;



public interface IUseRayPoint
{
    void Init(IkRagdollController ikRagdollController);
}


public class IkRagdollController : MonoBehaviour
{
    
    public enum LegPos{
        Back,
        Idle,
        Front
    }

    private Camera _mainCam; // 私有字段来存储相机的引用

    private Camera MainCam
    {
        get
        {
            if (_mainCam == null)
            {
                _mainCam = Camera.main;
            }
            return _mainCam;
        }
        set
        {
            _mainCam = value;
        }
    }

    private PlayerInputActions playerInputActions;

    

    [Header("---右腿---")] 
    public ConfigurableJoint rightLeg;

    private Rigidbody rightLegRigidbody;

    public Vector3 rightLegIdleEuler = new Vector3(0, 0, 0);
    public Vector3 rightLegFrontEuler = new Vector3(90, 0, 0);
    public Vector3 rightLegBehindEuler = new Vector3(-90, 0, 0);
    public Vector3 rightLegUpEuler = new Vector3(-90, 0, 0);
    
    [Header("---右膝盖---")]
    public ConfigurableJoint rightKnee;

    public Vector3 rightKneeIdleEuler = new Vector3(0, 0, 0);
    public Vector3 rightKneeFrontEuler = new Vector3(0, 0, 0);
    public Vector3 rightKneeBehindEuler = new Vector3(-90, 0, 0);
    public Vector3 rightKneeUpEuler = new Vector3(-90, 0, 0);
    
    [Header("---右脚---")]
    public ConfigurableJoint rightFoot;

    public Vector3 rightFootIdleEuler = new Vector3(0, 0, 0);
    public Vector3 rightFootFrontEuler = new Vector3(0, 0, 0);
    public Vector3 rightFootBehindEuler = new Vector3(-90, 0, 0);
    
    
    [Header("---左腿---")]
    public ConfigurableJoint leftLeg;

    private Rigidbody leftLegRigidbody;
    public Vector3 leftLegIdleEuler = new Vector3(0, 0, 0);
    public Vector3 leftLegFrontEuler = new Vector3(0, 0, 0);
    public Vector3 leftLegBehindEuler = new Vector3(-90, 0, 0);
    public Vector3 leftLegUpEuler = new Vector3(-90, 0, 0);
   
    
    [Header("---左膝盖---")]
    public ConfigurableJoint leftKnee;

    public Vector3 leftKneeIdleEuler = new Vector3(0, 0, 0);
    public Vector3 leftKneeFrontEuler = new Vector3(0, 0, 0);
    public Vector3 leftKneeBehindEuler = new Vector3(-90, 0, 0);
    public Vector3 leftKneeUpEuler = new Vector3(-90, 0, 0);
    
    [Header("---右脚---")]
    public ConfigurableJoint leftFoot;

    public Vector3 leftFootIdleEuler = new Vector3(0, 0, 0);
    public Vector3 leftFootFrontEuler = new Vector3(0, 0, 0);
    public Vector3 leftFootBehindEuler = new Vector3(-90, 0, 0);
    
    public Vector3 RayPoint { get; private set; }
    public void Awake()
    {
        MainCam = Camera.main;
        
        //初始化可控制骨骼
        var controlJoints = GetComponentsInChildren<IUseRayPoint>();
        foreach (var joint in controlJoints)
        {
            joint.Init(this);
        }

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Space.performed += RagdollTurn180;
        playerInputActions.Enable();

        leftLegRigidbody = leftLeg.GetComponent<Rigidbody>();
        rightLegRigidbody = rightLeg.GetComponent<Rigidbody>();
    }


    private Plane plane;

    private Quaternion leftLegInitialRotataion;
    private Quaternion rightLegInitialRotation;

    private float initialX;
    private void Start()
    {
        leftLegInitialRotataion = leftLeg.transform.localRotation;
        rightLegInitialRotation = rightLeg.transform.localRotation;
        plane = new Plane(hip.transform.right, hip.transform.position);
        initialX = hip.transform.position.x;
    }


    public ConfigurableJoint hip;
    private float curTurnYValue=0;
    
    void RagdollTurn180(InputAction.CallbackContext context)
    {
        StartCoroutine(SpineTurn());
    }

    IEnumerator SpineTurn()
    {
        playerInputActions.Disable();

        var originalPos = hip.transform.position;//转身前位置
        hip.xDrive = new JointDrive(){positionSpring = 0};
        
        
        var hipRigid = hip.GetComponent<Rigidbody>();
        Vector3 hitForce = Vector3.up * 8000; // forceMagnitude 是你想施加的力的大小
     
        hipRigid.AddForceAtPosition(hitForce, hipRigid.centerOfMass);
        Quaternion startRotation = hip.targetRotation;
        Quaternion endRotation = Quaternion.Euler(0, curTurnYValue + 180, 0);
        curTurnYValue += 180;

        float elapsedTime = 0f;
        float duration = 0.5f; // 旋转所需的时间，可以根据需要调整

        while (elapsedTime < duration)
        {
            hip.targetRotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        hip.targetRotation = endRotation; // 确保最终旋转是准确的
        //hipRigid.AddForceAtPosition(hitForce, hipRigid.centerOfMass);
        hip.xDrive = new JointDrive(){positionSpring = float.MaxValue,maximumForce = float.MaxValue,positionDamper = 3};

        hipRigid.constraints = RigidbodyConstraints.FreezePositionY;
        yield return new WaitForSeconds(1f);
        hipRigid.constraints = RigidbodyConstraints.None;
        //hip.targetPosition = originalPos- hip.transform.position ;
       // hipRigid.MovePosition(new Vector3(initialX,hip.transform.position.y,hip.transform.position.z));
        playerInputActions.Enable();
    }


    private LegPos lastLeftLegPos;
    private LegPos lastRightLegPos;

    private Quaternion rightLegTargetRotation;
    private Quaternion leftLegTargetRotation;
    private void FixedUpdate()
    {
        var mousePos = playerInputActions.Player.MouseDrag.ReadValue<Vector2>();
        //需要碰撞到物体才可以
        Ray ray = MainCam.ScreenPointToRay(mousePos);
        // RaycastHit hit;
        // bool isCollider = Physics.Raycast(ray, out hit,999,layerMask);
        // if (isCollider)
        // {
        //     //Debug.Log("射线检测到的点是"+hit.point+","+hit.collider.name);
        //     RayPoint = hit.point;
        // }
        
       
       

        float enter;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            //Debug.Log("射线检测到的点是"+hitPoint);
            RayPoint = hitPoint;
        }

        #region 腿部控制

        var pressZ = playerInputActions.Player.Z.IsPressed();
        var releaseZ = playerInputActions.Player.Z.WasReleasedThisFrame();
        var pressX = playerInputActions.Player.X.IsPressed();
        var releaseX = playerInputActions.Player.X.WasReleasedThisFrame();
        var pressC = playerInputActions.Player.C.IsPressed();
        var releaseC = playerInputActions.Player.C.WasReleasedThisFrame();
        var pressV = playerInputActions.Player.V.IsPressed();
        var releaseV = playerInputActions.Player.V.WasReleasedThisFrame();

        rightLegTargetRotation = Quaternion.Euler(rightLegIdleEuler);
        var rightKneeTargetRotation = Quaternion.Euler(rightKneeIdleEuler);
        var rightFootTargetRotation = Quaternion.Euler(rightFootIdleEuler);

        if (releaseX)
            lastRightLegPos = LegPos.Front;
        if (releaseZ)
            lastRightLegPos = LegPos.Back;

        if (releaseC)
        {
            lastLeftLegPos = LegPos.Front;
        }

        if (releaseV)
        {
            lastLeftLegPos = LegPos.Back;
        }
        

        if (!pressZ && !pressX)
        {
            rightLegTargetRotation = Quaternion.Euler(rightLegIdleEuler);
            rightKneeTargetRotation = Quaternion.Euler(rightKneeIdleEuler);
            
            rightFootTargetRotation = Quaternion.Euler(rightFootIdleEuler);

            // if (lastRightLegPos == LegPos.Back)//本来想搞一个退回位提高的代码的，但是想想调一下idle就行了。
            // {
            //     rightLeg.targetRotation=Quaternion.Euler(rightLegUpEuler);
            //     rightKnee.targetRotation = Quaternion.Euler(rightKneeUpEuler);
            // }

            
            
        }
        else
        {
            if (pressZ)
            {
                rightLegTargetRotation =quaternion.Euler(rightLegBehindEuler);
                rightKneeTargetRotation = quaternion.Euler(rightKneeBehindEuler);
                rightFootTargetRotation = Quaternion.Euler(rightFootBehindEuler);
            }
            if(pressX)
            {
                rightLegTargetRotation = Quaternion.Euler(rightLegFrontEuler);
                rightKneeTargetRotation = Quaternion.Euler(rightKneeFrontEuler);
                rightFootTargetRotation = Quaternion.Euler(rightFootFrontEuler);
            }
        }
        
        

        rightLeg.targetRotation =rightLegTargetRotation;
        rightKnee.targetRotation = rightKneeTargetRotation;
        rightFoot.targetRotation = rightFootTargetRotation;
        
        
        leftLegTargetRotation = Quaternion.Euler(leftLegIdleEuler);
        var leftKneeTargetRotation = Quaternion.Euler(leftKneeIdleEuler);
        var leftFootTargetRotation = Quaternion.Euler(leftFootIdleEuler);
        if (!pressC && !pressV)
        {
            leftLegTargetRotation = Quaternion.Euler(leftLegIdleEuler);
           
            leftKneeTargetRotation = Quaternion.Euler(leftKneeIdleEuler);
            leftFootTargetRotation = Quaternion.Euler(leftFootIdleEuler);
            
            // if (lastLeftLegPos == LegPos.Back)//本来想搞一个退回位提高的代码的，但是想想调一下idle就行了。
            // {
            //     leftLeg.targetRotation=Quaternion.Euler(leftLegUpEuler);
            //     leftKnee.targetRotation = Quaternion.Euler(leftKneeUpEuler);
            // }
        }
        else
        {
            
            if (pressC)
            {
                leftLegTargetRotation =quaternion.Euler(leftLegBehindEuler);
                
                leftKneeTargetRotation = quaternion.Euler(leftKneeBehindEuler);
                leftFootTargetRotation = Quaternion.Euler(leftFootBehindEuler);
            }
            if(pressV)
            {
                leftLegTargetRotation = Quaternion.Euler(leftLegFrontEuler);
                
                leftKneeTargetRotation = Quaternion.Euler(leftKneeFrontEuler);
                leftFootTargetRotation = Quaternion.Euler(leftFootFrontEuler);
            }
        }
        //Debug.Log(str);
        leftLeg.targetRotation = leftLegTargetRotation;
        leftKnee.targetRotation = leftKneeTargetRotation;
        leftFoot.targetRotation = leftFootTargetRotation;


        //leftLegRigidbody.AddRelativeTorque(-rightLeg.currentTorque); 

        #endregion
    }

    // void OnGUI()
    // {
    //     GUI.Label(new Rect(500,500,500,500),"rightLeg"+rightLegTargetRotation.eulerAngles + "\n"+
    //                                     "leftLeg"+leftLegTargetRotation.eulerAngles);
    // }

    

  
    
    private void OnDestroy()
    {
        playerInputActions.Player.Space.performed -= RagdollTurn180;
        playerInputActions.Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(RayPoint,Vector3.one*0.3f);
    }
}
