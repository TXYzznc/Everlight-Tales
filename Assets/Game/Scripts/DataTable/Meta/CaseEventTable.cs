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
/// CaseEventTable
/// </summary>
public class CaseEventTable : DataRowBase
{
	private int m_Id = 0;
    public override int Id { get { return m_Id; } }

        /// <summary>
        /// 案件事件稳定键
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 案件稳定键
        /// </summary>
        public string CaseKey { get; private set; }

        /// <summary>
        /// 调查入口标题
        /// </summary>
        public string InvestigationTitle { get; private set; }

        /// <summary>
        /// 调查入口说明
        /// </summary>
        public string InvestigationDescription { get; private set; }

        /// <summary>
        /// 维修入口标题
        /// </summary>
        public string RepairTitle { get; private set; }

        /// <summary>
        /// 维修入口说明
        /// </summary>
        public string RepairDescription { get; private set; }

        /// <summary>
        /// 维修入口显示耗时格数
        /// </summary>
        public int RepairTimeCost { get; private set; }

        /// <summary>
        /// 调查页面标题
        /// </summary>
        public string InvestigationPageTitle { get; private set; }

        /// <summary>
        /// 调查热点一名称
        /// </summary>
        public string Hotspot1Name { get; private set; }

        /// <summary>
        /// 调查热点一归一化横坐标
        /// </summary>
        public float Hotspot1X { get; private set; }

        /// <summary>
        /// 调查热点一归一化纵坐标
        /// </summary>
        public float Hotspot1Y { get; private set; }

        /// <summary>
        /// 调查热点二名称
        /// </summary>
        public string Hotspot2Name { get; private set; }

        /// <summary>
        /// 调查热点二归一化横坐标
        /// </summary>
        public float Hotspot2X { get; private set; }

        /// <summary>
        /// 调查热点二归一化纵坐标
        /// </summary>
        public float Hotspot2Y { get; private set; }

        /// <summary>
        /// 调查热点三名称
        /// </summary>
        public string Hotspot3Name { get; private set; }

        /// <summary>
        /// 调查热点三归一化横坐标
        /// </summary>
        public float Hotspot3X { get; private set; }

        /// <summary>
        /// 调查热点三归一化纵坐标
        /// </summary>
        public float Hotspot3Y { get; private set; }

        public int RevisitFee { get; private set; }

        public string RevisitBlueprintKey { get; private set; }

        public int CompletionPartKind { get; private set; }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++) columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            int index = 0; index++; m_Id = int.Parse(columnStrings[index++]); index++;
            Key = columnStrings[index++];
            CaseKey = columnStrings[index++];
            InvestigationTitle = columnStrings[index++];
            InvestigationDescription = columnStrings[index++];
            RepairTitle = columnStrings[index++];
            RepairDescription = columnStrings[index++];
            RepairTimeCost = int.Parse(columnStrings[index++]);
            InvestigationPageTitle = columnStrings[index++];
            Hotspot1Name = columnStrings[index++];
            Hotspot1X = float.Parse(columnStrings[index++]);
            Hotspot1Y = float.Parse(columnStrings[index++]);
            Hotspot2Name = columnStrings[index++];
            Hotspot2X = float.Parse(columnStrings[index++]);
            Hotspot2Y = float.Parse(columnStrings[index++]);
            Hotspot3Name = columnStrings[index++];
            Hotspot3X = float.Parse(columnStrings[index++]);
            Hotspot3Y = float.Parse(columnStrings[index++]);
            RevisitFee = int.Parse(columnStrings[index++]);
            RevisitBlueprintKey = columnStrings[index++];
            CompletionPartKind = int.Parse(columnStrings[index++]);
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
            {
                m_Id = binaryReader.Read7BitEncodedInt32();
                Key = binaryReader.ReadString();
                CaseKey = binaryReader.ReadString();
                InvestigationTitle = binaryReader.ReadString();
                InvestigationDescription = binaryReader.ReadString();
                RepairTitle = binaryReader.ReadString();
                RepairDescription = binaryReader.ReadString();
                RepairTimeCost = binaryReader.Read7BitEncodedInt32();
                InvestigationPageTitle = binaryReader.ReadString();
                Hotspot1Name = binaryReader.ReadString();
                Hotspot1X = binaryReader.ReadSingle();
                Hotspot1Y = binaryReader.ReadSingle();
                Hotspot2Name = binaryReader.ReadString();
                Hotspot2X = binaryReader.ReadSingle();
                Hotspot2Y = binaryReader.ReadSingle();
                Hotspot3Name = binaryReader.ReadString();
                Hotspot3X = binaryReader.ReadSingle();
                Hotspot3Y = binaryReader.ReadSingle();
                RevisitFee = binaryReader.Read7BitEncodedInt32();
                RevisitBlueprintKey = binaryReader.ReadString();
                CompletionPartKind = binaryReader.Read7BitEncodedInt32();
            }
            return true;
        }
}
