using System;
using System.Collections.Generic;
using System.Linq;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;

namespace Everlight.Tales.UI.Editor
{
    public static partial class BoardChallengeValidation
    {
        private static AnomalyConfig Anomaly(AnomalyType type, string extra = null, int threshold = 0,
            int cores = 0, int period = 0, int direction = 0, int deflection = 0) =>
            new AnomalyConfig(type, deflection, direction, threshold, cores, period, ExtraParamSet.Parse(extra ?? "", "anomaly fixture"));

        private static void Anomalies()
        {
            var original = Enum.GetValues(typeof(AnomalyType)).Cast<AnomalyType>().Select(AnomalyCatalog.Get).Where(x => x != null).ToArray();
            var parts = PartCatalog.All().ToArray();
            var obstacles = Enum.GetValues(typeof(ObstacleType)).Cast<ObstacleType>().Select(ObstacleCatalog.Get).Where(x => x != null).ToArray();
            try
            {
                AnomalyCatalog.Install(new[] {
                    Anomaly(AnomalyType.LocalSettleDeflection, "Priority=10;Controllable=true", deflection:1),
                    Anomaly(AnomalyType.ExpandingRift, "MaxExpansion=1", threshold:2),
                    Anomaly(AnomalyType.EchoZone, "EchoQuota=2;Resettable=true;NoEchoOfEcho=true"),
                    Anomaly(AnomalyType.SilenceZone, "SuppressStoryNodes=false;Controllable=true"),
                    Anomaly(AnomalyType.SettlingRail,"StepLength=2", direction:2),
                    Anomaly(AnomalyType.OverloadField,"RechargeAmount=2", threshold:2, cores:2),
                    Anomaly(AnomalyType.RedrawCurtain,"MaxLayers=2;PerRoundTargets=1"),
                    Anomaly(AnomalyType.PostTapReversal,"FlipDegrees=120",period:2)
                });
                PartCatalog.Install(new[] {
                    new PartConfig(PartType.TuningFork,2,10,1,effectScorePerTarget:4),
                    new PartConfig(PartType.ConductiveBridge,2,10,1,effectScorePerTarget:8),
                    new PartConfig(PartType.MeteringRatchet,2,10,1,publicEnergyPerEffect:1),
                    new PartConfig(PartType.CalibrationProbe,2,10,1,effectScorePerTarget:8),
                    new PartConfig(PartType.RivetPliers,2,0,0)
                });
                var gateConfig = new ObstacleConfig(ObstacleType.LinkedGate);
                ObstacleCatalog.Install(new[] {gateConfig});
                SoundAndEcho(gateConfig); RedrawAndSnapshot(); MotionAndTapHooks(); ThermalAndSilence(); GravityCyclesAndGuards();
            }
            finally { AnomalyCatalog.Install(original); RestoreParts(parts); ObstacleCatalog.Install(obstacles); }
        }
        private static void SoundAndEcho(ObstacleConfig gateConfig)
        {
            SettlementContext c = Context(); BoardState b = c.Board;
            var echo = new AnomalyRegion(AnomalyType.EchoZone,new[]{new HexCoord(2,0)}); b.RegisterAnomaly(echo);
            var overlap = new AnomalyRegion(AnomalyType.EchoZone,new[]{new HexCoord(2,0),new HexCoord(1,0)}); b.RegisterAnomaly(overlap);
            var bridge1=BoardEntity.Part(1,PartType.ConductiveBridge); b.Place(bridge1,new HexCoord(1,0));
            var bridge2=BoardEntity.Part(2,PartType.ConductiveBridge); b.Place(bridge2,default);
            var endpoint=BoardEntity.Facility(3); endpoint.SetSoundEndpoint("control"); b.Place(endpoint,new HexCoord(-1,0));
            var gate=BoardEntity.Obstacle(4,gateConfig); b.Place(gate,new HexCoord(-2,1)); endpoint.LinkedElementIds.Add(gate.Id);
            Check(echo.EchoRemaining==2,"buffering without actual release consumes no echo quota");
            SoundService.Emit(c,99,new HexCoord(2,0),HexDirection.D3,4); c.DrainQueue();
            var received=c.Log.Where(x=>x.Kind==SettlementEventKind.SoundReceived).ToArray();
            Check(echo.EchoRemaining==1 && overlap.EchoRemaining==2 && received.Length==2,
                "original emission consumes selected overlap quota once; relays and echo never recopy");
            Check(received[0].Provenance.RootEventId==received[1].Provenance.RootEventId
                && received[0].Provenance.Origin==EffectOrigin.Original && received[1].Provenance.Origin==EffectOrigin.Echo
                && received[0].Provenance.EventId!=received[1].Provenance.EventId,
                "two-hop bridge paths preserve one root distinct original and echo identities");
            Check(c.Log.FindIndex(x=>x.Kind==SettlementEventKind.SoundEmitted && x.Message=="echo")
                > c.Log.FindIndex(x=>x.Kind==SettlementEventKind.SoundReceived && x.Provenance.Origin==EffectOrigin.Original),
                "actual echo dispatch waits until original two-hop propagation completes");
            Check(gate.IsOpen && c.Log.Count(x=>x.Kind==SettlementEventKind.ControlApplied)==1,
                "original and echo may each reach endpoint but linked gate toggles once per root");
            int count=c.Log.Count(x=>x.Kind==SettlementEventKind.TriggerResolved);
            SoundService.Emit(c,99,new HexCoord(2,0),HexDirection.D3,4); c.DrainQueue();
            Check(!gate.IsOpen && c.Log.Count(x=>x.Kind==SettlementEventKind.TriggerResolved)==count+4,
                "independent sound root in same tap forwards through both bridges again");
            SoundService.Emit(c,99,new HexCoord(2,0),HexDirection.D3,4); c.DrainQueue();
            Check(echo.EchoRemaining==0 && overlap.EchoRemaining==2,"exhausted winning overlap does not fall back to another echo zone");
            Check(AnomalyService.ResetEcho(echo) && echo.EchoRemaining==2,"explicit reset restores only resettable echo quota");
            TriggerResolution facts=new CollisionTriggerResolver().Resolve(new SettlementResult(0,c.Log,true,0,0));
            Check(facts.Triggers.Count==c.Log.Count(x=>x.Kind==SettlementEventKind.TriggerResolved
                || x.Kind==SettlementEventKind.SoundReceived || x.Kind==SettlementEventKind.ControlApplied),
                "Events translates sound and control facts without executing or dropping them");
        }
        private static void RedrawAndSnapshot()
        {
            SettlementContext c=Context(); BoardState b=c.Board;
            var a=BoardEntity.Part(1,PartType.TuningFork); b.Place(a,default);
            var task=BoardEntity.MovableTaskMarker(2,null); b.Place(task,new HexCoord(1,0));
            var curtain=new AnomalyRegion(AnomalyType.RedrawCurtain,new[]{a.Coord,task.Coord}); b.RegisterAnomaly(curtain);
            curtain.RegisterCarrier(b,a.Id); curtain.RegisterCarrier(b,task.Id);
            AnomalyService.ApplyRoundEnd(b,0); AnomalyService.ApplyRoundEnd(b,0);
            Check(a.DisguiseLayers==1 && task.DisguiseLayers==0 && curtain.RedrawCursor==1,"shared round callback redraws registered carrier once with persistent cursor");
            AnomalyService.ApplyRoundEnd(b,1); a.SetDisguiseLayers(0); AnomalyService.ApplyRoundEnd(b,2);
            Check(a.DisguiseLayers==1 && task.DisguiseLayers==1,"lighting reveal preserves identity and allows later scheduled redraw");
            a.SetDisguiseLayers(2); task.SetDisguiseLayers(2); AnomalyService.ApplyRoundEnd(b,3);
            Check(a.DisguiseLayers==2 && task.DisguiseLayers==2,"full redraw carriers stop after bounded scan without exceeding layer cap");
            Check(AnomalyService.CalibrateAt(b,a.Coord) && curtain.RedrawStopped,"calibration stops future redraw without removing current layers");
            a.SetDisguiseLayers(0); AnomalyService.ApplyRoundEnd(b,4); Check(a.DisguiseLayers==0,"calibrated curtain stays stopped");
            var echo=new AnomalyRegion(AnomalyType.EchoZone,new[]{a.Coord}); b.RegisterAnomaly(echo); SoundService.Emit(c,a.Id,a.Coord,HexDirection.D0,0); c.DrainQueue();
            var reversal=new AnomalyRegion(AnomalyType.PostTapReversal,new[]{a.Coord}); b.RegisterAnomaly(reversal);
            reversal.TickTap(); AnomalyService.CalibrateAt(b,a.Coord);
            var rail=new AnomalyRegion(AnomalyType.SettlingRail,new[]{a.Coord}); b.RegisterAnomaly(rail); AnomalyService.Stabilize(rail);
            var save=new RepairAttemptSave(); BoardSnapshotCodec.Capture(save,b); save.LastAnomalyTap=4; var json=new NewtonsoftJsonHelper();
            BoardState restored=BoardSnapshotCodec.Restore(json.ToObject<RepairAttemptSave>(json.ToJson(save)));
            var again=new RepairAttemptSave(); BoardSnapshotCodec.Capture(again,restored);
            Check(json.ToJson(save)==json.ToJson(again),"anomaly IDs quotas cursors carriers stop freeze and tap boundary roundtrip exactly");
            AnomalyService.ApplyRoundEnd(restored,4);
            Check(restored.EntityAt(a.Coord).DisguiseLayers==0,"restored round callback cannot redraw already processed round");
            var bad=json.ToObject<RepairAttemptSave>(json.ToJson(save)); bad.Anomalies[0].CarrierIds.Add(999);
            bool rejected=false; try { BoardSnapshotCodec.Restore(bad); } catch(ArgumentException) { rejected=true; }
            Check(rejected,"snapshot rejects unresolved historical carrier identity");
            BoardState replay=BoardSnapshotCodec.Restore(json.ToObject<RepairAttemptSave>(json.ToJson(save)));
            var left=new SettlementContext(restored,new SettleState(),new SettlementEventQueue(),new TapScoreState(),
                new Everlight.Tales.Board.SessionState(),new List<SettlementEvent>());
            var right=new SettlementContext(replay,new SettleState(),new SettlementEventQueue(),new TapScoreState(),
                new Everlight.Tales.Board.SessionState(),new List<SettlementEvent>());
            foreach(SettlementContext context in new[]{left,right})
            { SoundService.Emit(context,a.Id,a.Coord,HexDirection.D0,4);context.DrainQueue();AnomalyService.ApplyRoundEnd(context.Board,5); }
            var leftSave=new RepairAttemptSave();BoardSnapshotCodec.Capture(leftSave,restored);
            var rightSave=new RepairAttemptSave();BoardSnapshotCodec.Capture(rightSave,replay);
            Check(json.ToJson(leftSave)==json.ToJson(rightSave)
                && left.Log.Select(x=>x.ToString()).SequenceEqual(right.Log.Select(x=>x.ToString())),
                "restored echo and redraw replay preserve full state and exact event order without stale transaction limits");
        }
        private static void MotionAndTapHooks()
        {
            SettlementContext c=Context(); BoardState b=c.Board;
            var p=BoardEntity.Part(1,PartType.None); b.Place(p,new HexCoord(-1,0));
            var rift=new AnomalyRegion(AnomalyType.ExpandingRift,new[]{HexCoord.Zero},new HexCoord(1,-1),
                new[]{new HexCoord(0,1),new HexCoord(0,2)}); b.RegisterAnomaly(rift);
            Check(b.Move(p,default,MoveSource.Arm)==MoveResult.Ok && p.Coord==rift.ExitCell && rift.UseCount==1,
                "mechanical-arm movement uses actual atomic rift entry");
            b.Move(p,new HexCoord(-1,0)); b.Move(p,default,MoveSource.Push);
            Check(rift.ExpansionCount==1 && rift.UseCount==2,"rift expands at exact configured success threshold");
            b.Move(p,new HexCoord(-1,0)); var blocker=BoardEntity.Facility(2); b.Place(blocker,rift.ExitCell);
            Check(b.Move(p,default)==MoveResult.Ok && p.Coord==HexCoord.Zero && rift.UseCount==2,"occupied rift exit blocks teleport without counting a use");
            b.Remove(blocker);
            for(int i=0;i<4;i++){b.Move(p,new HexCoord(-1,0));b.Move(p,default);}
            Check(rift.ExpansionCount==1,"rift maximum expansion cap is consumed independently of available expansion cells");
            var local=new AnomalyRegion(AnomalyType.LocalSettleDeflection,new[]{rift.ExitCell}); b.RegisterAnomaly(local);
            Check(AnomalyService.GravityAt(b,p.Coord,HexDirection.D0)==HexDirection.D1,"local gravity consumes configured deflection");
            AnomalyService.CalibrateAt(b,p.Coord);
            Check(AnomalyService.GravityAt(b,p.Coord,HexDirection.D0)==HexDirection.D0,"calibration toggles controllable deflection");
            b.Move(p,new HexCoord(-1,0));
            var before=new RepairAttemptSave();BoardSnapshotCodec.Capture(before,b);var json=new NewtonsoftJsonHelper();string unchanged=json.ToJson(before);
            PreviewMove[] preview=PreviewService.Compute(b,new SettleState()).ToArray();
            var after=new RepairAttemptSave();BoardSnapshotCodec.Capture(after,b);
            Check(unchanged==json.ToJson(after),"preview simulates rift counters without mutating real anomaly or entity state");
            new TapSettlement().Settle(b,new SettleState(),new TapContext(2),c.Session);
            Check(preview.Any(x=>x.EntityId==p.Id && x.To==p.Coord),"preview endpoint matches actual first fall through rift");

            c=Context(); b=c.Board; p=BoardEntity.Part(1,PartType.None);
            HexCoord edge=b.EnumerateNormal().First(x=>!b.IsValid(x.Neighbor(HexDirection.D0))
                && b.IsValid(x.Neighbor(HexDirection.D2).Neighbor(HexDirection.D2))); b.Place(p,edge);
            var rail=new AnomalyRegion(AnomalyType.SettlingRail,new[]{p.Coord}); b.RegisterAnomaly(rail);
            var transaction=new TapSettlement(); var settle=new SettleState();
            SettlementResult result=transaction.Settle(b,settle,new TapContext(3),c.Session);
            Check(result.Events.Count(x=>x.Kind==SettlementEventKind.EntityMoved && x.Message=="ability-step")==2,
                "tap-start stationary ordinary part sinks configured two steps at tap end");
            var stabilizer=BoardEntity.RepairTarget(2,new RepairTargetConfig(new[]{PartType.RivetPliers},1,0,0));
            stabilizer.SetSettlingStabilizer(); b.Place(stabilizer,edge);
            // 明确绑定的维修节点通过真实RepairEffect停止区域；普通碰撞/校准不替代维修。
            c.Queue.Enqueue(new RepairEffect(PartType.RivetPliers,stabilizer.Id)); c.DrainQueue();
            Check(rail.SettlingStopped,"declared rivet repair stabilizer stops future settling through actual repair event");
            Check(c.Session.TapSerial==0 && result.TapQuotaAfter==2,"anomaly cleanup consumes no extra tap or serial");
            c=Context();b=c.Board;
            var reversal=new AnomalyRegion(AnomalyType.PostTapReversal,new[]{HexCoord.Zero});b.RegisterAnomaly(reversal);
            settle=new SettleState();transaction.Settle(b,settle,new TapContext(3),c.Session);c.Session.TapSerial++;
            transaction.Settle(b,settle,new TapContext(2),c.Session);c.Session.TapSerial++;
            Check(settle.GravityDirection==HexDirection.D2 && reversal.TapCounter==0,"configured non-default flip resets exact tap period");
            AnomalyService.CalibrateAt(b,default);
            transaction.Settle(b,settle,new TapContext(3),c.Session);c.Session.TapSerial++;
            transaction.Settle(b,settle,new TapContext(2),c.Session);c.Session.TapSerial++;
            Check(settle.GravityDirection==HexDirection.D2 && !reversal.FreezeNextReversal && reversal.TapCounter==0,
                "calibration freezes exactly next scheduled reversal and consumes freeze");
            transaction.Settle(b,settle,new TapContext(1,isTerminalTap:true),c.Session);
            Check(reversal.TapCounter==0 && settle.GravityDirection==HexDirection.D2,"terminal tap neither ticks nor flips post-tap reversal");
        }
        private static void ThermalAndSilence()
        {
            SettlementContext c=Context();BoardState b=c.Board;
            var p=BoardEntity.Part(1,PartType.MeteringRatchet);b.Place(p,default);p.SetEnergy(4);
            var field=new AnomalyRegion(AnomalyType.OverloadField,new[]{p.Coord});b.RegisterAnomaly(field);
            for(int i=0;i<2;i++){c.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,p.Id));c.DrainQueue();}
            Check(field.Heat==0 && field.Cores==1 && p.Energy==4 && c.Session.PublicRepairEnergy==2,
                "actual chargeable executions heat field; threshold recharges configured amount without recursive heat");
            for(int i=0;i<2;i++){c.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,p.Id));c.DrainQueue();}
            int energy=p.Energy;c.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,p.Id));c.DrainQueue();
            Check(field.Cores==0 && field.Silent && p.Energy==energy && c.Session.PublicRepairEnergy==4 && c.Tap.TriggerScore==10,
                "exhausted field suppresses active abilities but retains ordinary trigger score");
            c=Context();b=c.Board;p=BoardEntity.Part(1,PartType.TuningFork);b.Place(p,default);
            var zone=new AnomalyRegion(AnomalyType.SilenceZone,new[]{p.Coord});b.RegisterAnomaly(zone);
            var echo=new AnomalyRegion(AnomalyType.EchoZone,new[]{p.Coord});b.RegisterAnomaly(echo);
            c.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock,0,p.Id));c.DrainQueue();
            Check(p.Energy==10 && c.Tap.TriggerScore==2 && echo.EchoRemaining==2 && !c.Log.Any(x=>x.Kind==SettlementEventKind.SoundEmitted),
                "silenced source emits no original sound and spends no echo quota");
            Check(!AnomalyService.IsSilenced(b,p.Coord,true),"default silence does not suppress declared story endpoints");
            AnomalyService.CalibrateAt(b,p.Coord);SoundService.Emit(c,p.Id,p.Coord,HexDirection.D0,4);c.DrainQueue();
            Check(echo.EchoRemaining==1,"delayed actual release after silence ends spends echo quota at release time");
        }
        private static void GravityCyclesAndGuards()
        {
            AnomalyConfig[] configs=Enum.GetValues(typeof(AnomalyType)).Cast<AnomalyType>().Select(AnomalyCatalog.Get).Where(x=>x!=null).ToArray();
            bool rejected=false;
            try { AnomalyCatalog.Install(new[]{Anomaly(AnomalyType.EchoZone,"NoEchoOfEcho=false")}); }
            catch(InvalidOperationException){rejected=true;}
            Check(rejected && AnomalyCatalog.Get(AnomalyType.EchoZone)!=null,"invalid recursive echo config is rejected without replacing current catalog");
            rejected=false;
            try { AnomalyCatalog.Install(new[]{Anomaly(AnomalyType.PostTapReversal,period:0)}); }
            catch(InvalidOperationException){rejected=true;}
            Check(rejected,"required reversal period cannot use non-applicable zero default");
            SettlementContext c=Context();BoardState b=c.Board;
            HexCoord start=HexCoord.Zero, next=start.Neighbor(HexDirection.D1);
            var p=BoardEntity.Part(1,PartType.None);b.Place(p,start);
            b.RegisterAnomaly(new AnomalyRegion(AnomalyType.LocalSettleDeflection,new[]{start}));
            try
            {
                AnomalyCatalog.Install(configs.Where(x=>x.Type!=AnomalyType.LocalSettleDeflection)
                    .Concat(new[]{Anomaly(AnomalyType.LocalSettleDeflection,deflection:4)}));
                b.RegisterAnomaly(new AnomalyRegion(AnomalyType.LocalSettleDeflection,new[]{next}));
                var second=BoardEntity.Part(2,PartType.None);HexCoord other=new HexCoord(-2,1);b.Place(second,other);
                // 第二枚也形成独立环，不能因其它重力移动不断重新开放已走过的环。
                b.RegisterAnomaly(new AnomalyRegion(AnomalyType.LocalSettleDeflection,new[]{other.Neighbor(HexDirection.D1)}));
                AnomalyCatalog.Install(configs);
                b.RegisterAnomaly(new AnomalyRegion(AnomalyType.LocalSettleDeflection,new[]{other}));
                var preview=PreviewService.Compute(b,new SettleState()).ToArray();
                SettlementResult result=new TapSettlement().Settle(b,new SettleState(),new TapContext(2),c.Session);
                Check(result.Stabilized && p.Coord==start && second.Coord==other && preview.Length==0,
                    "two independent local-gravity cycles terminate without extra tap or preview mutation");
            }
            finally { AnomalyCatalog.Install(configs); }
        }
    }
}
