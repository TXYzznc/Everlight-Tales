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
/// AnomalyTable
/// </summary>
public class AnomalyTable : DataRowBase
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
        /// 稳定键（AN-001~AN-008）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// AnomalyType 数值
        /// </summary>
        public int AnomalyKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 局部定势偏转步数
        /// </summary>
        public int Deflection
        {
            get;
            private set;
        }

        /// <summary>
        /// 沉降方向相位序号
        /// </summary>
        public int SettleDirection
        {
            get;
            private set;
        }

        /// <summary>
        /// 扩张或过载阈值
        /// </summary>
        public int Threshold
        {
            get;
            private set;
        }

        /// <summary>
        /// 过载场芯数量
        /// </summary>
        public int CoreCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 拍击后反转周期
        /// </summary>
        public int TapPeriod
        {
            get;
            private set;
        }

        /// <summary>
        /// 逐项参数，k=v;k=v 分号分隔；空表示用代码默认值
        /// </summary>
        public string ExtraData
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
            AnomalyKind = int.Parse(columnStrings[index++]);
            Deflection = int.Parse(columnStrings[index++]);
            SettleDirection = int.Parse(columnStrings[index++]);
            Threshold = int.Parse(columnStrings[index++]);
            CoreCount = int.Parse(columnStrings[index++]);
            TapPeriod = int.Parse(columnStrings[index++]);
            ExtraData = columnStrings[index++];

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
                    AnomalyKind = binaryReader.Read7BitEncodedInt32();
                    Deflection = binaryReader.Read7BitEncodedInt32();
                    SettleDirection = binaryReader.Read7BitEncodedInt32();
                    Threshold = binaryReader.Read7BitEncodedInt32();
                    CoreCount = binaryReader.Read7BitEncodedInt32();
                    TapPeriod = binaryReader.Read7BitEncodedInt32();
                    ExtraData = binaryReader.ReadString();
                }
            }

            return true;
        }
}
