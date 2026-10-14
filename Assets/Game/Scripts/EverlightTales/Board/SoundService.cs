using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>声音是带来源的逻辑信号。缓存方只在真正释放时调用Emit；中继调用Forward。</summary>
    public static class SoundService
    {
        public static bool Emit(SettlementContext context, int sourceId, HexCoord at, HexDirection port, int endpointScore)
        {
            if (AnomalyService.IsSilenced(context.Board, at)) return false;
            using var scope = context.BeginEffect(BoardEffectKind.Sound, sourceId, at, port);
            context.Log.Add(new SettlementEvent(SettlementEventKind.SoundEmitted, sourceId, at, at,
                message: "sound", provenance: context.CurrentEffect));
            Forward(context, sourceId, at, port, endpointScore);
            if (context.CurrentEffect.Origin == EffectOrigin.Echo) return true;
            AnomalyRegion region = AnomalyService.Select(context.Board, AnomalyType.EchoZone, at);
            if (region != null && region.EchoRemaining > 0)
            {
                region.EchoRemaining--;
                context.DeferUntilRootCompletes(new SoundPropagation(sourceId, at, port, endpointScore, true),
                    context.Derive(BoardEffectKind.Sound, sourceId, at, port, echo: true));
            }
            return true;
        }
        public static void Forward(SettlementContext context, int sourceId, HexCoord at, HexDirection port, int endpointScore) =>
            context.Queue.Enqueue(new SoundPropagation(sourceId, at, port, endpointScore, false));

        public static bool HasReceiver(BoardState board, HexCoord at)
        {
            return Receives(board.OccupantAt(at)) || Receives(board.FixtureAt(at));
        }
        private static bool Receives(BoardEntity entity) => entity != null && (entity.SoundEndpoint != null
            || entity.Kind == EntityKind.Part && entity.PartType == PartType.ConductiveBridge);

        private sealed class SoundPropagation : ISettlementEvent
        {
            private readonly int _source, _score;
            private readonly HexCoord _at;
            private readonly HexDirection _port;
            private readonly bool _echo;
            public string Name => "sound";
            public SoundPropagation(int source, HexCoord at, HexDirection port, int score, bool echo)
            { _source = source; _at = at; _port = port; _score = score; _echo = echo; }
            public void Apply(SettlementContext context)
            {
                // 复制的是已经释放的声包；原传播中的移动或静默变化不能改写发射格与端口。
                if (_echo) context.Log.Add(new SettlementEvent(SettlementEventKind.SoundEmitted, _source, _at, _at,
                    message: "echo", provenance: context.CurrentEffect));
                HexCoord destination = _at.Neighbor(_port);
                Receive(context, context.Board.OccupantAt(destination));
                Receive(context, context.Board.FixtureAt(destination));
            }
            private void Receive(SettlementContext context, BoardEntity target)
            {
                if (!Receives(target) || AnomalyService.IsSilenced(context.Board, target.Coord, target.Kind != EntityKind.Part)
                    || !context.TryLimited(target.Id, EffectLimit.SignalReceive)) return;
                if (target.Kind == EntityKind.Part && target.PartType == PartType.ConductiveBridge)
                {
                    context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Sound, _source, target.Id, _port));
                    return;
                }
                context.Tap.EffectScore += _score;
                context.Log.Add(new SettlementEvent(SettlementEventKind.SoundReceived, _source, _at, target.Coord,
                    target.Id, "sound", _score, provenance: context.CurrentEffect));
                foreach (int id in target.LinkedElementIds)
                {
                    if (!context.Board.TryGetEntity(id, out BoardEntity linked)) continue;
                    bool changed = false;
                    switch (linked.ObstacleType)
                    {
                        case ObstacleType.LinkedGate: changed = ObstacleService.ToggleGate(context, linked); break;
                        case ObstacleType.ClampingSeat:
                            if (context.TryLimited(linked.Id, EffectLimit.ClampRelease)) changed = ObstacleService.Release(context.Board, linked); break;
                        case ObstacleType.OneWayShutter:
                            if (context.TryLimited(linked.Id, EffectLimit.ShutterToggle))
                            { ObstacleService.ToggleShutter(linked); context.Board.MarkTraversalChanged(); changed = true; } break;
                    }
                    if (changed) context.Log.Add(new SettlementEvent(SettlementEventKind.ControlApplied, target.Id,
                        targetId: id, message: "sound-control", provenance: context.Derive(BoardEffectKind.Control, target.Id, target.Coord)));
                }
                if (target.SoundEndpoint == "echo-reset")
                    foreach (AnomalyRegion region in context.Board.AnomalyRegions)
                        if (region.TryEnter(target.Coord)) AnomalyService.ResetEcho(region);
            }
        }
    }
}
