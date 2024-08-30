//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameFramework.Entity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// AI 工具类。
    /// </summary>
    [IncludeInSettings(true)]
    public static class AIUtility
    {
        

        // /// <summary>
        // /// 获取和指定具有特定关系的所有阵营。
        // /// </summary>
        // /// <param name="camp">指定阵营。</param>
        // /// <param name="relation">关系。</param>
        // /// <returns>满足条件的阵营数组。</returns>
        // public static CampType[] GetCamps(CampType camp, RelationType relation)
        // {
        //     KeyValuePair<CampType, RelationType> key = new KeyValuePair<CampType, RelationType>(camp, relation);
        //     CampType[] result = null;
        //     if (s_CampAndRelationToCamps.TryGetValue(key, out result))
        //     {
        //         return result;
        //     }
        //
        //     // TODO: GC Alloc
        //     List<CampType> camps = new List<CampType>();
        //     Array campTypes = Enum.GetValues(typeof(CampType));
        //     for (int i = 0; i < campTypes.Length; i++)
        //     {
        //         CampType campType = (CampType)campTypes.GetValue(i);
        //         if (GetRelation(camp, campType) == relation)
        //         {
        //             camps.Add(campType);
        //         }
        //     }
        //
        //     // TODO: GC Alloc
        //     result = camps.ToArray();
        //     s_CampAndRelationToCamps[key] = result;
        //
        //     return result;
        // }

        /// <summary>
        /// 获取实体间的距离。
        /// </summary>
        /// <returns>实体间的距离。</returns>
        public static float GetDistance(Entity fromEntity, Entity toEntity)
        {
            Transform fromTransform = fromEntity.CachedTransform;
            Transform toTransform = toEntity.CachedTransform;
            return (toTransform.position - fromTransform.position).magnitude;
        }


        public static Vector3 RandomNavMeshPos(Vector3 center, float radius)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            randomDirection += center;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 1000, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                // 在初次采样失败后处理
                const int maxAttempts = 30;
                for (int i = 0; i < maxAttempts; i++)
                {
                    randomDirection = UnityEngine.Random.insideUnitSphere * radius;
                    randomDirection += center;

                    if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }
                }
        
                // 如果多次尝试都失败，可以返回中心点或者其他默认位置
                Debug.LogWarning("Failed to find a valid NavMesh position after multiple attempts.");
                return center;
            }
        }


        public static Vector3 GetClosestValidPoint(Vector3 origin)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(origin, out hit, 100f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                return origin;
            }
        }

        private static List<IEntity> cachedEntities = new List<IEntity>();
        private static float lastUpdateTime = 0;
        /// <summary>
        /// 寻找目标
        /// </summary>
        /// <param name="selfCampType"></param>
        /// <param name="relationType"></param>
        public static BattleUnit FindBattleUnit(BattleUnit self, RelationType relationTypes, Vector3 center, float radius)
        {
            if (Time.time > lastUpdateTime + 0.5f) // 每0.5秒更新一次，降低消耗
            {
                
                var battleUnitList = GameEntry.Entity.GetEntityGroup("ChaObj").GetAllEntities();
        
                cachedEntities.Clear();
                
                cachedEntities.AddRange(battleUnitList);
        
                lastUpdateTime = Time.time;
            }
        
            foreach (var entity in cachedEntities)
            {
                if (entity == null || entity.IsUnityNull())
                {
                    continue;
                }
                var entityLogic = ((UnityGameFramework.Runtime.Entity)entity).Logic;
                if(entityLogic==null || entityLogic==self)
                    continue;
                if ( entity.Handle.IsUnityNull()==false && entityLogic is BattleUnit targetableObject)
                {
                    if ( Vector3.SqrMagnitude(((GameObject)entity.Handle).transform.position - center) < radius * radius 
                        && (CombatComponent.GetRelation(self.GetBattleUnitData().Camp, targetableObject.GetBattleUnitData().Camp) & relationTypes) != RelationType.None 
                        && targetableObject.Available && !targetableObject.chaState.dead)
                    {
                        return targetableObject;
                    }
                }
            }
        
            return null;
        }
        
        public static List<BattleUnit> FindBattleUnits(BattleUnit self, RelationType relationTypes, Vector3 center, float radius)
        {
            List<BattleUnit> ret = new List<BattleUnit>(); 
            if (Time.time > lastUpdateTime + 0.5f) // 每0.5秒更新一次，降低消耗
            {
                
                var battleUnitList = GameEntry.Entity.GetEntityGroup("ChaObj").GetAllEntities();
        
                cachedEntities.Clear();
                
                cachedEntities.AddRange(battleUnitList);
        
                lastUpdateTime = Time.time;
            }
        
            foreach (var entity in cachedEntities)
            {
                if ( entity!=null && entity.IsUnityNull()==false && ((UnityGameFramework.Runtime.Entity)entity).Logic is BattleUnit targetableObject)
                {
                    if (Vector3.SqrMagnitude(((GameObject)entity.Handle).transform.position - center) < radius * radius 
                        && (CombatComponent.GetRelation(self.GetBattleUnitData().Camp, targetableObject.GetBattleUnitData().Camp) & relationTypes) != RelationType.None 
                        && targetableObject.Available && !targetableObject.chaState.dead)
                    {
                       ret.Add(targetableObject);
                    }
                }
            }
        
            return ret;
        }


        
       

        // /// <summary>
        // /// 爆炸并施加力
        // /// </summary>
        // /// <param name="attacker"></param>
        // /// <param name="center"></param>
        // /// <param name="radius"></param>
        // /// <param name="power"></param>
        // /// <param name="useLineCast"></param>
        // public static void ExplosionWithForce(BattleUnit attacker, Vector3 center, float radius, int power,bool useLineCast)
        // {
        //     HashSet<Entity> damagedEntities = new HashSet<Entity>();
        //     Dictionary<Rigidbody, Vector3> forceOnRigidbodies = new Dictionary<Rigidbody, Vector3>();
        //
        //     Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        //     foreach (var hitCollider in hitColliders)
        //     {
        //         bool hitSuccess = false;
        //         if (Physics.Linecast(center, hitCollider.transform.position, out RaycastHit hit))
        //         {
        //             if (hitCollider == hit.collider)
        //             {
        //                 hitSuccess = true;
        //                 Debug.Log("爆炸投射成功：" + hit.collider.name);
        //             }
        //             else
        //             {
        //                 Debug.Log("和" + hitCollider.name + "之间有障碍物：" + hit.collider.name);
        //             }
        //         }
        //         if (!useLineCast)
        //             hitSuccess = true;
        //
        //         if (hitSuccess)
        //         {
        //             BattleUnit entity = hitCollider.GetComponentInParent<BattleUnit>();
        //             if (entity != null && !damagedEntities.Contains(entity))
        //             {
        //                 var realDamage = CalcDamageHP(power, attacker.GetImpactData().Defense);
        //                 entity.ApplyDamage(attacker, realDamage);
        //                 damagedEntities.Add(entity);
        //             }
        //
        //             // 计算物理力并记录
        //             Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
        //             if (hitRigidbody != null)
        //             {
        //                 Vector3 forceDirection = (hitCollider.transform.position - center).normalized;
        //                 Vector3 force = forceDirection *
        //                                 CalculateForce(power, hitRigidbody, center,
        //                                     hitCollider.transform.position); // CalculateForce 是计算力的方法
        //
        //                 if (forceOnRigidbodies.ContainsKey(hitRigidbody))
        //                 {
        //                     forceOnRigidbodies[hitRigidbody] += force;
        //                 }
        //                 else
        //                 {
        //                     forceOnRigidbodies.Add(hitRigidbody, force);
        //                 }
        //             }
        //         }
        //     }
        //
        //     // 对每个 Rigidbody 施加合力
        //     foreach (var rbForcePair in forceOnRigidbodies)
        //     {
        //         rbForcePair.Key.AddForce(rbForcePair.Value);
        //     }
        // }
        //
        // // 计算力的方法（示例，您可以根据需求调整）
        // private static float CalculateForce(int power, Rigidbody rb, Vector3 explosionCenter, Vector3 colliderPosition)
        // {
        //     float distance = Vector3.Distance(explosionCenter, colliderPosition);
        //     float forceMagnitude = power / distance; // 举例：力量随距离递减
        //     return forceMagnitude * rb.mass; // 根据质量调整力的大小
        // }
        //
        //
        // /// <summary>
        // /// 爆炸效果实现，useLinecast决定是否被射线遮挡，为true是被遮挡时不造成伤害
        // /// </summary>
        // /// <param name="attacker"></param>
        // /// <param name="center"></param>
        // /// <param name="radius"></param>
        // /// <param name="power">攻击力</param>
        // /// <param name="useLineCast"></param>
        // public static void Explosion(BattleUnit attacker,Vector3 center, float radius,int power,bool useLineCast)
        // {
        //     // 记录已经受到伤害的实体，以确保同一个实体只受到一次伤害
        //     HashSet<Entity> damagedEntities = new HashSet<Entity>();
        //
        //     // 球形检测以找到爆炸半径内的所有碰撞体
        //     Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        //     foreach (var hitCollider in hitColliders)
        //     {
        //         bool hitSuccess = false;
        //         // 射线检测以确定是否有遮挡物
        //         if (useLineCast && Physics.Linecast(center, hitCollider.transform.position, out RaycastHit hit))
        //         {
        //             if (hitCollider == hit.collider)
        //             {
        //                 hitSuccess = true;
        //                 Debug.Log("爆炸投射成功：" + hit.collider.name);
        //             }
        //             else
        //             {
        //                 Debug.Log("和"+hitCollider.name+"之间有障碍物："+hit.collider.name);
        //             }
        //            
        //         }
        //
        //         if (!useLineCast)
        //             hitSuccess = true;
        //         if(hitSuccess)
        //         {
        //             // 检测到的碰撞体就是最近的对象，没有遮挡物
        //
        //             // 获取实体组件
        //             TargetableObject entity = hitCollider.GetComponentInParent<TargetableObject>();
        //             if (entity != null && !damagedEntities.Contains(entity) )
        //             {
        //                 // 对实体造成伤害
        //                 var realDamage = CalcDamageHP(power,
        //                     attacker.GetImpactData().Defense);
        //                 entity.ApplyDamage(attacker, realDamage);
        //
        //                 // 标记该实体已受伤害
        //                 damagedEntities.Add(entity);
        //             }
        //         }
        //     }
        // }

      
    }
}
