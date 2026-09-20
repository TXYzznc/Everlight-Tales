using UnityEngine;

namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 输入驱动载体。业务场景不直接引用本类型：它在业务启动 Procedure 进入时
    /// 由 <see cref="ProjectInputDriver.EnsureRunning"/> 创建，并跨场景保留。
    /// 唯一职责是每帧驱动 <see cref="InputModule"/>，不承载任何业务判定。
    /// </summary>
    [DefaultExecutionOrder(InputPumpExecutionOrder)]
    public sealed class ProjectInputDriver : MonoBehaviour
    {
        /// <summary>
        /// 输入泵的执行顺序。设为极小值，保证在同一帧内先于业务消费方完成轮询，
        /// 使业务在 Update 中取到的是本帧事件。
        /// </summary>
        public const int InputPumpExecutionOrder = -10000;

        private InputModule.InputService mService;
        private float mLastUnscaledTime;

        /// <summary>
        /// 保证输入驱动存在且正在运行。重复调用为幂等，不创建第二个载体。
        /// </summary>
        public static ProjectInputDriver EnsureRunning()
        {
            ProjectInputDriver existing = FindAnyObjectByType<ProjectInputDriver>();
            if (existing != null)
            {
                return existing;
            }

            InputModule.InputService service = InputModule.EnsureInitialized();
            GameObject host = new GameObject("[EverlightTales] InputDriver");
            DontDestroyOnLoad(host);
            ProjectInputDriver driver = host.AddComponent<ProjectInputDriver>();
            driver.mService = service;
            return driver;
        }

        /// <summary>
        /// 关闭输入驱动并释放输入服务。仅在框架退出或测试收尾时调用。
        /// </summary>
        public static void Shutdown()
        {
            ProjectInputDriver existing = FindAnyObjectByType<ProjectInputDriver>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            InputModule.Shutdown();
        }

        private void Awake()
        {
            mService = InputModule.GetService();
            mLastUnscaledTime = Time.unscaledTime;
            if (mService == null)
            {
                Debug.LogError(
                    "[EverlightTales] ProjectInputDriver started before the input module was initialized. " +
                    "Business startup procedure must call InputModule.EnsureInitialized first.");
            }
        }

        private void Update()
        {
            if (mService == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            float deltaTime = now - mLastUnscaledTime;
            mLastUnscaledTime = now;
            if (deltaTime < 0f)
            {
                deltaTime = 0f;
            }

            mService.Pump(deltaTime);
        }
    }
}
