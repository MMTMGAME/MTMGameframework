using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAnim : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;
    private UnityEngine.AI.NavMeshAgent NavMeshAgent
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
        if (ChaState != null && NavMeshAgent != null)
        {
            ChaState.AddAnimOrder(UnitAnim.AnimOrderType.Float,"Speed",NavMeshAgent.velocity.magnitude);

        }
    }
}
