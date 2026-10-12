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
/// LevelRoundTable
/// </summary>
public class LevelRoundTable : DataRowBase
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
        /// 关卡稳定键
        /// </summary>
        public string LevelKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 轮次序号
        /// </summary>
        public int RoundIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 本轮拍数
        /// </summary>
        public int TapCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 本轮目标分
        /// </summary>
        public int TargetScore
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
            RoundIndex = int.Parse(columnStrings[index++]);
            TapCount = int.Parse(columnStrings[index++]);
            TargetScore = int.Parse(columnStrings[index++]);

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
                    RoundIndex = binaryReader.Read7BitEncodedInt32();
                    TapCount = binaryReader.Read7BitEncodedInt32();
                    TargetScore = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
