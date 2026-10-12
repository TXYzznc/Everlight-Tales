//------------------------------------------------------------
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：__DATA_TABLE_CREATE_TIME__
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;
#if ENABLE_OBFUZ
[Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName | Obfuz.ObfuzScope.MethodName)]
#endif
/// <summary>
/// BuffTable
/// </summary>
public class BuffTable : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    /// 
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// 稳定键（BF-001~BF-020）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// Buff 显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 最早进入内容池的玩家阶段
        /// </summary>
        public string Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// 玩家可见效果描述
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否允许叠加
        /// </summary>
        public bool Stackable
        {
            get;
            private set;
        }

        /// <summary>
        /// 最大叠加层数
        /// </summary>
        public int MaxStacks
        {
            get;
            private set;
        }

        /// <summary>
        /// BuffEffectKind 数值
        /// </summary>
        public int Effect
        {
            get;
            private set;
        }

        /// <summary>
        /// 每层效果数值
        /// </summary>
        public int Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 作用零件 PartType 数值，0 表示通用
        /// </summary>
        public int TargetPartKind
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
            Key = columnStrings[index++];
            Name = columnStrings[index++];
            Stage = columnStrings[index++];
            Description = columnStrings[index++];
            Stackable = bool.Parse(columnStrings[index++]);
            MaxStacks = int.Parse(columnStrings[index++]);
            Effect = int.Parse(columnStrings[index++]);
            Value = int.Parse(columnStrings[index++]);
            TargetPartKind = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Key = binaryReader.ReadString();
                    Name = binaryReader.ReadString();
                    Stage = binaryReader.ReadString();
                    Description = binaryReader.ReadString();
                    Stackable = binaryReader.ReadBoolean();
                    MaxStacks = binaryReader.Read7BitEncodedInt32();
                    Effect = binaryReader.Read7BitEncodedInt32();
                    Value = binaryReader.Read7BitEncodedInt32();
                    TargetPartKind = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
