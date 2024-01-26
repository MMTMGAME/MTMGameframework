using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SceneCam : MonoBehaviour
{
   public CinemachineVirtualCamera cinemachine;

   public CinemachineTargetGroup cinemachineTargetGroup;
   public void SetFollow(Transform target)
   {
      cinemachine.Follow = target;
   }

   private void Start()
   {
      cinemachine.LookAt = cinemachineTargetGroup.Transform;
   }

   public List<Transform> aimTargets = new List<Transform>();
   public List<float> weights = new List<float>();
   public void AddToTargetGroup(Transform trans,float weight=1)
   {
      aimTargets.Add(trans);
      weights.Add(weight);
         UpdateTargetGroup();
   }

   public void RemoveFromTargetGroup(Transform trans)
   {
      var index = aimTargets.FindIndex(x => x == trans);
      Debug.Log("index:"+index);
      if (index >= 0)
      {
         aimTargets.RemoveAt(index);
         weights.RemoveAt(index);
         UpdateTargetGroup();
      }
      
      
   }

   public void UpdateTargetGroup()
   {
      
      cinemachineTargetGroup.m_Targets = new CinemachineTargetGroup.Target[aimTargets.Count];
      for (int i = 0; i < aimTargets.Count; i++)
      {
         cinemachineTargetGroup.m_Targets[i].target = aimTargets[i];
         cinemachineTargetGroup.m_Targets[i].weight = weights[i];
         cinemachineTargetGroup.m_Targets[i].radius = 0; // Set the radius if needed
      }
   }
   
   
}
