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
/// TaskTable
/// </summary>
public class TaskTable : DataRowBase
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
        /// 任务稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name
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
        /// 是否主线任务
        /// </summary>
        public bool IsMain
        {
            get;
            private set;
        }

        /// <summary>
        /// 委托人
        /// </summary>
        public string Client
        {
            get;
            private set;
        }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 总步骤数
        /// </summary>
        public int TotalSteps
        {
            get;
            private set;
        }

        /// <summary>
        /// 前置案件键，空表示无
        /// </summary>
        public string RequiredCaseDone
        {
            get;
            private set;
        }

        /// <summary>
        /// 前置成功普通维修次数
        /// </summary>
        public int RequiredSuccessCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 目标地点键
        /// </summary>
        public string PlaceKey
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
            RewardFee = int.Parse(columnStrings[index++]);
            IsMain = bool.Parse(columnStrings[index++]);
            Client = columnStrings[index++];
            Description = columnStrings[index++];
            TotalSteps = int.Parse(columnStrings[index++]);
            RequiredCaseDone = columnStrings[index++];
            RequiredSuccessCount = int.Parse(columnStrings[index++]);
            PlaceKey = columnStrings[index++];

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
                    RewardFee = binaryReader.Read7BitEncodedInt32();
                    IsMain = binaryReader.ReadBoolean();
                    Client = binaryReader.ReadString();
                    Description = binaryReader.ReadString();
                    TotalSteps = binaryReader.Read7BitEncodedInt32();
                    RequiredCaseDone = binaryReader.ReadString();
                    RequiredSuccessCount = binaryReader.Read7BitEncodedInt32();
                    PlaceKey = binaryReader.ReadString();
                }
            }

            return true;
        }
}
