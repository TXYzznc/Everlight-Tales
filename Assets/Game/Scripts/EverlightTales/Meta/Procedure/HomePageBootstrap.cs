using UnityEngine.SceneManagement;

namespace Everlight.Tales.Procedure
{
    /// <summary>
    /// 流程接入 b41：主页壳运行接入入口。
    /// 业务程序集（Everlight.Tales.*）由框架热更/启动链加载，晚于 Unity 的
    /// RuntimeInitializeOnLoadMethod 阶段，因此在业务程序集里写 [RuntimeInitializeOnLoadMethod]
    /// 不会被触发。改用「业务启动 Procedure 登记 + Unity 场景加载完成事件」自动触发，
    /// 等效实现 RuntimeInitializeOnLoadMethod 的意图：免场景挂点、运行时自动打开。
    /// UI 宿主与 Canvas 根节点由 GF 框架在 Launch 运行时动态生成（GFBuiltin.RootCanvas，
    /// DontDestroyOnLoad 持久），本类不新建画布；本类位于 Meta 层（引用 Hotfix），
    /// 仅使用 Hotfix 的 UIViews/GF.UI 与 Unity 场景事件，不引用业务 UI 程序集。
    /// </summary>
    public static class HomePageBootstrap
    {
        private static bool s_Subscribed;
        private static bool s_Opened;

        /// <summary>业务启动 Procedure 在进入入口场景前调用一次，登记场景加载完成回调。</summary>
        public static void EnsureSubscribed()
        {
            if (s_Subscribed)
            {
                return;
            }

            s_Subscribed = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (s_Opened || scene.name != EverlightTalesProcedure.EntrySceneName)
            {
                return;
            }

            // Home 加载完成时，框架预加载（UITable/UIGroupTable）已就绪，UI 系统可用。
            s_Opened = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GF.UI.OpenUIForm(UIViews.MainPageShell);
        }
    }
}
