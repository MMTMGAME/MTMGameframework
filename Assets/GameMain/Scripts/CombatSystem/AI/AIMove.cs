using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMove : MonoBehaviour
{
    private UnitMove m_UnitMove;

    private UnitMove UnitMove
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
    private NavMeshAgent NavMeshAgent
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
    private ChaState ChaState
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
    void FixedUpdate()
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
}