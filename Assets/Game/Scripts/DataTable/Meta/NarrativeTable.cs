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
/// NarrativeTable
/// </summary>
public class NarrativeTable : DataRowBase
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
        /// 叙事条目稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 脚本图稳定键，opening/request/confirmed/thanks
        /// </summary>
        public string ScriptKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 脚本内顺序
        /// </summary>
        public int SortOrder
        {
            get;
            private set;
        }

        /// <summary>
        /// 对话节点键，序章可为空
        /// </summary>
        public string NodeKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 说话人
        /// </summary>
        public string Speaker
        {
            get;
            private set;
        }

        /// <summary>
        /// 台词或旁白正文
        /// </summary>
        public string Text
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前节点选项文案，空表示无选项
        /// </summary>
        public string ChoiceLabel
        {
            get;
            private set;
        }

        /// <summary>
        /// 选项目标节点，空表示结束
        /// </summary>
        public string NextNodeKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 序章停留秒数，对话为 0
        /// </summary>
        public float Duration
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
            ScriptKey = columnStrings[index++];
            SortOrder = int.Parse(columnStrings[index++]);
            NodeKey = columnStrings[index++];
            Speaker = columnStrings[index++];
            Text = columnStrings[index++];
            ChoiceLabel = columnStrings[index++];
            NextNodeKey = columnStrings[index++];
            Duration = float.Parse(columnStrings[index++]);

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
                    ScriptKey = binaryReader.ReadString();
                    SortOrder = binaryReader.Read7BitEncodedInt32();
                    NodeKey = binaryReader.ReadString();
                    Speaker = binaryReader.ReadString();
                    Text = binaryReader.ReadString();
                    ChoiceLabel = binaryReader.ReadString();
                    NextNodeKey = binaryReader.ReadString();
                    Duration = binaryReader.ReadSingle();
                }
            }

            return true;
        }
}
