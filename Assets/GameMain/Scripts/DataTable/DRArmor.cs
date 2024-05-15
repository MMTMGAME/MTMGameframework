//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-15 16:29:54.735
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
    /// 装甲表。
    /// </summary>
    public class DRArmor : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取装甲编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取血量加算。
        /// </summary>
        public int HPAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取血量乘算（%）。
        /// </summary>
        public int HPTimes
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取MP加算。
        /// </summary>
        public int MPAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取MP乘算。
        /// </summary>
        public int MPTimes
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取攻击加算。
        /// </summary>
        public int AttackAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取攻击乘算。
        /// </summary>
        public int AttackTimes
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取防御加算。
        /// </summary>
        public int DefenseAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取防御乘算。
        /// </summary>
        public int DefenseTimes
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取附加路径。
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取移动速度加算。
        /// </summary>
        public int MoveSpeedAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取移动速度乘算。
        /// </summary>
        public int MoveSpeedTimes
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取行动速度加算。
        /// </summary>
        public int ActionSpeedAdd
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取行动速度乘算。
        /// </summary>
        public int ActionSpeedTimes
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
            HPAdd = int.Parse(columnStrings[index++]);
            HPTimes = int.Parse(columnStrings[index++]);
            MPAdd = int.Parse(columnStrings[index++]);
            MPTimes = int.Parse(columnStrings[index++]);
            AttackAdd = int.Parse(columnStrings[index++]);
            AttackTimes = int.Parse(columnStrings[index++]);
            DefenseAdd = int.Parse(columnStrings[index++]);
            DefenseTimes = int.Parse(columnStrings[index++]);
            Path = columnStrings[index++];
            MoveSpeedAdd = int.Parse(columnStrings[index++]);
            MoveSpeedTimes = int.Parse(columnStrings[index++]);
            ActionSpeedAdd = int.Parse(columnStrings[index++]);
            ActionSpeedTimes = int.Parse(columnStrings[index++]);

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
                    HPAdd = binaryReader.Read7BitEncodedInt32();
                    HPTimes = binaryReader.Read7BitEncodedInt32();
                    MPAdd = binaryReader.Read7BitEncodedInt32();
                    MPTimes = binaryReader.Read7BitEncodedInt32();
                    AttackAdd = binaryReader.Read7BitEncodedInt32();
                    AttackTimes = binaryReader.Read7BitEncodedInt32();
                    DefenseAdd = binaryReader.Read7BitEncodedInt32();
                    DefenseTimes = binaryReader.Read7BitEncodedInt32();
                    Path = binaryReader.ReadString();
                    MoveSpeedAdd = binaryReader.Read7BitEncodedInt32();
                    MoveSpeedTimes = binaryReader.Read7BitEncodedInt32();
                    ActionSpeedAdd = binaryReader.Read7BitEncodedInt32();
                    ActionSpeedTimes = binaryReader.Read7BitEncodedInt32();
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
