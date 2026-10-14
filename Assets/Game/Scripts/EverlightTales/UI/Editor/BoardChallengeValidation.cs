using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using UnityEditor;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>项目纯规则的EditMode状态断言；不运行场景、不修改玩家存档或生产表。</summary>
    public static partial class BoardChallengeValidation
    {
        private static readonly List<string> Lines = new List<string>();

        [MenuItem("Game Framework/EverlightTales/Board/验证障碍异常规则")]
        public static void Run()
        {
            if (UnityEditor.EditorApplication.isPlaying) throw new InvalidOperationException("停止Play Mode后执行EditMode规则检查。");
            Lines.Clear();
            try { Provenance(); FacilityOccupancy(); InstanceBindings(); MovementBoundaries(); GrowthAndRelease(); Anomalies(); Lines.Add("PASS ALL"); }
            catch (Exception exception) { Lines.Add("FAIL " + exception); throw; }
            finally
            {
                Directory.CreateDirectory("Library/BoardChallengeValidation");
                File.WriteAllLines("Library/BoardChallengeValidation/editmode.txt", Lines);
                UnityEngine.Debug.Log("[BoardChallenge][EditMode] " + Lines.Last());
            }
        }

        private static void Check(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException(label);
            Lines.Add("PASS " + label);
        }

        private static SettlementContext Context() => new SettlementContext(new BoardState(3), new SettleState(),
            new SettlementEventQueue(), new TapScoreState(), new Everlight.Tales.Board.SessionState(), new List<SettlementEvent>());

        private static void Provenance()
        {
            SettlementContext context = Context();
            context.Queue.Enqueue(new Marker("a", true));
            context.Queue.Enqueue(new Marker("b"));
            context.DrainQueue();
            SettlementEvent[] facts = context.Log.Where(x => x.Kind == SettlementEventKind.TriggerResolved).ToArray();
            SettlementEvent a = facts.First(x => x.Message == "a"), b = facts.First(x => x.Message == "b");
            SettlementEvent[] children = facts.Where(x => x.Message == "child").ToArray();
            Check(a.Provenance.RootEventId != b.Provenance.RootEventId,
                "independent same-tap effects have distinct roots even with same source and target");
            Check(children.Length == 2 && children.All(x => x.Provenance.RootEventId == a.Provenance.RootEventId
                && x.Provenance.ParentEventId == a.Provenance.EventId)
                && children[0].Provenance.EventId != children[1].Provenance.EventId,
                "derived branches preserve root and parent with unique event IDs");
            Check(facts.Count(x => x.Message == "child") == 2,
                "ordinary repeated interactions are retained");
            string[] order = facts.Select(x => x.Message).ToArray();
            Check(Array.LastIndexOf(order, "child") < Array.IndexOf(order, "echo")
                && Array.IndexOf(order, "echo") < Array.IndexOf(order, "later"),
                "deferred echo runs after root propagation and before next queued continuation");
            Check(facts.First(x => x.Message == "echo").Provenance.Origin == EffectOrigin.Echo,
                "echo retains explicit derived origin");
            TriggerResolution translated = new CollisionTriggerResolver().Resolve(new SettlementResult(0, context.Log, true, 0, 0));
            int index = 0; bool identities = true;
            while (translated.Queue.TryDequeue(out IBoardEvent value))
            {
                identities &= value.Provenance.RootEventId == facts[index].Provenance.RootEventId
                    && value.Provenance.EventId == facts[index].Provenance.EventId
                    && value.Provenance.ParentEventId == facts[index].Provenance.ParentEventId;
                index++;
            }
            Check(index == facts.Length && identities, "translation to ability FIFO preserves every original identity and repeated fact");
            using (context.BeginEffect(BoardEffectKind.Blast, 1, default, independent: true))
            {
                Check(context.TryLimited(9, EffectLimit.BlastDamage) && !context.TryLimited(9, EffectLimit.BlastDamage),
                    "only declared limited effects deduplicate within one root");
            }
            using (context.BeginEffect(BoardEffectKind.Blast, 1, default, independent: true))
                Check(context.TryLimited(9, EffectLimit.BlastDamage), "independent blast root may affect same target again");
            SettlementContext replay = Context(); replay.Queue.Enqueue(new Marker("a", true)); replay.Queue.Enqueue(new Marker("b")); replay.DrainQueue();
            Check(context.Log.Select(x => x.ToString()).SequenceEqual(replay.Log.Select(x => x.ToString())),
                "same inputs replay identical provenance and event order without prior transaction dedupe state");
            SettlementContext direct = Context();
            using (direct.BeginEffect(BoardEffectKind.Sound, 1, default))
                direct.DeferUntilRootCompletes(new Marker("echo"), direct.Derive(BoardEffectKind.Sound, 1, default, echo: true));
            direct.DrainQueue();
            Check(direct.Log.Any(x => x.Message == "echo"), "direct emission without queue children still flushes deferred effect");
        }

        private sealed class Marker : ISettlementEvent
        {
            private readonly bool _spawn;
            public string Name { get; }
            public Marker(string name, bool spawn = false) { Name = name; _spawn = spawn; }
            public void Apply(SettlementContext context)
            {
                context.Log.Add(new SettlementEvent(SettlementEventKind.TriggerResolved, 1, targetId: 9,
                    message: Name, provenance: context.CurrentEffect));
                if (!_spawn) return;
                context.Queue.Enqueue(new Marker("child")); context.Queue.Enqueue(new Marker("child"));
                context.Queue.Enqueue(new Marker("later"), context.Derive(BoardEffectKind.Ability, 1, default, independent: true));
                context.DeferUntilRootCompletes(new Marker("echo"), context.Derive(BoardEffectKind.Sound, 1, default, echo: true));
            }
        }

        private static void FacilityOccupancy()
        {
            SettlementContext context = Context(); BoardState board = context.Board;
            BoardEntity gate = BoardEntity.Obstacle(1, new ObstacleConfig(ObstacleType.LinkedGate,
                extra: ExtraParamSet.Parse("InitialOpen=true;ToggleOncePerRoot=true;DeferCloseWhenOccupied=true", "test gate")));
            BoardEntity part = BoardEntity.Part(2, PartType.None);
            Check(board.Place(gate, default) == PlaceResult.Ok && board.Place(part, new HexCoord(-1, 0)) == PlaceResult.Ok
                && board.Move(part, default) == MoveResult.Ok && board.OccupantAt(default) == part && board.FixtureAt(default) == gate,
                "open gate shares cell with exactly one occupant and keeps both stable IDs");
            using (context.BeginEffect(BoardEffectKind.Control, 10, default))
            {
                Check(ObstacleService.ToggleGate(context, gate) && gate.PendingClose && gate.IsOpen,
                    "closing occupied gate defers without deleting occupant");
                Check(!ObstacleService.ToggleGate(context, gate) && gate.PendingClose,
                    "same-root gate signal cannot toggle twice");
            }
            using (context.BeginEffect(BoardEffectKind.Control, 10, default))
                Check(ObstacleService.ToggleGate(context, gate) && !gate.PendingClose && gate.IsOpen,
                    "independent opening signal cancels pending close");
            using (context.BeginEffect(BoardEffectKind.Control, 10, default)) ObstacleService.ToggleGate(context, gate);
            Check(board.Move(part, new HexCoord(1, 0)) == MoveResult.Ok && !gate.IsOpen && !gate.PendingClose
                && board.Move(part, default) == MoveResult.Occupied,
                "gate closes when cell vacates and all movement queries reject closed gate");
            BoardEntity seat = BoardEntity.Obstacle(3, new ObstacleConfig(ObstacleType.ClampingSeat,
                extra: ExtraParamSet.Parse("AllowTags=normal;ReleaseWindowTaps=1", "test seat")));
            board.Place(seat, new HexCoord(-1, 0));
            Check(board.Move(part, seat.Coord) == MoveResult.Ok && seat.ClampedEntityId == part.Id
                && part.ClampedByEntityId == seat.Id && !part.IsMovable,
                "entering allowed clamp seat fixes occupant through shared movement rule");
            var session = new Everlight.Tales.Board.SessionState { ArmMoves = 1 };
            Check(ArmService.Move(board, session, part, new HexCoord(-2, 0)) == ArmMoveResult.FixedEntity && session.ArmMoves == 1,
                "clamped entity rejects mechanical arm without spending charge");
            Check(ObstacleService.Release(board, seat) && part.IsMovable && seat.ReleaseWindowRemaining == 1
                && !ObstacleService.TryClamp(seat, part), "release opens configured window without reclamping same occupant");
            BoardEntity task = BoardEntity.MovableTaskMarker(4, null); board.Place(task, new HexCoord(-2, 1));
            board.Move(part, new HexCoord(-2, 0));
            Check(board.Move(task, seat.Coord) == MoveResult.Ok && task.IsMovable,
                "normal-only clamp never captures task marker");
            BoardEntity freshSeat = BoardEntity.Obstacle(8, seat.ObstacleSetup); board.Place(freshSeat, new HexCoord(-2, 2));
            Check(board.Move(task, freshSeat.Coord) == MoveResult.Ok && task.IsMovable && freshSeat.ClampedEntityId == 0
                && freshSeat.ReleaseWindowRemaining == 0, "tag rejection is independent of active release window");
            BoardEntity shutter = BoardEntity.Obstacle(5, new ObstacleConfig(ObstacleType.OneWayShutter,
                passDirection: 2, extra: ExtraParamSet.Parse("ToggleDirection=4", "test shutter")));
            board.Place(shutter, new HexCoord(2, -1));
            Check(board.BlockerAt(shutter.Coord, HexDirection.D2) == null && board.BlockerAt(shutter.Coord, HexDirection.D0) == shutter,
                "shutter permits configured incoming direction only");
            ObstacleService.ToggleShutter(shutter);
            Check(shutter.PassDirection == HexDirection.D4, "shutter toggles to configured non-opposite direction");
            ObstacleService.ToggleShutter(shutter);
            Check(shutter.PassDirection == HexDirection.D2, "shutter returns to configured original direction");
            BoardEntity wall = BoardEntity.Obstacle(6, new ObstacleConfig(ObstacleType.FixedWall)); board.Place(wall, new HexCoord(0, 1));
            Check(!ObstacleService.Damage(context, wall) && board.TryGetEntity(wall.Id, out _), "permanent wall ignores ordinary damage");
            BoardEntity partition = BoardEntity.Obstacle(7, new ObstacleConfig(ObstacleType.BrittlePartition, 3,
                extra: ExtraParamSet.Parse("RootBlastOncePerRoot=true", "test partition")));
            board.Place(partition, new HexCoord(1, 1));
            using (context.BeginEffect(BoardEffectKind.Blast, 10, default))
            {
                Check(ObstacleService.Damage(context, partition), "first path of blast root damages partition");
                using (context.BeginEffect(BoardEffectKind.Blast, 11, default))
                    Check(!ObstacleService.Damage(context, partition) && partition.Durability == 2,
                        "derived blast path cannot damage same partition twice");
            }
            using (context.BeginEffect(BoardEffectKind.Blast, 10, default))
                Check(ObstacleService.Damage(context, partition) && partition.Durability == 1,
                    "independent blast root damages partition again");
            using (context.BeginEffect(BoardEffectKind.Control, 10, default)) ObstacleService.ToggleGate(context, gate);
            board.Move(part, gate.Coord);
            using (context.BeginEffect(BoardEffectKind.Control, 10, default)) ObstacleService.ToggleGate(context, gate);
            board.Move(task, new HexCoord(-1, 1));
            BoardEntity clamped = BoardEntity.Part(9, PartType.None); board.Place(clamped, new HexCoord(-1, 2));
            board.Move(clamped, freshSeat.Coord);
            SnapshotFixtures(board, new[] { gate.ObstacleSetup, seat.ObstacleSetup, shutter.ObstacleSetup,
                wall.ObstacleSetup, partition.ObstacleSetup }, clamped.Id);
        }

        private static void SnapshotFixtures(BoardState board, IEnumerable<ObstacleConfig> configs, int clampedId)
        {
            var original = new List<ObstacleConfig>();
            foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
                if (ObstacleCatalog.Get(type) != null) original.Add(ObstacleCatalog.Get(type));
            try
            {
                ObstacleCatalog.Install(configs);
                var helper = new NewtonsoftJsonHelper(); var saved = new RepairAttemptSave();
                BoardSnapshotCodec.Capture(saved, board); string json = helper.ToJson(saved);
                BoardState restored = BoardSnapshotCodec.Restore(helper.ToObject<RepairAttemptSave>(json));
                var again = new RepairAttemptSave(); BoardSnapshotCodec.Capture(again, restored);
                Check(helper.ToJson(again) == json, "layered fixture pending close clamp and release window roundtrip without reapplying entry effects");
                restored.TryGetEntity(clampedId, out BoardEntity clamped);
                Check(restored.Move(clamped, new HexCoord(-1, 2)) == MoveResult.FixedEntity,
                    "restored reciprocal clamp still prevents movement");
                var invalid = helper.ToObject<RepairAttemptSave>(json);
                invalid.Entities.First(x => x.ObstacleType == ObstacleType.ClampingSeat && x.ClampedEntityId != 0).ClampedEntityId = 999;
                bool rejected = false;
                try { BoardSnapshotCodec.Restore(invalid); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "snapshot rejects dangling clamp graph rather than guessing relation");
                invalid = helper.ToObject<RepairAttemptSave>(json); invalid.Version = 2; rejected = false;
                try { BoardSnapshotCodec.Restore(invalid); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "previous snapshot schema is explicitly incompatible with facility relationships");
            }
            finally { ObstacleCatalog.Install(original); }
        }

        private static LevelBoardConfig FixtureLevel(params FixedElementConfig[] elements) => new LevelBoardConfig(
            "binding-fixture", 3, Array.Empty<KeyPieceConfig>(), Array.Empty<PartType>(), 0, 0, fixedElements: elements);

        private static FixedElementConfig Fixed(int id, ObstacleType type, HexCoord cell, string relation = null) =>
            new FixedElementConfig(FixedElementKind.Obstacle, cell, type,
                repairTarget: type == ObstacleType.PressurePillar || type == ObstacleType.ProliferatingSeal && string.IsNullOrEmpty(relation)
                    ? new RepairTargetConfig(new[] { PartType.RivetPliers }, 1, 1, 20) : null,
                elementId: id, instanceData: relation);

        private static void Reject(Action action, string label)
        {
            bool rejected = false;
            try { action(); } catch (ArgumentException) { rejected = true; }
            Check(rejected, label);
        }

        private static void RestoreParts(PartConfig[] original)
        {
            if (original.Length != 0) { PartCatalog.Install(original); return; }
            // Install有意拒绝空目录；Editor夹具归还尚未加载的初态，不新增生产侧清空API。
            var field = typeof(PartCatalog).GetField("_byType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            ((System.Collections.IDictionary)field.GetValue(null)).Clear();
        }

        private static void InstanceBindings()
        {
            PartConfig[] originalParts = PartCatalog.All().ToArray();
            if (PartCatalog.Get(PartType.RivetPliers) == null) PartCatalog.Install(new[] { new PartConfig(PartType.RivetPliers, 4, 0, 0) });
            var original = new List<ObstacleConfig>();
            foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
                if (ObstacleCatalog.Get(type) != null) original.Add(ObstacleCatalog.Get(type));
            var configs = new[]
            {
                new ObstacleConfig(ObstacleType.FixedWall), new ObstacleConfig(ObstacleType.BrittlePartition,2),
                new ObstacleConfig(ObstacleType.LinkedGate, extra:ExtraParamSet.Parse("InitialOpen=true", "gate")),
                new ObstacleConfig(ObstacleType.ClampingSeat, extra:ExtraParamSet.Parse("AllowTags=task;ReleaseWindowTaps=1", "seat")),
                new ObstacleConfig(ObstacleType.ProliferatingSeal,1, extra:ExtraParamSet.Parse("GrowthPerTap=1;SegmentMaxDurability=1", "seal")),
                new ObstacleConfig(ObstacleType.PressurePillar,3, extra:ExtraParamSet.Parse("AttachedPartitions=2;ProtectedObject=true;Repairable=true", "pillar"))
            };
            try
            {
                ObstacleCatalog.Install(configs);
                var one = Fixed(101,ObstacleType.BrittlePartition,new HexCoord(-1,0));
                var two = Fixed(102,ObstacleType.BrittlePartition,new HexCoord(0,-1));
                var pillarRow = Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101,102;ProtectedElementId=301");
                var protectedRow = new FixedElementConfig(FixedElementKind.TaskMarker,new HexCoord(2,-1),elementId:301);
                var controller = new FixedElementConfig(FixedElementKind.Facility,new HexCoord(-1,1),elementId:401,instanceData:"LinkedElementIds=501");
                var gate = Fixed(501,ObstacleType.LinkedGate,new HexCoord(0,1));
                var seat = Fixed(601,ObstacleType.ClampingSeat,new HexCoord(0,2),"InitialClampedElementId=602");
                var marker = new FixedElementConfig(FixedElementKind.MovableMarker,new HexCoord(0,2),elementId:602);
                var root = Fixed(701,ObstacleType.ProliferatingSeal,new HexCoord(-2,1));
                var segment = Fixed(702,ObstacleType.ProliferatingSeal,new HexCoord(-2,2),"GrowthRootElementId=701");
                LevelBoardConfig level = FixtureLevel(one,two,pillarRow,protectedRow,controller,gate,seat,marker,root,segment);
                var rng = new RandomService(12);
                BoardState board = InitialBoardBuilder.Build(level,Array.Empty<PartType>(),rng);
                BoardEntity pillar = board.Entities.First(x=>x.ObstacleType==ObstacleType.PressurePillar);
                BoardEntity clamp = board.Entities.First(x=>x.ObstacleType==ObstacleType.ClampingSeat);
                Check(pillar.LinkedElementIds.SequenceEqual(new[]{1,2}) && pillar.ProtectedElementId == 4
                    && clamp.ClampedEntityId==8 && board.Entities.First(x=>x.Id==8).ClampedByEntityId==clamp.Id,
                    "two-stage assembly resolves stable row IDs into typed runtime relationships");
                Check(board.Entities.First(x=>x.Id==10).GrowthRootEntityId==9 && !board.Entities.First(x=>x.Id==10).SealActive
                    && rng.ConsumedCount==0, "seal graph resolves to one root without random consumption");
                var saved = new RepairAttemptSave(); BoardSnapshotCodec.Capture(saved,board);
                var json = new NewtonsoftJsonHelper(); var restored = BoardSnapshotCodec.Restore(json.ToObject<RepairAttemptSave>(json.ToJson(saved)));
                var again = new RepairAttemptSave(); BoardSnapshotCodec.Capture(again,restored);
                Check(json.ToJson(saved)==json.ToJson(again), "instance attachment protection root and clamp graphs survive exact snapshot roundtrip");
                Reject(()=>Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101,101"),"duplicate instance references rejected");
                Reject(()=>Fixed(701,ObstacleType.ProliferatingSeal,new HexCoord(-2,1),"GrowthPerTap=2"),"instance binding cannot override type parameters");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(one,Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101,999"))),
                    "dangling or cross-level attachment rejected before play");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(one,Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101"))),
                    "attachment count must match type configuration");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(one,protectedRow,Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101,301"))),
                    "wrong attachment type rejected");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(one,Fixed(101,ObstacleType.BrittlePartition,new HexCoord(1,1)))),"duplicate fixed row ID rejected");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(Fixed(701,ObstacleType.ProliferatingSeal,new HexCoord(-2,1),"GrowthRootElementId=702"),
                    Fixed(702,ObstacleType.ProliferatingSeal,new HexCoord(-2,2),"GrowthRootElementId=701"))),"cyclic seal roots rejected");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(Fixed(601,ObstacleType.ClampingSeat,new HexCoord(0,2),"InitialClampedElementId=101"),one)),
                    "invalid initial clamp type rejected");
                var badRng=new RandomService(12);
                Reject(()=>InitialBoardBuilder.Build(FixtureLevel(Fixed(101,ObstacleType.FixedWall,new HexCoord(9,9))),Array.Empty<PartType>(),badRng),
                    "invalid fixed coordinate blocks initialization");
                Check(badRng.ConsumedCount==0,"invalid assembly never consumes random draws");
                Check(InitialBoardBuilder.Build(FixtureLevel(new FixedElementConfig(FixedElementKind.Obstacle,new HexCoord(0,0),ObstacleType.FixedWall)),
                    Array.Empty<PartType>(),new RandomService(12)).EntityCount==1,"legacy empty binding rows remain compatible");

                var context=new SettlementContext(board,new SettleState(),new SettlementEventQueue(),new TapScoreState(),
                    new Everlight.Tales.Board.SessionState(),new List<SettlementEvent>());
                ObstacleService.Damage(context,pillar);
                Check(ObstacleService.RepairPillar(board,pillar) && pillar.Durability==pillar.MaxDurability
                    && !ObstacleService.RepairPillar(board,pillar),"repair restores surviving pillar only up to configured maximum");
                board.TryGetEntity(1,out BoardEntity removedAttachment); board.Remove(removedAttachment);
                BoardEntity unrelated=BoardEntity.Obstacle(board.AllocateEntityId(),configs[1]); board.Place(unrelated,new HexCoord(2,0));
                ObstacleService.Damage(context,pillar); ObstacleService.Damage(context,pillar); ObstacleService.Damage(context,pillar);
                Check(!board.TryGetEntity(pillar.Id,out _) && !board.TryGetEntity(2,out _) && board.TryGetEntity(unrelated.Id,out _)
                    && board.StructureAccidents==1,"pillar collapse removes only surviving explicit attachments and records bound protection accident once");
                Check(!ObstacleService.RepairPillar(board,pillar) && !ObstacleService.Damage(context,pillar)
                    && board.StructureAccidents==1,"removed pillar cannot be damaged rewarded or resurrected again");
                BoardState unbound=InitialBoardBuilder.Build(FixtureLevel(one,two,
                    Fixed(201,ObstacleType.PressurePillar,new HexCoord(1,0),"LinkedElementIds=101,102")),Array.Empty<PartType>(),new RandomService(12));
                BoardEntity unboundPillar=unbound.Entities.First(x=>x.ObstacleType==ObstacleType.PressurePillar);
                var unboundContext=new SettlementContext(unbound,new SettleState(),new SettlementEventQueue(),new TapScoreState(),
                    new Everlight.Tales.Board.SessionState(),new List<SettlementEvent>());
                for(int i=0;i<3;i++) ObstacleService.Damage(unboundContext,unboundPillar);
                Check(unbound.StructureAccidents==0,"type protection flag alone cannot create an unbound protection accident");
                Reject(()=>FixedElementBinding.Validate(FixtureLevel(new FixedElementConfig(FixedElementKind.Obstacle,
                    default,ObstacleType.PressurePillar,elementId:201,instanceData:"LinkedElementIds=101,102"),one,two)),
                    "repairable obstacle cannot enter with undeclared repair configuration");
                ObstacleCatalog.Install(configs.Where(x=>x.Type!=ObstacleType.ClampingSeat)
                    .Concat(new[]{new ObstacleConfig(ObstacleType.ClampingSeat)}));
                var normalLevel=new LevelBoardConfig("normal initial clamp",3,new[]{new KeyPieceConfig(PartType.RivetPliers,default)},
                    Array.Empty<PartType>(),1,0,fixedElements:new[]{Fixed(601,ObstacleType.ClampingSeat,default)});
                BoardState initialClamp=InitialBoardBuilder.Build(normalLevel,Array.Empty<PartType>(),new RandomService(12));
                Check(initialClamp.FixtureAt(default).ClampedEntityId==initialClamp.OccupantAt(default).Id
                    && !initialClamp.OccupantAt(default).IsMovable,"ordinary key piece initialized on seat clamps through same entry rule");
                Reject(()=>ObstacleCatalog.Install(new[]{new ObstacleConfig(ObstacleType.TurningRail,passDirection:6)}),"invalid rail direction rejected before play");
                Reject(()=>ObstacleCatalog.Install(new[]{new ObstacleConfig(ObstacleType.OneWayShutter,passDirection:0,
                    extra:ExtraParamSet.Parse("ToggleDirection=6","bad shutter"))}),"invalid alternate shutter direction rejected before play");
                Reject(()=>ObstacleCatalog.Install(new[]{new ObstacleConfig(ObstacleType.BrittlePartition,2,
                    extra:ExtraParamSet.Parse("RootBlastOncePerRoot=false","bad partition"))}),"unsupported unlimited same-root blast rejected");
            }
            finally { ObstacleCatalog.Install(original); RestoreParts(originalParts); }
        }

        private static void MovementBoundaries()
        {
            SettlementContext context = Context(); BoardState board = context.Board;
            BoardEntity rail = BoardEntity.Obstacle(1,new ObstacleConfig(ObstacleType.TurningRail,passDirection:4));
            BoardEntity part = BoardEntity.Part(2,PartType.None);
            board.Place(rail,default); board.Place(part,new HexCoord(-1,0));
            HexDirection direction = HexDirection.D0;
            Check(AbilityMovement.Step(context,part,ref direction,out _) && part.Coord==HexCoord.Zero && direction==HexDirection.D4,
                "one ability step enters configured rail and changes only remaining direction");
            Check(AbilityMovement.Step(context,part,ref direction,out _) && part.Coord==new HexCoord(-1,1),
                "remaining ability step follows non-default rail output");
            BoardEntity gate=BoardEntity.Obstacle(3,new ObstacleConfig(ObstacleType.LinkedGate,
                extra:ExtraParamSet.Parse("InitialOpen=true", "movement gate"))); board.Place(gate,new HexCoord(1,0));
            BoardEntity left=BoardEntity.Part(4,PartType.None), right=BoardEntity.Part(5,PartType.None);
            board.Place(left,new HexCoord(0,0)); board.Place(right,new HexCoord(1,0));
            using(context.BeginEffect(BoardEffectKind.Control,9,default)) ObstacleService.ToggleGate(context,gate);
            Check(board.MoveGroup(new[]{left,right},new[]{right.Coord,left.Coord},MoveSource.Push)
                && gate.PendingClose && gate.IsOpen && board.OccupantAt(gate.Coord)==left && board.EntityCount==5,
                "atomic exchange preserves occupied pending gate and every entity");
            BoardEntity closed=BoardEntity.Obstacle(6,new ObstacleConfig(ObstacleType.LinkedGate)); board.Place(closed,new HexCoord(2,0));
            HexCoord first=left.Coord, second=right.Coord;
            Check(!board.MoveGroup(new[]{left,right},new[]{closed.Coord,first},MoveSource.Push)
                && left.Coord==first && right.Coord==second && board.EntityCount==6,
                "invalid group movement rejects before removing any occupant");
            Check(board.CanMove(left,closed.Coord)==MoveResult.Occupied && board.CanMove(right,new HexCoord(0,1))==MoveResult.Ok,
                "mechanical arm target query matches actual movement blocker rule");

            SettlementContext endpointContext=Context(); BoardState endpointBoard=endpointContext.Board;
            BoardEntity endpointGate=BoardEntity.Obstacle(1,new ObstacleConfig(ObstacleType.LinkedGate,
                extra:ExtraParamSet.Parse("InitialOpen=true", "endpoint gate")));
            endpointBoard.Place(endpointGate,default); endpointBoard.SetEndpointLabel(default,7);
            BoardEntity marker=BoardEntity.MovableTaskMarker(2,new GoalConfig("gate",GoalKind.PushedInto,0,7,1));
            endpointBoard.Place(marker,new HexCoord(-1,0)); direction=HexDirection.D0;
            Check(AbilityMovement.Step(endpointContext,marker,ref direction,out _) && marker.IsLocked
                && endpointBoard.FixtureAt(default)==endpointGate && GoalEvaluator.IsComplete(endpointBoard,marker),
                "endpoint goal and open fixture coexist with the same pushed marker");
            BoardEntity seat=BoardEntity.Obstacle(3,new ObstacleConfig(ObstacleType.ClampingSeat));
            endpointBoard.Place(seat,new HexCoord(1,0)); BoardEntity mover=BoardEntity.Part(4,PartType.None);
            endpointBoard.Place(mover,new HexCoord(2,0)); direction=HexDirection.D3;
            Check(AbilityMovement.Step(endpointContext,mover,ref direction,out _) && !mover.IsMovable
                && !AbilityMovement.Step(endpointContext,mover,ref direction,out _) && mover.Coord==seat.Coord,
                "multi-step ability stops at clamp instead of counting rejected moves");
        }

        private static void GrowthAndRelease()
        {
            PartConfig[] originalParts = PartCatalog.All().ToArray();
            var originalObstacles = new List<ObstacleConfig>();
            foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
                if (ObstacleCatalog.Get(type) != null) originalObstacles.Add(ObstacleCatalog.Get(type));
            var sealConfig = new ObstacleConfig(ObstacleType.ProliferatingSeal,1,
                extra:ExtraParamSet.Parse("GrowthPerTap=2;SegmentMaxDurability=2", "growth fixture"));
            var seatConfig = new ObstacleConfig(ObstacleType.ClampingSeat,
                extra:ExtraParamSet.Parse("AllowTags=normal;ReleaseWindowTaps=2", "release fixture"));
            try
            {
                PartCatalog.Install(new[] { new PartConfig(PartType.RivetPliers,4,0,0) });
                ObstacleCatalog.Install(new[] { sealConfig, seatConfig });
                SettlementContext context=Context(); BoardState board=context.Board;
                BoardEntity root=BoardEntity.Obstacle(1,sealConfig,new RepairTargetConfig(new[]{PartType.RivetPliers},1,1,20));
                BoardEntity oldSegment=BoardEntity.SealSegment(2,sealConfig,root.Id);
                BoardEntity rivet=BoardEntity.Part(3,PartType.RivetPliers), blocker=BoardEntity.Part(4,PartType.None);
                board.Place(root,default); board.Place(oldSegment,new HexCoord(0,1)); board.Place(rivet,new HexCoord(-1,0));
                board.Place(blocker,new HexCoord(1,-1)); board.SetEndpointLabel(new HexCoord(1,0),5);
                Check(ObstacleService.ApplyTapEnd(context) && board.EntityCount==6
                    && board.OccupantAt(new HexCoord(0,-1))?.ObstacleType==ObstacleType.ProliferatingSeal
                    && board.OccupantAt(new HexCoord(-1,1))?.ObstacleType==ObstacleType.ProliferatingSeal,
                    "one growth root shares configured budget across stable original segments and direction order");
                Check(board.OccupantAt(new HexCoord(1,0))==null && board.OccupantAt(blocker.Coord)==blocker
                    && board.Entities.Where(x=>x.Id>=5).All(x=>x.Durability==2 && x.MaxDurability==2 && x.GrowthRootEntityId==root.Id && !x.SealActive),
                    "new segments use configured durability and never overwrite endpoint or occupant");
                Check(!ObstacleService.ApplyTapEnd(context) && board.EntityCount==6,"repeated same-tap hook cannot grow again");
                context.Session.PublicRepairEnergy=1;
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,rivet.Id)); context.DrainQueue();
                Check(!root.SealActive && board.EntityCount==6 && context.Session.PublicRepairEnergy==0 && context.Tap.EffectScore==20,
                    "actual rivet trigger cuts configured root with declared repair cost and score without removing segments");
                context.Session.PublicRepairEnergy=1;
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,rivet.Id)); context.DrainQueue();
                Check(context.Session.PublicRepairEnergy==1 && context.Tap.EffectScore==20,"already cut root cannot reward or charge again even when energy is available");

                BoardEntity seat=BoardEntity.Obstacle(board.AllocateEntityId(),seatConfig);
                BoardEntity part=BoardEntity.Part(board.AllocateEntityId(),PartType.None);
                board.Place(seat,new HexCoord(2,-1)); board.Place(part,new HexCoord(2,0)); board.Move(part,seat.Coord);
                ObstacleService.Release(board,seat);
                context.Session.TapSerial++; ObstacleService.ApplyTapEnd(context);
                Check(seat.ReleaseWindowRemaining==1 && board.EntityCount==8,"cut root stops future growth while first complete release tap elapses");
                Check(!ObstacleService.ApplyTapEnd(context) && seat.ReleaseWindowRemaining==1,"duplicate tap hook cannot shorten release window");
                context.Session.TapSerial++; ObstacleService.ApplyTapEnd(context);
                Check(seat.ReleaseWindowRemaining==0 && !ObstacleService.TryClamp(seat,part) && part.IsMovable,
                    "expired window does not reclamp the same stationary released ID");
                Check(board.Move(part,new HexCoord(2,0))==MoveResult.Ok && board.Move(part,seat.Coord)==MoveResult.Ok && !part.IsMovable,
                    "leaving then reentering after full window restores clamp eligibility");
                var saved=new RepairAttemptSave(); BoardSnapshotCodec.Capture(saved,board); var helper=new NewtonsoftJsonHelper();
                BoardState restored=BoardSnapshotCodec.Restore(helper.ToObject<RepairAttemptSave>(helper.ToJson(saved)));
                var again=new RepairAttemptSave(); BoardSnapshotCodec.Capture(again,restored);
                Check(helper.ToJson(saved)==helper.ToJson(again),"grown segment capacities root cut and boundary serial survive exact snapshot roundtrip");
                SettlementContext full=Context();
                BoardEntity fullRoot=BoardEntity.Obstacle(1,sealConfig,new RepairTargetConfig(new[]{PartType.RivetPliers},1,1,20));
                full.Board.Place(fullRoot,default); full.Board.SetEndpointLabel(new HexCoord(1,0),5);
                for(int i=1;i<HexDirections.Count;i++) full.Board.Place(BoardEntity.Part(i+1,PartType.None),HexCoord.Zero.Neighbor(HexDirections.All[i]));
                int allocation=full.Board.NextEntityId;
                ObstacleService.ApplyTapEnd(full);
                Check(full.Board.EntityCount==6 && full.Board.NextEntityId==allocation,"full neighborhood stops growth without covering endpoint or allocating phantom segment");
            }
            finally { RestoreParts(originalParts); ObstacleCatalog.Install(originalObstacles); }
        }
    }
}
