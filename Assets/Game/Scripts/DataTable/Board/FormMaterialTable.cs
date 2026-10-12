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
/// FormMaterialTable
/// </summary>
public class FormMaterialTable : DataRowBase
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
        /// 所属形态稳定键
        /// </summary>
        public string FormKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 材料稳定键
        /// </summary>
        public string MaterialKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 所需数量
        /// </summary>
        public int Count
        {
            get;
            private set;
        }

        /// <summary>
        /// 类型化材料来源案件，空表示不限定
        /// </summary>
        public string SourceCase
        {
            get;
            private set;
        }

        /// <summary>
        /// 配方显示顺序
        /// </summary>
        public int SortOrder
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
            FormKey = columnStrings[index++];
            MaterialKey = columnStrings[index++];
            Count = int.Parse(columnStrings[index++]);
            SourceCase = columnStrings[index++];
            SortOrder = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    FormKey = binaryReader.ReadString();
                    MaterialKey = binaryReader.ReadString();
                    Count = binaryReader.Read7BitEncodedInt32();
                    SourceCase = binaryReader.ReadString();
                    SortOrder = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
