using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIRotate : MonoBehaviour
{
    private UnitRotate m_UnitRotate;
    private bool rotationDisabled = false;  // Flag to ensure updateRotation is disabled only once

    private UnitRotate UnitRotate
    {
        get
        {
            if (m_UnitRotate == null)
            {
                m_UnitRotate = GetComponent<UnitRotate>();
            }
            return m_UnitRotate;
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

    // FixedUpdate is used for physics-related updates
    void FixedUpdate()
    {
        if(NavMeshAgent==null)
            return;
        
        // Ensure the NavMeshAgent is available and updateRotation is disabled
        if (!rotationDisabled)
        {
            NavMeshAgent.updateRotation = false;
            NavMeshAgent.updateUpAxis = false;
            
            rotationDisabled = true;  // Set flag to true after disabling rotation
        }
       

        if (UnitRotate != null/* && NavMeshAgent.desiredVelocity.sqrMagnitude > 0.1f*/)
        {
            // Calculate the rotation from the NavMeshAgent's desired velocity
            var targetDir = NavMeshAgent.desiredVelocity.normalized;
            
           
            
            
            Quaternion targetRotation=transform.rotation;
            if (targetDir != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(targetDir);
            }
            
            
            if(ChaState)
                ChaState.OrderRotateTo(targetRotation);
        }
    }
}