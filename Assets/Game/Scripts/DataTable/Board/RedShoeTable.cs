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
/// RedShoeTable
/// </summary>
public class RedShoeTable : DataRowBase
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
        /// 绑定关卡稳定键
        /// </summary>
        public string LevelKey
        {
            get;
            private set;
        }

        /// <summary>
        /// 红鞋起点 Q
        /// </summary>
        public int StartQ
        {
            get;
            private set;
        }

        /// <summary>
        /// 红鞋起点 R
        /// </summary>
        public int StartR
        {
            get;
            private set;
        }

        /// <summary>
        /// 红鞋初始方向数值
        /// </summary>
        public int StartDirection
        {
            get;
            private set;
        }

        /// <summary>
        /// 封存匣 Q
        /// </summary>
        public int BoxQ
        {
            get;
            private set;
        }

        /// <summary>
        /// 封存匣 R
        /// </summary>
        public int BoxR
        {
            get;
            private set;
        }

        /// <summary>
        /// 封存匣维修所需进度
        /// </summary>
        public int BoxRequired
        {
            get;
            private set;
        }

        /// <summary>
        /// 封存入口 Q
        /// </summary>
        public int EntryQ
        {
            get;
            private set;
        }

        /// <summary>
        /// 封存入口 R
        /// </summary>
        public int EntryR
        {
            get;
            private set;
        }

        /// <summary>
        /// 分离所需导流格数
        /// </summary>
        public int RequiredDiversion
        {
            get;
            private set;
        }

        /// <summary>
        /// 束缚失败上限
        /// </summary>
        public int RestraintLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// 每拍规则步进次数
        /// </summary>
        public int StepsPerTap
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
            StartQ = int.Parse(columnStrings[index++]);
            StartR = int.Parse(columnStrings[index++]);
            StartDirection = int.Parse(columnStrings[index++]);
            BoxQ = int.Parse(columnStrings[index++]);
            BoxR = int.Parse(columnStrings[index++]);
            BoxRequired = int.Parse(columnStrings[index++]);
            EntryQ = int.Parse(columnStrings[index++]);
            EntryR = int.Parse(columnStrings[index++]);
            RequiredDiversion = int.Parse(columnStrings[index++]);
            RestraintLimit = int.Parse(columnStrings[index++]);
            StepsPerTap = int.Parse(columnStrings[index++]);

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
                    StartQ = binaryReader.Read7BitEncodedInt32();
                    StartR = binaryReader.Read7BitEncodedInt32();
                    StartDirection = binaryReader.Read7BitEncodedInt32();
                    BoxQ = binaryReader.Read7BitEncodedInt32();
                    BoxR = binaryReader.Read7BitEncodedInt32();
                    BoxRequired = binaryReader.Read7BitEncodedInt32();
                    EntryQ = binaryReader.Read7BitEncodedInt32();
                    EntryR = binaryReader.Read7BitEncodedInt32();
                    RequiredDiversion = binaryReader.Read7BitEncodedInt32();
                    RestraintLimit = binaryReader.Read7BitEncodedInt32();
                    StepsPerTap = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
