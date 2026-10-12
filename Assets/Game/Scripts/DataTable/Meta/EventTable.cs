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
/// EventTable
/// </summary>
public class EventTable : DataRowBase
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
        /// 事件稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 事件显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// EventKind 数值
        /// </summary>
        public int EventKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 最早投放阶段
        /// </summary>
        public int MinStage
        {
            get;
            private set;
        }

        /// <summary>
        /// 白天投放权重
        /// </summary>
        public int DayWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 夜晚投放权重
        /// </summary>
        public int NightWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// 完成后消耗的时间格
        /// </summary>
        public int TimeCost
        {
            get;
            private set;
        }

        /// <summary>
        /// 完成奖励维修费
        /// </summary>
        public int RewardFee
        {
            get;
            private set;
        }

        /// <summary>
        /// 投放地点稳定键
        /// </summary>
        public string PlaceKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否可重复
        /// </summary>
        public bool Repeatable
        {
            get;
            private set;
        }

        /// <summary>
        /// 失败收尾文案
        /// </summary>
        public string FailureWrapUp
        {
            get;
            private set;
        }

        /// <summary>
        /// 奖励材料稳定键，空表示无
        /// </summary>
        public string RewardMaterialKey
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
            EventKind = int.Parse(columnStrings[index++]);
            MinStage = int.Parse(columnStrings[index++]);
            DayWeight = int.Parse(columnStrings[index++]);
            NightWeight = int.Parse(columnStrings[index++]);
            TimeCost = int.Parse(columnStrings[index++]);
            RewardFee = int.Parse(columnStrings[index++]);
            PlaceKey = columnStrings[index++];
            Repeatable = bool.Parse(columnStrings[index++]);
            FailureWrapUp = columnStrings[index++];
            RewardMaterialKey = columnStrings[index++];

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
                    EventKind = binaryReader.Read7BitEncodedInt32();
                    MinStage = binaryReader.Read7BitEncodedInt32();
                    DayWeight = binaryReader.Read7BitEncodedInt32();
                    NightWeight = binaryReader.Read7BitEncodedInt32();
                    TimeCost = binaryReader.Read7BitEncodedInt32();
                    RewardFee = binaryReader.Read7BitEncodedInt32();
                    PlaceKey = binaryReader.ReadString();
                    Repeatable = binaryReader.ReadBoolean();
                    FailureWrapUp = binaryReader.ReadString();
                    RewardMaterialKey = binaryReader.ReadString();
                }
            }

            return true;
        }
}
