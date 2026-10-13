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
/// ChallengeTable
/// </summary>
public class ChallengeTable : DataRowBase
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
        /// 挑战稳定键（C-001~C-015）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定案件稳定键
        /// </summary>
        public string CaseKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 挑战显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定关卡稳定键，空表示逐关参数后置
        /// </summary>
        public string LevelKey
        {
            get;
            private set;
        }

        /// <summary>
        /// ChallengeKind 数值
        /// </summary>
        public int ChallengeKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 规则库编号（CR-xxx）
        /// </summary>
        public string RuleId
        {
            get;
            private set;
        }

        /// <summary>
        /// 首次投放阶段（S1~S5）
        /// </summary>
        public string Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// 挑战目标文案
        /// </summary>
        public string GoalText
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
            CaseKey = columnStrings[index++];
            Name = columnStrings[index++];
            LevelKey = columnStrings[index++];
            ChallengeKind = int.Parse(columnStrings[index++]);
            RuleId = columnStrings[index++];
            Stage = columnStrings[index++];
            GoalText = columnStrings[index++];
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
                    CaseKey = binaryReader.ReadString();
                    Name = binaryReader.ReadString();
                    LevelKey = binaryReader.ReadString();
                    ChallengeKind = binaryReader.Read7BitEncodedInt32();
                    RuleId = binaryReader.ReadString();
                    Stage = binaryReader.ReadString();
                    GoalText = binaryReader.ReadString();
                    ExtraData = binaryReader.ReadString();
                }
            }

            return true;
        }
}
