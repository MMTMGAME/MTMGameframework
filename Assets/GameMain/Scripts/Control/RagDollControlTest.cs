using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollControlTest : MonoBehaviour
{
    [Header("---RightLeg---")] 
    public ConfigurableJoint rightLeg;

    public Vector3 rightLegIdleEuler = new Vector3(0, 0, 0);

    [Header("---LeftLeg---")]
    public ConfigurableJoint leftLeg;
    
    public Vector3 leftLegIdleEuler = new Vector3(0, 0, 0);
    
  
  
    
    private void FixedUpdate()
    {
        
        var leftLegTargetRotation = Quaternion.Euler(leftLegIdleEuler);
        leftLeg.targetRotation = leftLegTargetRotation;
        
       var rightLegTargetRotation = Quaternion.Euler(rightLegIdleEuler);
       rightLeg.targetRotation = rightLegTargetRotation;

      
    }
}
