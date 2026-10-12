//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
//------------------------------------------------------------

using GameFramework;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

/// <summary>CaseRewardOptionTable</summary>
public class CaseRewardOptionTable : DataRowBase
{
    private int m_Id;
    public override int Id { get { return m_Id; } }

    public string Key { get; private set; }
    public string OwnerKey { get; private set; }
    public int RewardKind { get; private set; }
    public string RewardId { get; private set; }
    public int SortOrder { get; private set; }
    public string Label { get; private set; }

    public override bool ParseDataRow(string dataRowString, object userData)
    {
        string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
        for (int i = 0; i < columnStrings.Length; i++) columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
        int index = 0;
        index++;
        m_Id = int.Parse(columnStrings[index++]);
        index++;
        Key = columnStrings[index++];
        OwnerKey = columnStrings[index++];
        RewardKind = int.Parse(columnStrings[index++]);
        RewardId = columnStrings[index++];
        SortOrder = int.Parse(columnStrings[index++]);
        Label = columnStrings[index++];
        return true;
    }

    public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
    {
        using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
        using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
        {
            m_Id = binaryReader.Read7BitEncodedInt32();
            Key = binaryReader.ReadString();
            OwnerKey = binaryReader.ReadString();
            RewardKind = binaryReader.Read7BitEncodedInt32();
            RewardId = binaryReader.ReadString();
            SortOrder = binaryReader.Read7BitEncodedInt32();
            Label = binaryReader.ReadString();
        }
        return true;
    }
}
