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
/// StageReleaseTable
/// </summary>
public class StageReleaseTable : DataRowBase
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
        /// 投放条目稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// ReleaseKind 数值
        /// </summary>
        public int ReleaseKind
        {
            get;
            private set;
        }

        /// <summary>
        /// StageLevel 数值
        /// </summary>
        public int Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// 首次出现提示
        /// </summary>
        public string FirstAppearance
        {
            get;
            private set;
        }

        /// <summary>
        /// 永久解锁条件
        /// </summary>
        public string UnlockCondition
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
            ReleaseKind = int.Parse(columnStrings[index++]);
            Stage = int.Parse(columnStrings[index++]);
            FirstAppearance = columnStrings[index++];
            UnlockCondition = columnStrings[index++];

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
                    ReleaseKind = binaryReader.Read7BitEncodedInt32();
                    Stage = binaryReader.Read7BitEncodedInt32();
                    FirstAppearance = binaryReader.ReadString();
                    UnlockCondition = binaryReader.ReadString();
                }
            }

            return true;
        }
}
