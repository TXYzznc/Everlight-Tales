namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 输入语义共用时间常量。触摸与鼠标两条来源 MUST 使用同一组阈值，
    /// 否则编辑器调试手感与真机不一致。
    /// </summary>
    public static class InputTiming
    {
        /// <summary>长按成立所需按住秒数。</summary>
        public const float LongPressSeconds = 0.5f;

        /// <summary>低于该像素位移的移动视为静止，避免抖动被识别为拖动。</summary>
        public const float DragDeadZonePixels = 1f;
    }
}
