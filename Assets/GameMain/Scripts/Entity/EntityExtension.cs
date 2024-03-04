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
        
        
        public static void ShowPlayer(this EntityComponent entityCompoennt, PlayerData data)
        {
            entityCompoennt.ShowEntity(typeof(Player), "Player", Constant.AssetPriority.PlayerAsset, data);
        }
        
        public static void ShowSceneCam(this EntityComponent entityCompoennt)
        {
            entityCompoennt.ShowEntity(typeof(SceneCam), "Camera", Constant.AssetPriority.CameraAsset, new SceneCamData(GameEntry.Entity.GenerateSerialId(), 10001));
        }

        public static void ShowDebugSphere(this EntityComponent entityCompoennt,Vector3 pos,Quaternion rotation,float duration)
        {
            entityCompoennt.ShowEntity(typeof(DebugSphere), "Debug", Constant.AssetPriority.EffectAsset,new DebugSphereData(GameEntry.Entity.GenerateSerialId(),1,duration)
            {
                Position = pos,
                Rotation = rotation
            });
        }
        public static void ShowDebug3DText(this EntityComponent entityCompoennt,Vector3 pos,Quaternion rotation,string msg,float duration)
        {
            entityCompoennt.ShowEntity(typeof(Debug3DText), "Debug", Constant.AssetPriority.EffectAsset,new Debug3DTextData(GameEntry.Entity.GenerateSerialId(),
                2,msg,duration)
            {
                Position = pos,
                Rotation = rotation
            });
        }
        
        public static void ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, data);
        }

        public static void ShowBattleUnit(this EntityComponent entityComponent, BattleUnitData data)
        {
            entityComponent.ShowEntity(typeof(BattleUnit), "BattleUnit", Constant.AssetPriority.BattleUnitAsset, data);
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
