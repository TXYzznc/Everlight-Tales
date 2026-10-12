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
/// PartTable
/// </summary>
public class PartTable : DataRowBase
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
        /// 稳定键（P-001~P-020）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 零件显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// PartType 数值
        /// </summary>
        public int PartKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 每次成立触发的触发分
        /// </summary>
        public int TriggerScore
        {
            get;
            private set;
        }

        /// <summary>
        /// 零件能量容量
        /// </summary>
        public int EnergyCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// 单次自身效果消耗的能量
        /// </summary>
        public int EffectCost
        {
            get;
            private set;
        }

        /// <summary>
        /// 每实际推动一格的效果分
        /// </summary>
        public int EffectScorePerCell
        {
            get;
            private set;
        }

        /// <summary>
        /// 每个受冲击对象的效果分
        /// </summary>
        public int EffectScorePerTarget
        {
            get;
            private set;
        }

        /// <summary>
        /// 每次效果产出的公共维修能量
        /// </summary>
        public int PublicEnergyPerEffect
        {
            get;
            private set;
        }

        /// <summary>
        /// 达到次数门槛后的额外效果分
        /// </summary>
        public int BonusScoreFromNth
        {
            get;
            private set;
        }

        /// <summary>
        /// 额外效果分的次数门槛
        /// </summary>
        public int BonusNth
        {
            get;
            private set;
        }

        /// <summary>
        /// 起爆后是否移除本体
        /// </summary>
        public bool RemovesSelf
        {
            get;
            private set;
        }

        /// <summary>
        /// 移除前剩余能量每点的额外效果分
        /// </summary>
        public int EffectScorePerEnergy
        {
            get;
            private set;
        }

        /// <summary>
        /// 单次能力直接推进的格数上限
        /// </summary>
        public int PushDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// 射线搜索或延伸距离
        /// </summary>
        public int RayRange
        {
            get;
            private set;
        }

        /// <summary>
        /// 蓄量释放阈值
        /// </summary>
        public int ChargeThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// 次生效果分
        /// </summary>
        public int SecondaryEffectScore
        {
            get;
            private set;
        }

        /// <summary>
        /// 解锁阶段
        /// </summary>
        public string Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// 来源提示
        /// </summary>
        public string SourceHint
        {
            get;
            private set;
        }

        /// <summary>
        /// 一句话功能提示
        /// </summary>
        public string UseHint
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
            PartKind = int.Parse(columnStrings[index++]);
            TriggerScore = int.Parse(columnStrings[index++]);
            EnergyCapacity = int.Parse(columnStrings[index++]);
            EffectCost = int.Parse(columnStrings[index++]);
            EffectScorePerCell = int.Parse(columnStrings[index++]);
            EffectScorePerTarget = int.Parse(columnStrings[index++]);
            PublicEnergyPerEffect = int.Parse(columnStrings[index++]);
            BonusScoreFromNth = int.Parse(columnStrings[index++]);
            BonusNth = int.Parse(columnStrings[index++]);
            RemovesSelf = bool.Parse(columnStrings[index++]);
            EffectScorePerEnergy = int.Parse(columnStrings[index++]);
            PushDistance = int.Parse(columnStrings[index++]);
            RayRange = int.Parse(columnStrings[index++]);
            ChargeThreshold = int.Parse(columnStrings[index++]);
            SecondaryEffectScore = int.Parse(columnStrings[index++]);
            Stage = columnStrings[index++];
            SourceHint = columnStrings[index++];
            UseHint = columnStrings[index++];

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
                    PartKind = binaryReader.Read7BitEncodedInt32();
                    TriggerScore = binaryReader.Read7BitEncodedInt32();
                    EnergyCapacity = binaryReader.Read7BitEncodedInt32();
                    EffectCost = binaryReader.Read7BitEncodedInt32();
                    EffectScorePerCell = binaryReader.Read7BitEncodedInt32();
                    EffectScorePerTarget = binaryReader.Read7BitEncodedInt32();
                    PublicEnergyPerEffect = binaryReader.Read7BitEncodedInt32();
                    BonusScoreFromNth = binaryReader.Read7BitEncodedInt32();
                    BonusNth = binaryReader.Read7BitEncodedInt32();
                    RemovesSelf = binaryReader.ReadBoolean();
                    EffectScorePerEnergy = binaryReader.Read7BitEncodedInt32();
                    PushDistance = binaryReader.Read7BitEncodedInt32();
                    RayRange = binaryReader.Read7BitEncodedInt32();
                    ChargeThreshold = binaryReader.Read7BitEncodedInt32();
                    SecondaryEffectScore = binaryReader.Read7BitEncodedInt32();
                    Stage = binaryReader.ReadString();
                    SourceHint = binaryReader.ReadString();
                    UseHint = binaryReader.ReadString();
                }
            }

            return true;
        }
}
