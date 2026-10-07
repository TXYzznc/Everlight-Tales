#if UNITY_EDITOR
namespace Everlight.Tales.UI.Editor
{
    public static class GuestPageContractMigration
    {
        // 保留旧调用入口，统一使用支持滚动列表和空状态节点的绑定实现。
        public static void Apply() => GuestPanelContractMigration.Apply();
    }
}
#endif
