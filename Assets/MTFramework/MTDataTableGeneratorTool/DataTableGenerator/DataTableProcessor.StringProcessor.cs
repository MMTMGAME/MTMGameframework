﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;

namespace GameMain.Editor.DataTableTools
{
    public sealed partial class MTDataTableProcessor
    {
        private sealed class StringProcessor : GenericDataProcessor<string>
        {
            public override bool IsSystem
            {
                get
                {
                    return true;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "string";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "string",
                    "system.string"
                };
            }

            public override string Parse(string value)
            {
                return value;
            }

            public override void WriteToStream(MTDataTableProcessor mtDataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}
