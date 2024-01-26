//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using System.Reflection;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId = 0;

        public static Entity GetGameEntity(this EntityComponent entityComponent, int entityId)
        {
            UnityGameFramework.Runtime.Entity entity = entityComponent.GetEntity(entityId);
            if (entity == null)
            {
                return null;
            }

            return (Entity)entity.Logic;
        }

        public static void HideEntity(this EntityComponent entityComponent, Entity entity)
        {
            entityComponent.HideEntity(entity.Entity);
        }

        public static void AttachEntity(this EntityComponent entityComponent, Entity entity, int ownerId, string parentTransformPath = null, object userData = null)
        {
            entityComponent.AttachEntity(entity.Entity, ownerId, parentTransformPath, userData);
        }

       

        public static void ShowWeapon(this EntityComponent entityComponent, WeaponData data)
        {
            entityComponent.ShowEntity(typeof(Weapon), "Weapon", Constant.AssetPriority.WeaponAsset, data);
        }

        public static void ShowArmor(this EntityComponent entityComponent, ArmorData data)
        {
            entityComponent.ShowEntity(typeof(Armor), "Armor", Constant.AssetPriority.ArmorAsset, data);
        }

        public static void ShowBullet(this EntityComponent entityCompoennt, BulletData data)
        {
            entityCompoennt.ShowEntity(typeof(Bullet), "Bullet", Constant.AssetPriority.BulletAsset, data);
        }
        
        public static void ShowPhysicsBullet(this EntityComponent entityCompoennt, PhysicsBulletData data)
        {
            entityCompoennt.ShowEntity(typeof(PhysicsBullet), "Bullet", Constant.AssetPriority.BulletAsset, data);
        }
        
        public static void ShowFirecrackerBullet(this EntityComponent entityCompoennt, PhysicsBulletData data)
        {
            entityCompoennt.ShowEntity(typeof(FirecrackerBullet), "Bullet", Constant.AssetPriority.BulletAsset, data);
        }

        
        public static void ShowSceneItem<T>(this EntityComponent entityComponent, SceneItemData data) where T : SceneItem
        {
            entityComponent.ShowEntity(typeof(T), "SceneItem", Constant.AssetPriority.SceneAsset, data);
        }


        public static void ShowSceneItemByName(this EntityComponent entityComponent, string typeName, SceneItemData data)
        {
            // 获取类型
            Type type = Type.GetType(typeName);
            if (type != null && typeof(SceneItem).IsAssignableFrom(type))
            {
                // 获取 ShowSceneItem<T> 的方法信息
                MethodInfo methodInfo = typeof(EntityComponent).GetMethod(nameof(ShowSceneItem), BindingFlags.Public | BindingFlags.Static);
                // 构造泛型方法
                MethodInfo genericMethod = methodInfo.MakeGenericMethod(new Type[] { type });
                // 调用方法
                genericMethod.Invoke(entityComponent, new object[] { data });
            }
            else
            {
                Debug.LogError("Invalid type for ShowSceneItem: " + typeName);
            }
        }


        public static void ShowPlayer(this EntityComponent entityCompoennt, PlayerData data)
        {
            entityCompoennt.ShowEntity(typeof(Player), "Player", Constant.AssetPriority.MyAircraftAsset, data);
        }
        public static void ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, data);
        }

        public static void ShowOldMan(this EntityComponent entityComponent, OldManData data)
        {
            entityComponent.ShowEntity(typeof(OldMan), "Neutral", Constant.AssetPriority.AircraftAsset, data);
        }

        private static void ShowEntity(this EntityComponent entityComponent, Type logicType, string entityGroup, int priority, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity drEntity = dtEntity.GetDataRow(data.TypeId);
            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }

            entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drEntity.AssetName), entityGroup, priority, data);
        }

        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return --s_SerialId;
        }
    }
}
