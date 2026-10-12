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
/// MaterialTable
/// </summary>
public class MaterialTable : DataRowBase
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
        /// 稳定键（MT-001~MT-006）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 材料显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 材料稀有度（Common=0、Refined=1、Rare=2）
        /// </summary>
        public int Rarity
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否按来源怪谈类型化
        /// </summary>
        public bool IsTypedByCase
        {
            get;
            private set;
        }

        /// <summary>
        /// 材料风味描述
        /// </summary>
        public string FlavorText
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
            Rarity = int.Parse(columnStrings[index++]);
            IsTypedByCase = bool.Parse(columnStrings[index++]);
            FlavorText = columnStrings[index++];

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
                    Rarity = binaryReader.Read7BitEncodedInt32();
                    IsTypedByCase = binaryReader.ReadBoolean();
                    FlavorText = binaryReader.ReadString();
                }
            }

            return true;
        }
}
