using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 序章播放序列（P3-005 纯逻辑）：按步骤时长推进字幕/旁白，可跳过，播完完成。
    /// 表现层（OpeningPage）据此显示当前文本；资源（立绘/音频/播片）由表现层挂接。
    /// </summary>
    public sealed class OpeningSequence
    {
        private readonly IReadOnlyList<PrologueStepConfig> _steps;
        private int _index;
        private float _elapsed;

        public OpeningSequence(IReadOnlyList<PrologueStepConfig> steps)
        {
            _steps = steps ?? System.Array.Empty<PrologueStepConfig>();
        }

        public PrologueStepConfig Current => IsComplete ? null : _steps[_index];

        public bool IsComplete => _index >= _steps.Count;

        public int StepIndex => _index;

        /// <summary>推进 delta 秒；返回本帧是否切到下一步。</summary>
        public bool Advance(float deltaTime)
        {
            if (IsComplete)
            {
                return false;
            }

            PrologueStepConfig step = _steps[_index];
            _elapsed += deltaTime;
            if (_elapsed >= step.Duration)
            {
                _elapsed = 0f;
                _index++;
                return true;
            }

            return false;
        }

        /// <summary>跳过当前步骤（推进到下一步）。</summary>
        public void Skip()
        {
            if (!IsComplete)
            {
                _index++;
                _elapsed = 0f;
            }
        }
    }
}
