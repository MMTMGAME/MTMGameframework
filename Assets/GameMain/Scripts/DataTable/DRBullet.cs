//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-01-25 14:57:08.088
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
        /// 获取死亡音效数组。
        /// </summary>
        public string DeadSoundIdArrStr
        {
            get;
            private set;
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
        /// 获取死亡特效Id。
        /// </summary>
        public int DeadEffectId
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
            DeadSoundIdArrStr = columnStrings[index++];
            HideSoundIdArrStr = columnStrings[index++];
            FireSoundId = int.Parse(columnStrings[index++]);
            DeadEffectId = int.Parse(columnStrings[index++]);
            HideEffectId = int.Parse(columnStrings[index++]);
            FireEffectId = int.Parse(columnStrings[index++]);

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
                    DeadSoundIdArrStr = binaryReader.ReadString();
                    HideSoundIdArrStr = binaryReader.ReadString();
                    FireSoundId = binaryReader.Read7BitEncodedInt32();
                    DeadEffectId = binaryReader.Read7BitEncodedInt32();
                    HideEffectId = binaryReader.Read7BitEncodedInt32();
                    FireEffectId = binaryReader.Read7BitEncodedInt32();
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
