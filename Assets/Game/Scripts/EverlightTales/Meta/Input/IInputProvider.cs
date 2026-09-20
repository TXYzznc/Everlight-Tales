namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 输入来源契约。实现方负责读取各自的底层输入并把语义事件写入自身队列，
    /// 由 <see cref="InputService"/> 每帧轮询并消费。
    /// 该契约是业务与底层输入之间的唯一边界：业务代码 MUST NOT 直接读取
    /// <c>UnityEngine.Input</c>、<c>UnityEngine.InputSystem</c> 或 <c>UnityEngine.Touch</c>。
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>来源标识，用于日志与诊断。</summary>
        string ProviderName { get; }

        /// <summary>底层输入在当前平台是否可用。</summary>
        bool IsAvailable { get; }

        /// <summary>初始化来源。可重复调用，实现应向幂等。</summary>
        void Initialize();

        /// <summary>
        /// 每帧轮询底层输入，把本帧产生的语义事件写入自身队列。
        /// </summary>
        /// <param name="deltaTime">本帧真实经过时间，用于长按等时间判定。</param>
        void Poll(float deltaTime);

        /// <summary>取出一个待消费事件。</summary>
        bool TryDequeue(out InputEvent inputEvent);

        /// <summary>释放来源持有的资源并清空队列。</summary>
        void Shutdown();
    }
}
