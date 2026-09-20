using System.Collections.Generic;
using UnityEngine;

namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 触摸输入来源。跟随一个稳定的手指 ID，忽略其它手指，
    /// 产出按下、抬起、拖动、长按四类语义事件。
    /// 本类型是业务侧允许读取底层输入的唯二实现之一（另一为编辑器鼠标回退）。
    /// </summary>
    public sealed class TouchInputProvider : IInputProvider
    {
        private const int PointerFingerNone = -1;

        private readonly Queue<InputEvent> mEvents = new Queue<InputEvent>(16);
        private int mPointerFingerId = PointerFingerNone;
        private Vector2 mLastPosition;
        private float mHeldSeconds;
        private bool mLongPressSent;

        /// <inheritdoc />
        public string ProviderName
        {
            get { return "Touch"; }
        }

        /// <inheritdoc />
        public bool IsAvailable
        {
            get { return UnityEngine.Input.touchSupported; }
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
            if (!IsAvailable)
            {
                if (mPointerFingerId != PointerFingerNone)
                {
                    Emit(InputEvent.Pointer(InputEventType.PressUp, mLastPosition, Time.unscaledTime));
                    ResetPointer();
                }

                return;
            }

            bool found = TryFindPointerTouch(out Touch touch);
            if (found)
            {
                HandleActiveTouch(touch, deltaTime);
                return;
            }

            if (mPointerFingerId != PointerFingerNone)
            {
                Emit(InputEvent.Pointer(InputEventType.PressUp, mLastPosition, Time.unscaledTime));
                ResetPointer();
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

        private bool TryFindPointerTouch(out Touch touch)
        {
            int count = UnityEngine.Input.touchCount;
            if (mPointerFingerId != PointerFingerNone)
            {
                for (int i = 0; i < count; i++)
                {
                    Touch candidate = UnityEngine.Input.GetTouch(i);
                    if (candidate.fingerId == mPointerFingerId)
                    {
                        touch = candidate;
                        return true;
                    }
                }

                touch = default;
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                Touch candidate = UnityEngine.Input.GetTouch(i);
                if (candidate.phase == TouchPhase.Began)
                {
                    mPointerFingerId = candidate.fingerId;
                    touch = candidate;
                    return true;
                }
            }

            touch = default;
            return false;
        }

        private void HandleActiveTouch(Touch touch, float deltaTime)
        {
            Vector2 position = touch.position;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    mLastPosition = position;
                    mHeldSeconds = 0f;
                    mLongPressSent = false;
                    Emit(InputEvent.Pointer(InputEventType.PressDown, position, Time.unscaledTime));
                    break;

                case TouchPhase.Moved:
                    Vector2 delta = position - mLastPosition;
                    mLastPosition = position;
                    if (delta != Vector2.zero)
                    {
                        Emit(new InputEvent(InputEventType.Drag, position, delta, 0, Time.unscaledTime));
                    }

                    TrackHold(deltaTime);
                    break;

                case TouchPhase.Stationary:
                    mLastPosition = position;
                    TrackHold(deltaTime);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    Emit(InputEvent.Pointer(InputEventType.PressUp, position, Time.unscaledTime));
                    ResetPointer();
                    break;
            }
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
            mPointerFingerId = PointerFingerNone;
            mLastPosition = Vector2.zero;
            mHeldSeconds = 0f;
            mLongPressSent = false;
        }
    }
}
