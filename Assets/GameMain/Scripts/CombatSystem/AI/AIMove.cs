using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityGameFramework.Runtime;

public class AIMove : MonoBehaviour
{
    private UnitMove m_UnitMove;

    protected GroundChecker groundChecker;

    protected Rigidbody rb;

    protected Animator animator;

    private void Awake()
    {
        groundChecker = gameObject.GetOrAddComponent<GroundChecker>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        groundChecker.onGroundedStatusChange += OnGroundedStatusChange;
        
    }

    private void OnDisable()
    {
        groundChecker.onGroundedStatusChange -= OnGroundedStatusChange;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }


    protected UnitMove UnitMove
    {
        get
        {
            if (m_UnitMove == null)
            {
                m_UnitMove = GetComponent<UnitMove>();
            }
            return m_UnitMove;
        }
    }

    private NavMeshAgent m_NavMeshAgent;
    protected NavMeshAgent NavMeshAgent
    {
        get
        {
            if (m_NavMeshAgent == null)
            {
                m_NavMeshAgent = GetComponent<NavMeshAgent>();
            }
            return m_NavMeshAgent;
        }
    }

    private ChaState m_ChaState;
    private static readonly int IsGround = Animator.StringToHash("IsGround");

    protected ChaState ChaState
    {
        get
        {
            if (m_ChaState == null)
            {
                m_ChaState = GetComponent<ChaState>();
            }
            return m_ChaState;
        }
    }

    
   

    // FixedUpdate is used for physics-based update
    public virtual void FixedUpdate()
    {
        if (UnitMove != null && NavMeshAgent != null)
        {
            // Disable NavMeshAgent's automatic position updating
            if (NavMeshAgent.updatePosition != false)
            {
                NavMeshAgent.updatePosition = false;
            }

            // Synchronize NavMeshAgent's speed with CharacterState's speed if available
            if (ChaState != null)
            {
                NavMeshAgent.speed = ChaState.MoveSpeed;
            }

           
            // Use UnitMove to manually update the position
            ChaState.OrderMove(NavMeshAgent.desiredVelocity );
            
            // Set the nextPosition for the NavMeshAgent
            NavMeshAgent.nextPosition = transform.position ;

        }
    }

    private void Update()
    {
        animator.SetBool(IsGround,groundChecker.grounded);
        try
        {
            if (groundChecker.grounded)
            {
                
                // if (ChaState.GetBuffById("CollideAnimHandle").Count > 0)
                // {
                //     Debug.LogError("AIMove_update_grounded_Trigger_Bounce");
                //     ChaState.AddAnimOrder(UnitAnim.AnimOrderType.Float, "BounceSpeed", rb.velocity.magnitude);
                //     ChaState.AddAnimOrder(UnitAnim.AnimOrderType.Trigger, "Bounce");
                // }
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnGroundedStatusChange(bool status)
    {
        if (status)
        {
            //落地

            if (ChaState)
            {
               
              
            }
           
        }
        else
        {
           
        }
    }
   

}