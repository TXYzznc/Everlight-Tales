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
/// RedShoeTurnTable
/// </summary>
public class RedShoeTurnTable : DataRowBase
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
        /// 绑定关卡稳定键
        /// </summary>
        public string LevelKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 转向格 Q
        /// </summary>
        public int Q
        {
            get;
            private set;
        }

        /// <summary>
        /// 转向格 R
        /// </summary>
        public int R
        {
            get;
            private set;
        }

        /// <summary>
        /// 转向后的方向数值
        /// </summary>
        public int Direction
        {
            get;
            private set;
        }

        /// <summary>
        /// 转向格顺序
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
            LevelKey = columnStrings[index++];
            Q = int.Parse(columnStrings[index++]);
            R = int.Parse(columnStrings[index++]);
            Direction = int.Parse(columnStrings[index++]);
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
                    LevelKey = binaryReader.ReadString();
                    Q = binaryReader.Read7BitEncodedInt32();
                    R = binaryReader.Read7BitEncodedInt32();
                    Direction = binaryReader.Read7BitEncodedInt32();
                    SortOrder = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
