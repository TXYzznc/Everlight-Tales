namespace Everlight.Tales.Data
{
    /// <summary>版本与渠道口径（P6-010）：首发版本号/渠道/构建号。</summary>
    public sealed class VersionConfig
    {
        public string Version;   // 语义化版本
        public string Channel;   // 首发渠道（taptap / appstore / googleplay …）
        public int BuildNumber;

        public VersionConfig(string version, string channel, int buildNumber)
        {
            Version = version;
            Channel = channel;
            BuildNumber = buildNumber;
        }
    }

    /// <summary>版本口径目录（P6-010）。</summary>
    public static class VersionCatalog
    {
        public static readonly VersionConfig FirstRelease = new VersionConfig(
            version: "0.1.0",
            channel: "taptap",
            buildNumber: 1);
    }
}
