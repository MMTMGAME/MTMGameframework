using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameMain;
using UnityEngine;

public class SceneCam : Entity
{
   public CinemachineVirtualCamera cinemachine;

   public CinemachineTargetGroup cinemachineTargetGroup;

   protected override void OnInit(object userData)
   {
      base.OnInit(userData);
      cinemachine = GetComponentInChildren<CinemachineVirtualCamera>();
      cinemachineTargetGroup = GetComponentInChildren<CinemachineTargetGroup>();
      cinemachine.LookAt = cinemachineTargetGroup.Transform;
   }

   public void SetFollow(Transform target)
   {
      cinemachine.Follow = target;
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

   void UpdateTargetGroup()
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
