//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using System.Collections.Generic;
using System.Reflection;
using GameMain;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    [IncludeInSettings(true)]
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId = 0;
        
        //子弹击中特效表
        //将音效击中表进行映射
        private static Dictionary<string, Func<DRBulletImpactEffect, int>> materialToEffectIdMap;

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

        

        //固定Id
        public static void ShowBulletObj(this EntityComponent entityComponent,BulletLauncher bulletLauncher,Action<Entity> action)
        {
            entityComponent.ShowEntity(typeof(BulletObj), "BulletObj", Constant.AssetPriority.BulletAsset,new BulletObjData(GameEntry.Entity.GenerateSerialId(),bulletLauncher.model.entityTypeId,bulletLauncher)
            {
                Position = bulletLauncher.firePosition,
                OnShowCallBack = action
            });
        }

        public static int ShowAoeObj(this EntityComponent entityComponent, AoeLauncher aoeLauncher,
            Action<Entity> action)
        {
            var serialId = GameEntry.Entity.GenerateSerialId();
            entityComponent.ShowEntity(typeof(AoeObj), "AoeObj", Constant.AssetPriority.BulletAsset,new AoeObjData(serialId,aoeLauncher.model.entityTypeId,aoeLauncher)
            {
                Position = aoeLauncher.position,
                Rotation = aoeLauncher.rotation,
                OnShowCallBack = action
            });
            return serialId;
        }
        
        
        public static void ShowPlayer(this EntityComponent entityCompoennt, PlayerData data)
        {
            entityCompoennt.ShowEntity(typeof(Player), "ChaObj", Constant.AssetPriority.PlayerAsset, data);
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
        
        
        
        #region 音效击中表

        private static int GetEffectIdFromMaterial(string materialName, DRBulletImpactEffect dr)
        {
            // 确保字典已经初始化
            if (materialToEffectIdMap == null)
            {
                InitializeMaterialToEffectIdMap();
            }

            // 使用字典查询对应的函数并调用
            if (materialToEffectIdMap.TryGetValue(materialName, out var getEffectIdFunc))
            {
                return getEffectIdFunc(dr);
            }

            // 如果没有找到对应的材质名称，返回一个错误代码或默认值
            return dr.Default; // 或者根据实际情况选择合适的返回值
        }

        private static void InitializeMaterialToEffectIdMap()
        {
            materialToEffectIdMap = new Dictionary<string, Func<DRBulletImpactEffect, int>>
            {
                {"Default",dr=>dr.Default},
                {"Concrete", dr => dr.Concrete},
                {"Wood", dr => dr.Wood},
                {"Stone", dr => dr.Stone},
                {"Metal", dr => dr.Metal},
                {"Dirt", dr => dr.Dirt},
                {"Sand", dr => dr.Sand},
                {"Grass", dr => dr.Grass},
                {"Glass", dr => dr.Glass},
                {"Water", dr => dr.Water},
                {"Fabric", dr => dr.Fabric},
                {"Rubber", dr => dr.Rubber},
                {"Plastic", dr => dr.Plastic},
                {"Ice", dr => dr.Ice},
                {"Snow", dr => dr.Snow}
            };

        }

        public static void ShowBulletImpactEffect(this EntityComponent entityComponent,int bulletId,string physicalMatName,Vector3 pos,Quaternion quaternion)
        {
            IDataTable<DRBulletImpactEffect> dtEffect = GameEntry.DataTable.GetDataTable<DRBulletImpactEffect>();
            DRBulletImpactEffect dr = dtEffect.GetDataRow(bulletId);
            var effectId = GetEffectIdFromMaterial(physicalMatName, dr);
            entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, new EffectData(GameEntry.Entity.GenerateSerialId(),
                effectId)
            {
                Position = pos,
                Rotation = quaternion,
            });
        }

        #endregion

        
        
        public static void ShowModelObj(this EntityComponent entityComponent, int typeId,Vector3 pos,Quaternion rotation,Action<Entity> callBack )
        {
            entityComponent.ShowEntity(typeof(ModelObj), "ModelObj", Constant.AssetPriority.EffectAsset, new ModelObjData(
                GameEntry.Entity.GenerateSerialId(),typeId)
            {
                Position = pos,
                Rotation = rotation,
                OnShowCallBack = callBack,
            });
        }
        
        public static void ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, data);
        }

        public static void ShowBattleUnit(this EntityComponent entityComponent, global::BattleUnitData data)
        {
            entityComponent.ShowEntity(typeof(BattleUnit), "ChaObj", Constant.AssetPriority.BattleUnitAsset, data);
        }
        
        private static void ShowEntity(this EntityComponent entityComponent, Type logicType, string entityGroup, int priority, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            //原版读表代码
            // IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            // DREntity drEntity = dtEntity.GetDataRow(data.TypeId);
            // if (drEntity == null)
            // {
            //     Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
            //     return;
            // }
            //
            // entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drEntity.AssetName), entityGroup, priority, data);


            var obj = GameEntry.SoDataTableComponent.GetSoDataRow<EntityDataRow>(data.TypeId).assetGameObject;
           
            entityComponent.ShowEntity(data.Id, logicType, obj, entityGroup, priority, data);
        }

        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return --s_SerialId;
        }
    }
}
