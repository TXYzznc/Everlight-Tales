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
/// LevelFixedElementTable
/// </summary>
public class LevelFixedElementTable : DataRowBase
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
        /// FixedElementKind 数值
        /// </summary>
        public int ElementKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 六边形坐标 Q
        /// </summary>
        public int Q
        {
            get;
            private set;
        }

        /// <summary>
        /// 六边形坐标 R
        /// </summary>
        public int R
        {
            get;
            private set;
        }

        /// <summary>
        /// ObstacleType 数值
        /// </summary>
        public int ObstacleType
        {
            get;
            private set;
        }

        /// <summary>
        /// 端点标签
        /// </summary>
        public int Label
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定目标稳定键
        /// </summary>
        public string GoalKey
        {
            get;
            private set;
        }

        /// <summary>
        /// GoalKind 数值
        /// </summary>
        public int GoalKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标实体 ID
        /// </summary>
        public int GoalEntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标锚点标签
        /// </summary>
        public int GoalAnchorLabel
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标所需量
        /// </summary>
        public int GoalRequired
        {
            get;
            private set;
        }

        /// <summary>
        /// 维修零件 PartType 数值
        /// </summary>
        public string RepairPartKinds
        {
            get;
            private set;
        }

        /// <summary>
        /// 维修所需进度
        /// </summary>
        public int RepairRequired
        {
            get;
            private set;
        }

        /// <summary>
        /// 单次维修能量消耗
        /// </summary>
        public int RepairEnergyCost
        {
            get;
            private set;
        }

        /// <summary>
        /// 实例关系 k=v；固定元素行ID
        /// </summary>
        public string InstanceData
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
            ElementKind = int.Parse(columnStrings[index++]);
            Q = int.Parse(columnStrings[index++]);
            R = int.Parse(columnStrings[index++]);
            ObstacleType = int.Parse(columnStrings[index++]);
            Label = int.Parse(columnStrings[index++]);
            GoalKey = columnStrings[index++];
            GoalKind = int.Parse(columnStrings[index++]);
            GoalEntityId = int.Parse(columnStrings[index++]);
            GoalAnchorLabel = int.Parse(columnStrings[index++]);
            GoalRequired = int.Parse(columnStrings[index++]);
            RepairPartKinds = columnStrings[index++];
            RepairRequired = int.Parse(columnStrings[index++]);
            RepairEnergyCost = int.Parse(columnStrings[index++]);
            InstanceData = columnStrings[index++];

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
                    ElementKind = binaryReader.Read7BitEncodedInt32();
                    Q = binaryReader.Read7BitEncodedInt32();
                    R = binaryReader.Read7BitEncodedInt32();
                    ObstacleType = binaryReader.Read7BitEncodedInt32();
                    Label = binaryReader.Read7BitEncodedInt32();
                    GoalKey = binaryReader.ReadString();
                    GoalKind = binaryReader.Read7BitEncodedInt32();
                    GoalEntityId = binaryReader.Read7BitEncodedInt32();
                    GoalAnchorLabel = binaryReader.Read7BitEncodedInt32();
                    GoalRequired = binaryReader.Read7BitEncodedInt32();
                    RepairPartKinds = binaryReader.ReadString();
                    RepairRequired = binaryReader.Read7BitEncodedInt32();
                    RepairEnergyCost = binaryReader.Read7BitEncodedInt32();
                    InstanceData = binaryReader.ReadString();
                }
            }

            return true;
        }
}
