//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace GameMain
{
    /// <summary>
    /// 关系类型。
    /// </summary>
    [Flags]
    public enum RelationType : byte
    {
        /// <summary>
        /// 没有任何关系。
        /// </summary>
        None = 0,

        /// <summary>
        /// 友好的。
        /// </summary>
        Friendly = 1 << 0, // 1

        /// <summary>
        /// 中立的。
        /// </summary>
        Neutral = 1 << 1, // 2

        /// <summary>
        /// 敌对的。
        /// </summary>
        Hostile = 1 << 2, // 4
    }

}
