//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2024-05-16 18:50:58.945
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
    /// 战斗单位表，名字想不好，玩家和AI都归类在此种。
    /// </summary>
    public class DRBattleUnit : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取单位Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取主武器。
        /// </summary>
        public int WeaponId0
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取副武器。
        /// </summary>
        public int WeaponId1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取特殊武器（双手）。
        /// </summary>
        public int WeaponId2
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取装甲编号0。
        /// </summary>
        public int ArmorId0
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取装甲编号1。
        /// </summary>
        public int ArmorId1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取装甲编号2。
        /// </summary>
        public int ArmorId2
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

        /// <summary>
        /// 获取右手武器位置。
        /// </summary>
        public string WeaponPath0
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取左手武器位置。
        /// </summary>
        public string WeaponPath1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取特殊武器位置。
        /// </summary>
        public string WeaponPath2
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取死亡分数。
        /// </summary>
        public int DieScore
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始HP。
        /// </summary>
        public int BaseHP
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始攻击力。
        /// </summary>
        public int BaseAttack
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始防御。
        /// </summary>
        public int BaseDefense
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始MP。
        /// </summary>
        public int BaseMP
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始移动速度。
        /// </summary>
        public int BaseMoveSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取初始行动速度。
        /// </summary>
        public int BaseActionSpeed
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
            WeaponId0 = int.Parse(columnStrings[index++]);
            WeaponId1 = int.Parse(columnStrings[index++]);
            WeaponId2 = int.Parse(columnStrings[index++]);
            ArmorId0 = int.Parse(columnStrings[index++]);
            ArmorId1 = int.Parse(columnStrings[index++]);
            ArmorId2 = int.Parse(columnStrings[index++]);
            DeadEffectId = int.Parse(columnStrings[index++]);
            DeadSoundId = int.Parse(columnStrings[index++]);
            WeaponPath0 = columnStrings[index++];
            WeaponPath1 = columnStrings[index++];
            WeaponPath2 = columnStrings[index++];
            DieScore = int.Parse(columnStrings[index++]);
            BaseHP = int.Parse(columnStrings[index++]);
            BaseAttack = int.Parse(columnStrings[index++]);
            BaseDefense = int.Parse(columnStrings[index++]);
            BaseMP = int.Parse(columnStrings[index++]);
            BaseMoveSpeed = int.Parse(columnStrings[index++]);
            BaseActionSpeed = int.Parse(columnStrings[index++]);

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
                    WeaponId0 = binaryReader.Read7BitEncodedInt32();
                    WeaponId1 = binaryReader.Read7BitEncodedInt32();
                    WeaponId2 = binaryReader.Read7BitEncodedInt32();
                    ArmorId0 = binaryReader.Read7BitEncodedInt32();
                    ArmorId1 = binaryReader.Read7BitEncodedInt32();
                    ArmorId2 = binaryReader.Read7BitEncodedInt32();
                    DeadEffectId = binaryReader.Read7BitEncodedInt32();
                    DeadSoundId = binaryReader.Read7BitEncodedInt32();
                    WeaponPath0 = binaryReader.ReadString();
                    WeaponPath1 = binaryReader.ReadString();
                    WeaponPath2 = binaryReader.ReadString();
                    DieScore = binaryReader.Read7BitEncodedInt32();
                    BaseHP = binaryReader.Read7BitEncodedInt32();
                    BaseAttack = binaryReader.Read7BitEncodedInt32();
                    BaseDefense = binaryReader.Read7BitEncodedInt32();
                    BaseMP = binaryReader.Read7BitEncodedInt32();
                    BaseMoveSpeed = binaryReader.Read7BitEncodedInt32();
                    BaseActionSpeed = binaryReader.Read7BitEncodedInt32();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, int>[] m_WeaponId = null;

        public int WeaponIdCount
        {
            get
            {
                return m_WeaponId.Length;
            }
        }

        public int GetWeaponId(int id)
        {
            foreach (KeyValuePair<int, int> i in m_WeaponId)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetWeaponId with invalid id '{0}'.", id));
        }

        public int GetWeaponIdAt(int index)
        {
            if (index < 0 || index >= m_WeaponId.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetWeaponIdAt with invalid index '{0}'.", index));
            }

            return m_WeaponId[index].Value;
        }

        private KeyValuePair<int, int>[] m_ArmorId = null;

        public int ArmorIdCount
        {
            get
            {
                return m_ArmorId.Length;
            }
        }

        public int GetArmorId(int id)
        {
            foreach (KeyValuePair<int, int> i in m_ArmorId)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetArmorId with invalid id '{0}'.", id));
        }

        public int GetArmorIdAt(int index)
        {
            if (index < 0 || index >= m_ArmorId.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetArmorIdAt with invalid index '{0}'.", index));
            }

            return m_ArmorId[index].Value;
        }

        private KeyValuePair<int, string>[] m_WeaponPath = null;

        public int WeaponPathCount
        {
            get
            {
                return m_WeaponPath.Length;
            }
        }

        public string GetWeaponPath(int id)
        {
            foreach (KeyValuePair<int, string> i in m_WeaponPath)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetWeaponPath with invalid id '{0}'.", id));
        }

        public string GetWeaponPathAt(int index)
        {
            if (index < 0 || index >= m_WeaponPath.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetWeaponPathAt with invalid index '{0}'.", index));
            }

            return m_WeaponPath[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_WeaponId = new KeyValuePair<int, int>[]
            {
                new KeyValuePair<int, int>(0, WeaponId0),
                new KeyValuePair<int, int>(1, WeaponId1),
                new KeyValuePair<int, int>(2, WeaponId2),
            };

            m_ArmorId = new KeyValuePair<int, int>[]
            {
                new KeyValuePair<int, int>(0, ArmorId0),
                new KeyValuePair<int, int>(1, ArmorId1),
                new KeyValuePair<int, int>(2, ArmorId2),
            };

            m_WeaponPath = new KeyValuePair<int, string>[]
            {
                new KeyValuePair<int, string>(0, WeaponPath0),
                new KeyValuePair<int, string>(1, WeaponPath1),
                new KeyValuePair<int, string>(2, WeaponPath2),
            };
        }
    }
}
