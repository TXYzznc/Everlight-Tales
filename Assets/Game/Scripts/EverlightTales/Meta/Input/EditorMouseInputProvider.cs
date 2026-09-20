using System.Collections.Generic;
using UnityEngine;

namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 编辑器鼠标回退来源。用鼠标左键模拟触摸指针，用鼠标右键产出按钮语义，
    /// 使编辑器调试与真机走同一条事件契约。
    /// 本类型是业务侧允许读取底层输入的唯二实现之一（另一为触摸来源）。
    /// </summary>
    public sealed class EditorMouseInputProvider : IInputProvider
    {
        private const int SecondaryButtonValue = 1;

        private readonly Queue<InputEvent> mEvents = new Queue<InputEvent>(16);
        private bool mPointerDown;
        private Vector2 mLastPosition;
        private float mHeldSeconds;
        private bool mLongPressSent;

        /// <inheritdoc />
        public string ProviderName
        {
            get { return "EditorMouse"; }
        }

        /// <inheritdoc />
        public bool IsAvailable
        {
            get { return true; }
        }

        /// <inheritdoc />
        public void Initialize()
        {
            mEvents.Clear();
            ResetPointer();
        }

        /// <inheritdoc />
        public void Poll(float deltaTime)
        {
            Vector2 position = UnityEngine.Input.mousePosition;
            if (!mPointerDown && UnityEngine.Input.GetMouseButtonDown(0))
            {
                mPointerDown = true;
                mLastPosition = position;
                mHeldSeconds = 0f;
                mLongPressSent = false;
                Emit(InputEvent.Pointer(InputEventType.PressDown, position, Time.unscaledTime));
            }

            if (mPointerDown)
            {
                Vector2 delta = position - mLastPosition;
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    if (delta.sqrMagnitude > InputTiming.DragDeadZonePixels * InputTiming.DragDeadZonePixels)
                    {
                        mLastPosition = position;
                        Emit(new InputEvent(InputEventType.Drag, position, delta, 0, Time.unscaledTime));
                    }

                    TrackHold(deltaTime);
                }

                if (UnityEngine.Input.GetMouseButtonUp(0))
                {
                    Emit(InputEvent.Pointer(InputEventType.PressUp, position, Time.unscaledTime));
                    ResetPointer();
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                Emit(new InputEvent(InputEventType.Button, position, Vector2.zero, SecondaryButtonValue, Time.unscaledTime));
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out InputEvent inputEvent)
        {
            if (mEvents.Count == 0)
            {
                inputEvent = default;
                return false;
            }

            inputEvent = mEvents.Dequeue();
            return true;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            mEvents.Clear();
            ResetPointer();
        }

        private void TrackHold(float deltaTime)
        {
            if (mLongPressSent)
            {
                return;
            }

            mHeldSeconds += deltaTime;
            if (mHeldSeconds < InputTiming.LongPressSeconds)
            {
                return;
            }

            mLongPressSent = true;
            Emit(new InputEvent(InputEventType.LongPress, mLastPosition, Vector2.zero, 0, Time.unscaledTime));
        }

        private void Emit(InputEvent inputEvent)
        {
            mEvents.Enqueue(inputEvent);
        }

        private void ResetPointer()
        {
            mPointerDown = false;
            mLastPosition = Vector2.zero;
            mHeldSeconds = 0f;
            mLongPressSent = false;
        }
    }
}
