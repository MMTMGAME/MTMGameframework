//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-16 18:50:58.949
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
    /// 子弹音效配置表。
    /// </summary>
    public class DRBullet : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取子弹实体编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取隐藏时声音Id。
        /// </summary>
        public string HideSoundIdArrStr
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取发射时声音Id。
        /// </summary>
        public int FireSoundId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取隐藏时特效Id。
        /// </summary>
        public int HideEffectId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取发射时特效Id。
        /// </summary>
        public int FireEffectId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取子弹逻辑类型。
        /// </summary>
        public string StrategyComponent
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取特殊数据(json)。
        /// </summary>
        public string SpecialData
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取使用重力。
        /// </summary>
        public bool UseGravity
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取碰撞体。
        /// </summary>
        public bool IsTrigger
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
            HideSoundIdArrStr = columnStrings[index++];
            FireSoundId = int.Parse(columnStrings[index++]);
            HideEffectId = int.Parse(columnStrings[index++]);
            FireEffectId = int.Parse(columnStrings[index++]);
            StrategyComponent = columnStrings[index++];
            SpecialData = columnStrings[index++];
            UseGravity = bool.Parse(columnStrings[index++]);
            IsTrigger = bool.Parse(columnStrings[index++]);

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
                    HideSoundIdArrStr = binaryReader.ReadString();
                    FireSoundId = binaryReader.Read7BitEncodedInt32();
                    HideEffectId = binaryReader.Read7BitEncodedInt32();
                    FireEffectId = binaryReader.Read7BitEncodedInt32();
                    StrategyComponent = binaryReader.ReadString();
                    SpecialData = binaryReader.ReadString();
                    UseGravity = binaryReader.ReadBoolean();
                    IsTrigger = binaryReader.ReadBoolean();
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
