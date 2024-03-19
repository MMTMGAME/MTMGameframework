using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using GameFramework.Event;
using GameMain;
using UnityEngine;

public class SceneCam : Entity
{
   public CinemachineVirtualCamera cinemachine;

   public CinemachineTargetGroup cinemachineTargetGroup;

   public ParticleSystem speedLineFx;

   private Cinemachine3rdPersonFollow thirdPerson;

   public float barrageModeHeight = 9.17f;
   public float barrageModeZDistance = -10.64f;
   private float normalModeHeight;
   private float normalModeZDistance;


   private void Start()
   {
      thirdPerson=cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
      normalModeHeight = thirdPerson.VerticalArmLength;
      normalModeZDistance = thirdPerson.ShoulderOffset.z;
   }

   private void OnEnable()
   {
      GameEntry.Event.Subscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
      GameEntry.Event.Subscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
   }

   private void OnDisable()
   {
      GameEntry.Event.Unsubscribe(PlayerEnterBarrageRoadEvtArgs.EventId,OnPlayerEnterBarrageRoad);
      GameEntry.Event.Unsubscribe(PlayerExitBarrageRoadEvtArgs.EventId,OnPlayerExitBarrageRoad);
   }

   void OnPlayerEnterBarrageRoad(object sender,GameEventArgs args)
   {
      DOTween.To(() => thirdPerson.VerticalArmLength, (x) => thirdPerson.VerticalArmLength = x, barrageModeHeight, 1);
      DOTween.To(() => thirdPerson.ShoulderOffset.z, (x) => thirdPerson.ShoulderOffset.z = x, barrageModeZDistance, 1);
     
   }
   
   void OnPlayerExitBarrageRoad
      (object sender,GameEventArgs args)
   {
      DOTween.To(() => thirdPerson.VerticalArmLength, (x) => thirdPerson.VerticalArmLength = x, normalModeHeight, 1);
      DOTween.To(() => thirdPerson.ShoulderOffset.z, (x) => thirdPerson.ShoulderOffset.z = x, normalModeZDistance, 1);

   }

   protected override void OnInit(object userData)
   {
      base.OnInit(userData);
      cinemachine = GetComponentInChildren<CinemachineVirtualCamera>();
      cinemachineTargetGroup = GetComponentInChildren<CinemachineTargetGroup>();
      cinemachine.LookAt = cinemachineTargetGroup.Transform;

      speedLineFx = GetComponentInChildren<ParticleSystem>();
      
      speedLineFx.gameObject.SetActive(false);
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

   public void SwitchSpeedline(bool status)
   {
      speedLineFx.gameObject.SetActive(status);
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
