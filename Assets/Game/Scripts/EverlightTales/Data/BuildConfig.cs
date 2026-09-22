namespace Everlight.Tales.Data
{
    /// <summary>构建口径（P6-009）：应用名/包名/平台/目标帧率。</summary>
    public sealed class BuildConfig
    {
        public string AppName;
        public string BundleId;
        public string Platform;
        public int TargetFrameRate;

        public BuildConfig(string appName, string bundleId, string platform, int targetFrameRate)
        {
            AppName = appName;
            BundleId = bundleId;
            Platform = platform;
            TargetFrameRate = targetFrameRate;
        }
    }

    /// <summary>构建口径目录（P6-009）。</summary>
    public static class BuildCatalog
    {
        public static readonly BuildConfig Mobile = new BuildConfig(
            appName: "Everlight-Tales",
            bundleId: "com.everlight.tales",
            platform: "Android",
            targetFrameRate: 60);
    }
}
