//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-15 16:29:54.777
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
    /// 子弹特效表。
    /// </summary>
    public class DRBulletImpactEffect : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取子弹Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取混凝土。
        /// </summary>
        public int Concrete
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取木材。
        /// </summary>
        public int Wood
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取石头。
        /// </summary>
        public int Stone
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取金属 。
        /// </summary>
        public int Metal
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取泥土。
        /// </summary>
        public int Dirt
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取沙子 。
        /// </summary>
        public int Sand
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取草地 。
        /// </summary>
        public int Grass
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取玻璃 。
        /// </summary>
        public int Glass
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取水 。
        /// </summary>
        public int Water
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取布料。
        /// </summary>
        public int Fabric
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取橡胶 。
        /// </summary>
        public int Rubber
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取塑料 。
        /// </summary>
        public int Plastic
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取冰 。
        /// </summary>
        public int Ice
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取雪 。
        /// </summary>
        public int Snow
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取默认（无物理材质）。
        /// </summary>
        public int Default
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
            Concrete = int.Parse(columnStrings[index++]);
            Wood = int.Parse(columnStrings[index++]);
            Stone = int.Parse(columnStrings[index++]);
            Metal = int.Parse(columnStrings[index++]);
            Dirt = int.Parse(columnStrings[index++]);
            Sand = int.Parse(columnStrings[index++]);
            Grass = int.Parse(columnStrings[index++]);
            Glass = int.Parse(columnStrings[index++]);
            Water = int.Parse(columnStrings[index++]);
            Fabric = int.Parse(columnStrings[index++]);
            Rubber = int.Parse(columnStrings[index++]);
            Plastic = int.Parse(columnStrings[index++]);
            Ice = int.Parse(columnStrings[index++]);
            Snow = int.Parse(columnStrings[index++]);
            Default = int.Parse(columnStrings[index++]);

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
                    Concrete = binaryReader.Read7BitEncodedInt32();
                    Wood = binaryReader.Read7BitEncodedInt32();
                    Stone = binaryReader.Read7BitEncodedInt32();
                    Metal = binaryReader.Read7BitEncodedInt32();
                    Dirt = binaryReader.Read7BitEncodedInt32();
                    Sand = binaryReader.Read7BitEncodedInt32();
                    Grass = binaryReader.Read7BitEncodedInt32();
                    Glass = binaryReader.Read7BitEncodedInt32();
                    Water = binaryReader.Read7BitEncodedInt32();
                    Fabric = binaryReader.Read7BitEncodedInt32();
                    Rubber = binaryReader.Read7BitEncodedInt32();
                    Plastic = binaryReader.Read7BitEncodedInt32();
                    Ice = binaryReader.Read7BitEncodedInt32();
                    Snow = binaryReader.Read7BitEncodedInt32();
                    Default = binaryReader.Read7BitEncodedInt32();
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
