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
/// TrialTable
/// </summary>
public class TrialTable : DataRowBase
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
        /// 试机稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 试机名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 借用形态稳定键
        /// </summary>
        public string BorrowedFormKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 宿主 PartType 数值
        /// </summary>
        public int HostPartKind
        {
            get;
            private set;
        }

        /// <summary>
        /// 完成后获得的图样键
        /// </summary>
        public string BlueprintKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 每轮拍数
        /// </summary>
        public int TapCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 每轮目标分
        /// </summary>
        public int TargetScore
        {
            get;
            private set;
        }

        /// <summary>
        /// 特殊目标描述
        /// </summary>
        public string SpecialGoal
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
            BorrowedFormKey = columnStrings[index++];
            HostPartKind = int.Parse(columnStrings[index++]);
            BlueprintKey = columnStrings[index++];
            TapCount = int.Parse(columnStrings[index++]);
            TargetScore = int.Parse(columnStrings[index++]);
            SpecialGoal = columnStrings[index++];

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
                    BorrowedFormKey = binaryReader.ReadString();
                    HostPartKind = binaryReader.Read7BitEncodedInt32();
                    BlueprintKey = binaryReader.ReadString();
                    TapCount = binaryReader.Read7BitEncodedInt32();
                    TargetScore = binaryReader.Read7BitEncodedInt32();
                    SpecialGoal = binaryReader.ReadString();
                }
            }

            return true;
        }
}
