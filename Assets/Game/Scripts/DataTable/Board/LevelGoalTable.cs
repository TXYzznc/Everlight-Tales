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
/// LevelGoalTable
/// </summary>
public class LevelGoalTable : DataRowBase
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
        /// 目标稳定键
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
        /// 被追踪实体 ID
        /// </summary>
        public int EntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标锚点标签
        /// </summary>
        public int AnchorLabel
        {
            get;
            private set;
        }

        /// <summary>
        /// 所需进度量
        /// </summary>
        public int Required
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标显示文案
        /// </summary>
        public string Text
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
            GoalKey = columnStrings[index++];
            GoalKind = int.Parse(columnStrings[index++]);
            EntityId = int.Parse(columnStrings[index++]);
            AnchorLabel = int.Parse(columnStrings[index++]);
            Required = int.Parse(columnStrings[index++]);
            Text = columnStrings[index++];

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
                    GoalKey = binaryReader.ReadString();
                    GoalKind = binaryReader.Read7BitEncodedInt32();
                    EntityId = binaryReader.Read7BitEncodedInt32();
                    AnchorLabel = binaryReader.Read7BitEncodedInt32();
                    Required = binaryReader.Read7BitEncodedInt32();
                    Text = binaryReader.ReadString();
                }
            }

            return true;
        }
}
