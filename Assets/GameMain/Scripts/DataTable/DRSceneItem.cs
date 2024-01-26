//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-01-25 14:57:08.087
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    /// SceneItem配置表。
    /// </summary>
    public class DRSceneItem : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取道具编号（基于实体表）。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取左手路径。
        /// </summary>
        public string LeftHandPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取右手路径。
        /// </summary>
        public string RightHandPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取最大血量。
        /// </summary>
        public int MaxHp
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取攻击。
        /// </summary>
        public int Attack
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取防御（都为0）。
        /// </summary>
        public int Defense
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取EntityLogic类名。
        /// </summary>
        public string EntityLogic
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取死亡特效编号。
        /// </summary>
        public int DeadEffectId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取死亡声音编号。
        /// </summary>
        public int DeadSoundId
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            LeftHandPath = columnStrings[index++];
            RightHandPath = columnStrings[index++];
            MaxHp = int.Parse(columnStrings[index++]);
            Attack = int.Parse(columnStrings[index++]);
            Defense = int.Parse(columnStrings[index++]);
            EntityLogic = columnStrings[index++];
            DeadEffectId = int.Parse(columnStrings[index++]);
            DeadSoundId = int.Parse(columnStrings[index++]);

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    LeftHandPath = binaryReader.ReadString();
                    RightHandPath = binaryReader.ReadString();
                    MaxHp = binaryReader.Read7BitEncodedInt32();
                    Attack = binaryReader.Read7BitEncodedInt32();
                    Defense = binaryReader.Read7BitEncodedInt32();
                    EntityLogic = binaryReader.ReadString();
                    DeadEffectId = binaryReader.Read7BitEncodedInt32();
                    DeadSoundId = binaryReader.Read7BitEncodedInt32();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
