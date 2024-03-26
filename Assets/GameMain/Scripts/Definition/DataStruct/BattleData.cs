//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameMain
{
    [StructLayout(LayoutKind.Auto)]
    public struct BattleData
    {
        private readonly CampType m_Camp;
        private readonly int m_HP;
       
        private readonly int m_Defense;

        public BattleData(CampType camp, int hp, int defense)
        {
            m_Camp = camp;
            m_HP = hp;
            m_Defense = defense;
        }


        
        public CampType Camp
        {
            get
            {
                return m_Camp;
            }
        }

        public int HP
        {
            get
            {
                return m_HP;
            }
        }

       

        public int Defense
        {
            get
            {
                return m_Defense;
            }
        }
    }
}
