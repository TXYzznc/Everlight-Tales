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
/// PlaceTable
/// </summary>
public class PlaceTable : DataRowBase
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
        /// 地点稳定键
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// 地点名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 地点描述
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// PlaceUnlockSource 数值
        /// </summary>
        public int UnlockSource
        {
            get;
            private set;
        }

        /// <summary>
        /// 地图 X 坐标
        /// </summary>
        public float X
        {
            get;
            private set;
        }

        /// <summary>
        /// 地图 Y 坐标
        /// </summary>
        public float Y
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否为家园地点
        /// </summary>
        public bool IsHome
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
            Description = columnStrings[index++];
            UnlockSource = int.Parse(columnStrings[index++]);
            X = float.Parse(columnStrings[index++]);
            Y = float.Parse(columnStrings[index++]);
            IsHome = bool.Parse(columnStrings[index++]);

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
                    Description = binaryReader.ReadString();
                    UnlockSource = binaryReader.Read7BitEncodedInt32();
                    X = binaryReader.ReadSingle();
                    Y = binaryReader.ReadSingle();
                    IsHome = binaryReader.ReadBoolean();
                }
            }

            return true;
        }
}
