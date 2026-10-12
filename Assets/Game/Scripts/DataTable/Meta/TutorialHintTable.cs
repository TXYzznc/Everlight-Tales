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
/// TutorialHintTable
/// </summary>
public class TutorialHintTable : DataRowBase
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
        /// 教学提示稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 教学段序号，从 0 开始
        /// </summary>
        public int StageIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// 教学标题
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// 教学目标
        /// </summary>
        public string TeachGoal
        {
            get;
            private set;
        }

        /// <summary>
        /// 引导文案
        /// </summary>
        public string GuideText
        {
            get;
            private set;
        }

        /// <summary>
        /// 高亮目标
        /// </summary>
        public string HighlightTarget
        {
            get;
            private set;
        }

        /// <summary>
        /// 通过后解锁的零件 PartType 数值
        /// </summary>
        public int UnlockPartKind
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
            StageIndex = int.Parse(columnStrings[index++]);
            Title = columnStrings[index++];
            TeachGoal = columnStrings[index++];
            GuideText = columnStrings[index++];
            HighlightTarget = columnStrings[index++];
            UnlockPartKind = int.Parse(columnStrings[index++]);

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
                    StageIndex = binaryReader.Read7BitEncodedInt32();
                    Title = binaryReader.ReadString();
                    TeachGoal = binaryReader.ReadString();
                    GuideText = binaryReader.ReadString();
                    HighlightTarget = binaryReader.ReadString();
                    UnlockPartKind = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
