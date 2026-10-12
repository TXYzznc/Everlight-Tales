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
/// CalibrationTable
/// </summary>
public class CalibrationTable : DataRowBase
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
        /// 校准项稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 校准分类枚举
        /// </summary>
        public int Category
        {
            get;
            private set;
        }

        /// <summary>
        /// 校准项名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前校准值
        /// </summary>
        public int Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 可接受下界
        /// </summary>
        public int Min
        {
            get;
            private set;
        }

        /// <summary>
        /// 可接受上界
        /// </summary>
        public int Max
        {
            get;
            private set;
        }

        /// <summary>
        /// 校准说明
        /// </summary>
        public string Note
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
            Category = int.Parse(columnStrings[index++]);
            Name = columnStrings[index++];
            Value = int.Parse(columnStrings[index++]);
            Min = int.Parse(columnStrings[index++]);
            Max = int.Parse(columnStrings[index++]);
            Note = columnStrings[index++];

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
                    Category = binaryReader.Read7BitEncodedInt32();
                    Name = binaryReader.ReadString();
                    Value = binaryReader.Read7BitEncodedInt32();
                    Min = binaryReader.Read7BitEncodedInt32();
                    Max = binaryReader.Read7BitEncodedInt32();
                    Note = binaryReader.ReadString();
                }
            }

            return true;
        }
}
