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
/// AssetBindingTable
/// </summary>
public class AssetBindingTable : DataRowBase
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
        /// 资源槽位稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// AssetCategory 数值
        /// </summary>
        public int Category
        {
            get;
            private set;
        }

        /// <summary>
        /// 资源槽位名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 资源路径，未挂接时为空
        /// </summary>
        public string Path
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
            Path = columnStrings[index++];

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
                    Path = binaryReader.ReadString();
                }
            }

            return true;
        }
}
