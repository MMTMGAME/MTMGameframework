//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameMain
{
    [Serializable]
    public class ArmorData : AccessoryObjectData
    {
       
        private string path;
        
        //属性
        public ChaProperty[] propMod=new ChaProperty[2];

        public List<string> buffs = new List<string>();
        public ArmorData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            //IDataTable<DRArmor> dtArmor = GameEntry.DataTable.GetDataTable<DRArmor>();
            var drArmor = GameEntry.SoDataTableComponent.GetSoDataRow<ArmorDataRow>(TypeId);
            if (drArmor == null)
            {
                return;
            }
            
            path = drArmor.pathKey;
            
            //属性
            propMod[0].hp = drArmor.hpAdd;
            propMod[0].mp = drArmor.mpAdd;
            propMod[0].attack = drArmor.attackAdd;
            propMod[0].defense = drArmor.defenseAdd;
            propMod[0].moveSpeed = drArmor.moveSpeedAdd;
            propMod[0].actionSpeed = drArmor.actionSpeedAdd;

            propMod[1].hp = drArmor.hpTimes;
            propMod[1].mp = drArmor.mpTimes;
            propMod[1].attack = drArmor.attackTimes;
            propMod[1].defense = drArmor.defenseTimes;
            propMod[0].moveSpeed = drArmor.moveSpeedTimes;
            propMod[0].actionSpeed = drArmor.actionSpeedTimes;

            buffs = drArmor.buffs.ToList();
        }

        

        public string Path
        {
            get
            {
                return path;
            }
        }
    }
}
