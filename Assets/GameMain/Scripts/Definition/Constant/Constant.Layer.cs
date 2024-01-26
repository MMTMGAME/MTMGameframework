//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace GameMain
{
    public static partial class Constant
    {
        /// <summary>
        /// 层。
        /// </summary>
        public static class Layer
        {
            public const string DefaultLayerName = "Default";
            public static readonly int DefaultLayerId = LayerMask.NameToLayer(DefaultLayerName);

            public const string UILayerName = "UI";
            public static readonly int UILayerId = LayerMask.NameToLayer(UILayerName);

            public const string TargetableObjectLayerName = "Targetable Object";
            public static readonly int TargetableObjectLayerId = LayerMask.NameToLayer(TargetableObjectLayerName);

            public const string SceneItemLayerName = "SceneItem";
            public static readonly int SceneItemLayerId = LayerMask.NameToLayer(SceneItemLayerName);

            public const string BodyBonesLayerName = "BodyBones";
            public static readonly int BodyBonesLayerId = LayerMask.NameToLayer(BodyBonesLayerName);
            
            public const string HoldingSceneItemLayerName = "HoldingSceneItem";
            public static readonly int HoldingSceneItemLayerId = LayerMask.NameToLayer(HoldingSceneItemLayerName);
        }
    }
}
