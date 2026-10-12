//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
//------------------------------------------------------------

using GameFramework;
using System;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

/// <summary>BatchTable</summary>
public class BatchTable : DataRowBase
{
    private int m_Id;
    public override int Id { get { return m_Id; } }

    public string Key { get; private set; }
    public int BatchNumber { get; private set; }
    public string Name { get; private set; }
    public int RequiredCount { get; private set; }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
        for (int i = 0; i < columnStrings.Length; i++) columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
        int index = 0;
        index++;
        m_Id = int.Parse(columnStrings[index++]);
        index++;
        Key = columnStrings[index++];
        BatchNumber = int.Parse(columnStrings[index++]);
        Name = columnStrings[index++];
        RequiredCount = int.Parse(columnStrings[index++]);
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
        using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
        {
            m_Id = binaryReader.Read7BitEncodedInt32();
            Key = binaryReader.ReadString();
            BatchNumber = binaryReader.Read7BitEncodedInt32();
            Name = binaryReader.ReadString();
            RequiredCount = binaryReader.Read7BitEncodedInt32();
        }
        return true;
    }
}
