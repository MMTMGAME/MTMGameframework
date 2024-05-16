//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-16 18:50:58.941
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
        /// 获取开火音效。
        /// </summary>
        public int ShootSoundId
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

        /// <summary>
        /// 获取skill列表。
        /// </summary>
        public string Skills
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取buff列表。
        /// </summary>
        public string Buffs
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
            ShootSoundId = int.Parse(columnStrings[index++]);
            AttackLogicComponent = columnStrings[index++];
            ShootPoint = columnStrings[index++];
            HPAdd = int.Parse(columnStrings[index++]);
            HPTimes = int.Parse(columnStrings[index++]);
            MPAdd = int.Parse(columnStrings[index++]);
            MPTimes = int.Parse(columnStrings[index++]);
            AttackAdd = int.Parse(columnStrings[index++]);
            AttackTimes = int.Parse(columnStrings[index++]);
            DefenseAdd = int.Parse(columnStrings[index++]);
            DefenseTimes = int.Parse(columnStrings[index++]);
            MoveSpeedAdd = int.Parse(columnStrings[index++]);
            MoveSpeedTimes = int.Parse(columnStrings[index++]);
            ActionSpeedAdd = int.Parse(columnStrings[index++]);
            ActionSpeedTimes = int.Parse(columnStrings[index++]);
            Skills = columnStrings[index++];
            Buffs = columnStrings[index++];

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
                    ShootSoundId = binaryReader.Read7BitEncodedInt32();
                    AttackLogicComponent = binaryReader.ReadString();
                    ShootPoint = binaryReader.ReadString();
                    HPAdd = binaryReader.Read7BitEncodedInt32();
                    HPTimes = binaryReader.Read7BitEncodedInt32();
                    MPAdd = binaryReader.Read7BitEncodedInt32();
                    MPTimes = binaryReader.Read7BitEncodedInt32();
                    AttackAdd = binaryReader.Read7BitEncodedInt32();
                    AttackTimes = binaryReader.Read7BitEncodedInt32();
                    DefenseAdd = binaryReader.Read7BitEncodedInt32();
                    DefenseTimes = binaryReader.Read7BitEncodedInt32();
                    MoveSpeedAdd = binaryReader.Read7BitEncodedInt32();
                    MoveSpeedTimes = binaryReader.Read7BitEncodedInt32();
                    ActionSpeedAdd = binaryReader.Read7BitEncodedInt32();
                    ActionSpeedTimes = binaryReader.Read7BitEncodedInt32();
                    Skills = binaryReader.ReadString();
                    Buffs = binaryReader.ReadString();
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
