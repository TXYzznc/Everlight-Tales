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
/// LevelTable
/// </summary>
public class LevelTable : DataRowBase
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
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 关卡显示名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 盘面半径档位 n
        /// </summary>
        public int Radius
        {
            get;
            private set;
        }

        /// <summary>
        /// 初盘零件总数
        /// </summary>
        public int InitialPartCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始机械臂次数
        /// </summary>
        public int InitialArmMoves
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始公共维修能量
        /// </summary>
        public int InitialPublicRepairEnergy
        {
            get;
            private set;
        }

        /// <summary>
        /// 关卡目标文案
        /// </summary>
        public string Objective
        {
            get;
            private set;
        }

        /// <summary>
        /// 预计耗时格数
        /// </summary>
        public int Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 绑定事件稳定键
        /// </summary>
        public string EventKey
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
            Radius = int.Parse(columnStrings[index++]);
            InitialPartCount = int.Parse(columnStrings[index++]);
            InitialArmMoves = int.Parse(columnStrings[index++]);
            InitialPublicRepairEnergy = int.Parse(columnStrings[index++]);
            Objective = columnStrings[index++];
            Duration = int.Parse(columnStrings[index++]);
            EventKey = columnStrings[index++];

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
                    Radius = binaryReader.Read7BitEncodedInt32();
                    InitialPartCount = binaryReader.Read7BitEncodedInt32();
                    InitialArmMoves = binaryReader.Read7BitEncodedInt32();
                    InitialPublicRepairEnergy = binaryReader.Read7BitEncodedInt32();
                    Objective = binaryReader.ReadString();
                    Duration = binaryReader.Read7BitEncodedInt32();
                    EventKey = binaryReader.ReadString();
                }
            }

            return true;
        }
}
