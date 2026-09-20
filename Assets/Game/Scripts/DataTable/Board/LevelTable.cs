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
/// LevelTable
/// </summary>
public class LevelTable : DataRowBase
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
        /// 关卡显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 盘面半径档位 n，合法格数 3n(n-1)+1
        /// </summary>
        public int Radius
        {
            get;
            private set;
        }

        /// <summary>
        /// 本关轮数
        /// </summary>
        public int RoundCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 本关目标分数
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
            Name = columnStrings[index++];
            Radius = int.Parse(columnStrings[index++]);
            RoundCount = int.Parse(columnStrings[index++]);
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
                    Name = binaryReader.ReadString();
                    Radius = binaryReader.Read7BitEncodedInt32();
                    RoundCount = binaryReader.Read7BitEncodedInt32();
                    TargetScore = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
