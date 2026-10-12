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
/// CaseTable
/// </summary>
public class CaseTable : DataRowBase
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
        /// 案件稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 案件名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 故事批次
        /// </summary>
        public string Batch
        {
            get;
            private set;
        }

        /// <summary>
        /// 案件总阶段数
        /// </summary>
        public int TotalStages
        {
            get;
            private set;
        }

        /// <summary>
        /// 案件来源类型
        /// </summary>
        public string Source
        {
            get;
            private set;
        }

        /// <summary>
        /// 首次出现地点稳定键
        /// </summary>
        public string FirstPlaceKey
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
            Batch = columnStrings[index++];
            TotalStages = int.Parse(columnStrings[index++]);
            Source = columnStrings[index++];
            FirstPlaceKey = columnStrings[index++];

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
                    Batch = binaryReader.ReadString();
                    TotalStages = binaryReader.Read7BitEncodedInt32();
                    Source = binaryReader.ReadString();
                    FirstPlaceKey = binaryReader.ReadString();
                }
            }

            return true;
        }
}
