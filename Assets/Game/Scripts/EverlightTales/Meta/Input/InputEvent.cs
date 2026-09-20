using System;
using UnityEngine;

namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 输入语义类型。业务侧只消费这些语义，不感知底层是触摸还是鼠标。
    /// </summary>
    public enum InputEventType
    {
        /// <summary>指针按下。</summary>
        PressDown = 0,

        /// <summary>指针抬起。</summary>
        PressUp = 1,

        /// <summary>拖动位移。</summary>
        Drag = 2,

        /// <summary>长按成立。</summary>
        LongPress = 3,

        /// <summary>按钮语义。</summary>
        Button = 4,
    }

    /// <summary>
    /// 一次输入事件的载荷。只承载语义与坐标，不承载业务判定。
    /// </summary>
    [Serializable]
    public readonly struct InputEvent
    {
        /// <summary>事件语义类型。</summary>
        public readonly InputEventType Type;

        /// <summary>指针当前屏幕位置。</summary>
        public readonly Vector2 Position;

        /// <summary>本帧位移，仅拖动事件使用。</summary>
        public readonly Vector2 Delta;

        /// <summary>按钮或长按的载荷值，未使用时为 0。</summary>
        public readonly int Value;

        /// <summary>产生时刻的帧时间。</summary>
        public readonly float Timestamp;

        /// <summary>构造一个输入事件。</summary>
        public InputEvent(InputEventType type, Vector2 position, Vector2 delta, int value, float timestamp)
        {
            Type = type;
            Position = position;
            Delta = delta;
            Value = value;
            Timestamp = timestamp;
        }

        /// <summary>构造一个不带位移与载荷的输入事件。</summary>
        public static InputEvent Pointer(InputEventType type, Vector2 position, float timestamp)
        {
            return new InputEvent(type, position, Vector2.zero, 0, timestamp);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Type + "@" + Position + " delta=" + Delta + " value=" + Value;
        }
    }
}
