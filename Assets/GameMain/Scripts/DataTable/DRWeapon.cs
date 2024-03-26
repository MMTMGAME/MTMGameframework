//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-03-26 17:34:40.685
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
    /// 武器表。
    /// </summary>
    public class DRWeapon : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取武器编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取攻击力。
        /// </summary>
        public int Attack
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取攻击间隔。
        /// </summary>
        public float AttackInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取子弹编号。
        /// </summary>
        public int BulletId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取子弹速度或者初始力。
        /// </summary>
        public float BulletSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取子弹声音编号。
        /// </summary>
        public int BulletSoundId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取攻击模板。
        /// </summary>
        public string AttackLogicComponent
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取发射子弹的Transform，仅远程武器使用,近战武器也可以设置，用于特殊情况转为远程武器。
        /// </summary>
        public string ShootPoint
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
            Attack = int.Parse(columnStrings[index++]);
            AttackInterval = float.Parse(columnStrings[index++]);
            BulletId = int.Parse(columnStrings[index++]);
            BulletSpeed = float.Parse(columnStrings[index++]);
            BulletSoundId = int.Parse(columnStrings[index++]);
            AttackLogicComponent = columnStrings[index++];
            ShootPoint = columnStrings[index++];

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
                    Attack = binaryReader.Read7BitEncodedInt32();
                    AttackInterval = binaryReader.ReadSingle();
                    BulletId = binaryReader.Read7BitEncodedInt32();
                    BulletSpeed = binaryReader.ReadSingle();
                    BulletSoundId = binaryReader.Read7BitEncodedInt32();
                    AttackLogicComponent = binaryReader.ReadString();
                    ShootPoint = binaryReader.ReadString();
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
