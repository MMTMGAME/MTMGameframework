//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// AI 工具类。
    /// </summary>
    public static class AIUtility
    {
        private static Dictionary<CampPair, RelationType> s_CampPairToRelation = new Dictionary<CampPair, RelationType>();
        private static Dictionary<KeyValuePair<CampType, RelationType>, CampType[]> s_CampAndRelationToCamps = new Dictionary<KeyValuePair<CampType, RelationType>, CampType[]>();

        static AIUtility()
        {
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Player2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Player2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Enemy2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral2), RelationType.Hostile);

            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Player2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Enemy2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral2, CampType.Neutral2), RelationType.Neutral);
        }

        /// <summary>
        /// 获取两个阵营之间的关系。
        /// </summary>
        /// <param name="first">阵营一。</param>
        /// <param name="second">阵营二。</param>
        /// <returns>阵营间关系。</returns>
        public static RelationType GetRelation(CampType first, CampType second)
        {
            if (first > second)
            {
                CampType temp = first;
                first = second;
                second = temp;
            }

            RelationType relationType;
            if (s_CampPairToRelation.TryGetValue(new CampPair(first, second), out relationType))
            {
                return relationType;
            }

            Log.Warning("Unknown relation between '{0}' and '{1}'.", first.ToString(), second.ToString());
            return RelationType.Unknown;
        }

        /// <summary>
        /// 获取和指定具有特定关系的所有阵营。
        /// </summary>
        /// <param name="camp">指定阵营。</param>
        /// <param name="relation">关系。</param>
        /// <returns>满足条件的阵营数组。</returns>
        public static CampType[] GetCamps(CampType camp, RelationType relation)
        {
            KeyValuePair<CampType, RelationType> key = new KeyValuePair<CampType, RelationType>(camp, relation);
            CampType[] result = null;
            if (s_CampAndRelationToCamps.TryGetValue(key, out result))
            {
                return result;
            }

            // TODO: GC Alloc
            List<CampType> camps = new List<CampType>();
            Array campTypes = Enum.GetValues(typeof(CampType));
            for (int i = 0; i < campTypes.Length; i++)
            {
                CampType campType = (CampType)campTypes.GetValue(i);
                if (GetRelation(camp, campType) == relation)
                {
                    camps.Add(campType);
                }
            }

            // TODO: GC Alloc
            result = camps.ToArray();
            s_CampAndRelationToCamps[key] = result;

            return result;
        }

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


        public static void Attack(TargetableObject attacker, TargetableObject victim)
        {
            if (attacker == null || victim == null)
            {
                return;
            }
            
            ImpactData attackerImpactData = attacker.GetImpactData();
            ImpactData victimImpactData = victim.GetImpactData();
            if (GetRelation(attackerImpactData.Camp, victimImpactData.Camp) == RelationType.Friendly)
            {
                return;
            }

           
            int targetDamageHP = CalcDamageHP(attackerImpactData.Attack, victimImpactData.Defense);

           

           
            victim.ApplyDamage(attacker, targetDamageHP);
            
        }
        
        
       

        public static void ExplosionWithForce(TargetableObject attacker, Vector3 center, float radius, int power)
        {
            HashSet<Entity> damagedEntities = new HashSet<Entity>();
            Dictionary<Rigidbody, Vector3> forceOnRigidbodies = new Dictionary<Rigidbody, Vector3>();

            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            foreach (var hitCollider in hitColliders)
            {
                bool hitSuccess = false;
                if (Physics.Linecast(center, hitCollider.transform.position, out RaycastHit hit))
                {
                    if (hitCollider == hit.collider)
                    {
                        hitSuccess = true;
                        Debug.Log("爆炸投射成功：" + hit.collider.name);
                    }
                    else
                    {
                        Debug.Log("和" + hitCollider.name + "之间有障碍物：" + hit.collider.name);
                    }
                }

                if (hitSuccess)
                {
                    TargetableObject entity = hitCollider.GetComponentInParent<TargetableObject>();
                    if (entity != null && !damagedEntities.Contains(entity))
                    {
                        var realDamage = CalcDamageHP(power, attacker.GetImpactData().Defense);
                        entity.ApplyDamage(attacker, realDamage);
                        damagedEntities.Add(entity);
                    }

                    // 计算物理力并记录
                    Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
                    if (hitRigidbody != null)
                    {
                        Vector3 forceDirection = (hitCollider.transform.position - center).normalized;
                        Vector3 force = forceDirection *
                                        CalculateForce(power, hitRigidbody, center,
                                            hitCollider.transform.position); // CalculateForce 是计算力的方法

                        if (forceOnRigidbodies.ContainsKey(hitRigidbody))
                        {
                            forceOnRigidbodies[hitRigidbody] += force;
                        }
                        else
                        {
                            forceOnRigidbodies.Add(hitRigidbody, force);
                        }
                    }
                }
            }

            // 对每个 Rigidbody 施加合力
            foreach (var rbForcePair in forceOnRigidbodies)
            {
                rbForcePair.Key.AddForce(rbForcePair.Value);
            }
        }

        // 计算力的方法（示例，您可以根据需求调整）
        private static float CalculateForce(int power, Rigidbody rb, Vector3 explosionCenter, Vector3 colliderPosition)
        {
            float distance = Vector3.Distance(explosionCenter, colliderPosition);
            float forceMagnitude = power / distance; // 举例：力量随距离递减
            return forceMagnitude * rb.mass; // 根据质量调整力的大小
        }

        
        // 爆炸效果实现
        public static void Explosion(TargetableObject attacker,Vector3 center, float radius,int power)
        {
            // 记录已经受到伤害的实体，以确保同一个实体只受到一次伤害
            HashSet<Entity> damagedEntities = new HashSet<Entity>();

            // 球形检测以找到爆炸半径内的所有碰撞体
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            foreach (var hitCollider in hitColliders)
            {
                bool hitSuccess = false;
                // 射线检测以确定是否有遮挡物
                if (Physics.Linecast(center, hitCollider.transform.position, out RaycastHit hit))
                {
                    if (hitCollider == hit.collider)
                    {
                        hitSuccess = true;
                        Debug.Log("爆炸投射成功：" + hit.collider.name);
                    }
                    else
                    {
                        Debug.Log("和"+hitCollider.name+"之间有障碍物："+hit.collider.name);
                    }
                   
                }
                if(hitSuccess)
                {
                    // 检测到的碰撞体就是最近的对象，没有遮挡物

                    // 获取实体组件
                    TargetableObject entity = hitCollider.GetComponentInParent<TargetableObject>();
                    if (entity != null && !damagedEntities.Contains(entity))
                    {
                        // 对实体造成伤害
                        var realDamage = CalcDamageHP(power,
                            attacker.GetImpactData().Defense);
                        entity.ApplyDamage(attacker, realDamage);

                        // 标记该实体已受伤害
                        damagedEntities.Add(entity);
                    }
                }
            }
        }

        private static int CalcDamageHP(int attack, int defense)
        {
            if (attack <= 0)
            {
                return 0;
            }

            if (defense < 0)
            {
                defense = 0;
            }

            return attack * attack / (attack + defense);
        }

        [StructLayout(LayoutKind.Auto)]
        private struct CampPair
        {
            private readonly CampType m_First;
            private readonly CampType m_Second;

            public CampPair(CampType first, CampType second)
            {
                m_First = first;
                m_Second = second;
            }

            public CampType First
            {
                get
                {
                    return m_First;
                }
            }

            public CampType Second
            {
                get
                {
                    return m_Second;
                }
            }
        }
    }
}
