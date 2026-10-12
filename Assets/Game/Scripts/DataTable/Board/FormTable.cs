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
/// FormTable
/// </summary>
public class FormTable : DataRowBase
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
        /// 形态稳定键（P-001-F01 或 M-001）
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 形态显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 形态种类（F=0、M=1）
        /// </summary>
        public int FormKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 宿主零件 PartType 数值
        /// </summary>
        public int HostPartKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 形态功能描述
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 加工所需维修费
        /// </summary>
        public int UnlockFee
        {
            get;
            private set;
        }

        /// <summary>
        /// 所需图样 ID
        /// </summary>
        public string BlueprintId
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
        /// 解锁后是否自动设为当前形态
        /// </summary>
        public bool AutoSetCurrent
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
            FormKind = int.Parse(columnStrings[index++]);
            HostPartKind = int.Parse(columnStrings[index++]);
            Description = columnStrings[index++];
            UnlockFee = int.Parse(columnStrings[index++]);
            BlueprintId = columnStrings[index++];
            SourceHint = columnStrings[index++];
            AutoSetCurrent = bool.Parse(columnStrings[index++]);

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
                    FormKind = binaryReader.Read7BitEncodedInt32();
                    HostPartKind = binaryReader.Read7BitEncodedInt32();
                    Description = binaryReader.ReadString();
                    UnlockFee = binaryReader.Read7BitEncodedInt32();
                    BlueprintId = binaryReader.ReadString();
                    SourceHint = binaryReader.ReadString();
                    AutoSetCurrent = binaryReader.ReadBoolean();
                }
            }

            return true;
        }
}
