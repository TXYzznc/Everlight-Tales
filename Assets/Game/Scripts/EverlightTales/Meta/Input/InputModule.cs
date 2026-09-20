using System.Collections.Generic;

namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 输入单据入口。业务代码只允许经本类型获取输入事件。
    /// 本类型不读取任何底层输入，只把当前 <see cref="IInputProvider"/> 产生的事件
    /// 转发给业务，并在替换来源时清空缓冲，避免混入前一来源的事件。
    /// </summary>
    public static class InputModule
    {
        private static InputService sService;

        /// <summary>输入服务是否已初始化。</summary>
        public static bool IsInitialized
        {
            get { return sService != null && sService.IsInitialized; }
        }

        /// <summary>当前来源标识；未初始化时返回空字符串。</summary>
        public static string CurrentProviderName
        {
            get { return sService == null ? string.Empty : sService.ProviderName; }
        }

        /// <summary>
        /// 建立输入服务并绑定默认来源。重复调用为幂等：已初始化时直接返回原服务，
        /// MUST NOT 因为再次调用而替换来源或丢弃缓冲。
        /// </summary>
        public static InputService EnsureInitialized()
        {
            if (sService != null && sService.IsInitialized)
            {
                return sService;
            }

            sService = new InputService();
            sService.Bind(InputProviderFactory.CreateDefault());
            return sService;
        }

        /// <summary>取当前服务；未初始化时返回 null。</summary>
        public static InputService GetService()
        {
            return sService != null && sService.IsInitialized ? sService : null;
        }

        /// <summary>
        /// 替换输入来源。替换会释放旧来源并清空已缓冲事件，
        /// 替换后取出的事件 MUST 全部来自新来源。
        /// </summary>
        public static void ReplaceProvider(IInputProvider provider)
        {
            InputService service = EnsureInitialized();
            service.ReplaceProvider(provider);
        }

        /// <summary>取出一个待消费输入事件。</summary>
        public static bool TryDequeue(out InputEvent inputEvent)
        {
            if (sService == null)
            {
                inputEvent = default;
                return false;
            }

            return sService.TryDequeue(out inputEvent);
        }

        /// <summary>清空缓冲事件。</summary>
        public static void Clear()
        {
            sService?.Clear();
        }

        /// <summary>
        /// 关闭输入服务。仅在框架退出或测试收尾时调用，
        /// 业务场景切换 MUST NOT 关闭输入服务。
        /// </summary>
        public static void Shutdown()
        {
            if (sService == null)
            {
                return;
            }

            sService.Dispose();
            sService = null;
        }

        /// <summary>取当前来源的类型，供启动日志与诊断使用。</summary>
        public static string DescribeProvider()
        {
            return sService == null ? "none" : sService.DescribeProvider();
        }

        /// <summary>
        /// 输入服务。持有当前来源、每帧轮询并把事件放入缓冲队列。
        /// 队列为单次消费：事件被取出后不再次返回。
        /// </summary>
        public sealed class InputService
        {
            // 单帧最多转发的事件数，防止异常来源在一帧内无限产出导致卡死。
            private const int MaxEventsPerFrame = 256;

            private readonly Queue<InputEvent> mBuffer = new Queue<InputEvent>(32);
            private IInputProvider mProvider;

            /// <summary>服务是否就绪。</summary>
            public bool IsInitialized { get; private set; }

            /// <summary>当前来源标识。</summary>
            public string ProviderName
            {
                get { return mProvider == null ? string.Empty : mProvider.ProviderName; }
            }

            /// <summary>当前缓冲事件数量。</summary>
            public int BufferedCount
            {
                get { return mBuffer.Count; }
            }

            /// <summary>绑定初始来源。</summary>
            internal void Bind(IInputProvider provider)
            {
                mProvider = provider;
                mProvider?.Initialize();
                IsInitialized = true;
            }

            /// <summary>
            /// 替换来源：先关闭旧来源，再清空缓冲，最后初始化新来源。
            /// 顺序不可交换，否则旧来源的残留事件会被新来源的事件混入。
            /// </summary>
            public void ReplaceProvider(IInputProvider provider)
            {
                mBuffer.Clear();
                mProvider?.Shutdown();
                mProvider = provider;
                mProvider?.Initialize();
                IsInitialized = true;
            }

            /// <summary>
            /// 每帧轮询。只转发当前来源在本帧产出的事件，
            /// 超过单帧上限的余量留待下一帧继续转发，不丢弃。
            /// </summary>
            public void Pump(float deltaTime)
            {
                if (!IsInitialized || mProvider == null)
                {
                    return;
                }

                mProvider.Poll(deltaTime);
                int forwarded = 0;
                while (forwarded < MaxEventsPerFrame && mProvider.TryDequeue(out InputEvent inputEvent))
                {
                    mBuffer.Enqueue(inputEvent);
                    forwarded++;
                }
            }

            /// <summary>取出一个已缓冲事件。</summary>
            public bool TryDequeue(out InputEvent inputEvent)
            {
                if (mBuffer.Count == 0)
                {
                    inputEvent = default;
                    return false;
                }

                inputEvent = mBuffer.Dequeue();
                return true;
            }

            /// <summary>清空已缓冲事件。</summary>
            public void Clear()
            {
                mBuffer.Clear();
            }

            /// <summary>取当前来源描述。</summary>
            public string DescribeProvider()
            {
                if (mProvider == null)
                {
                    return "none";
                }

                return mProvider.ProviderName + (mProvider.IsAvailable ? "" : "(unavailable)");
            }

            /// <summary>关闭服务并释放来源。</summary>
            public void Dispose()
            {
                mBuffer.Clear();
                mProvider?.Shutdown();
                mProvider = null;
                IsInitialized = false;
            }
        }
    }
}
