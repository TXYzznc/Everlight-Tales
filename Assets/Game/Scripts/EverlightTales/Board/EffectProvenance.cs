using System;

namespace Everlight.Tales.Board
{
    public enum BoardEffectKind : byte { Interaction, Ability, Blast, Control, Sound, Light, Due, TapEnd }
    public enum EffectOrigin : byte { Original, Echo }
    public enum EffectLimit : byte { BlastDamage, GateToggle, SignalReceive, BridgeForward, ClampRelease, ShutterToggle }

    /// <summary>一次效果事实的来源；派生事件继承根，翻译和播放不重新分配身份。</summary>
    public readonly struct EffectProvenance
    {
        public readonly long RootEventId;
        public readonly long EventId;
        public readonly long ParentEventId;
        public readonly BoardEffectKind EffectKind;
        public readonly EffectOrigin Origin;
        public readonly int SourceEntityId;
        public readonly HexCoord EmittedAt;
        public readonly HexDirection Direction;
        public bool IsValid => RootEventId != 0 && EventId != 0;

        public EffectProvenance(long root, long id, long parent, BoardEffectKind kind, EffectOrigin origin,
            int source, HexCoord at, HexDirection direction)
        {
            RootEventId = root; EventId = id; ParentEventId = parent; EffectKind = kind;
            Origin = origin; SourceEntityId = source; EmittedAt = at; Direction = direction;
        }
    }

    internal readonly struct EffectLimitKey : IEquatable<EffectLimitKey>
    {
        private readonly long _root;
        private readonly int _target;
        private readonly EffectLimit _limit;
        private readonly EffectOrigin _origin;

        public EffectLimitKey(EffectProvenance source, int target, EffectLimit limit)
        {
            _root = source.RootEventId; _target = target; _limit = limit;
            // 一次回声是一次独立受限信号，允许原声和回声各接收一次；爆破仍只按根。
            _origin = limit == EffectLimit.SignalReceive || limit == EffectLimit.BridgeForward
                ? source.Origin : EffectOrigin.Original;
        }
        public bool Equals(EffectLimitKey other) => _root == other._root && _target == other._target
            && _limit == other._limit && _origin == other._origin;
        public override bool Equals(object obj) => obj is EffectLimitKey other && Equals(other);
        public override int GetHashCode() => unchecked((((_root.GetHashCode() * 397) ^ _target) * 31
            ^ (int)_limit) * 31 ^ (int)_origin);
    }
}
