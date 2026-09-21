using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 世界存档服务（P3-012）：Capture 把世界状态快照成纯 DTO，Restore 从 DTO 重建世界状态。
    /// 地点从 PlaceCatalog 重建（解锁集合 + 开局常驻），案件/任务存足够重构配置的状态字段。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class WorldSaveService
    {
        public static WorldSave Capture(WorldState world)
        {
            var save = new WorldSave
            {
                Day = world.Day,
                Period = world.Period,
                RepairFee = world.RepairFee,
                BatchNumber = world.BatchNumber,
                TutorialComplete = world.TutorialComplete,
                HomeState = world.HomeState,
            };

            foreach (PlaceState place in world.Map.Places)
            {
                save.Places.Add(new PlaceSave { Id = place.Config.Id, Status = place.Status });
            }

            save.OwnedParts.AddRange(world.OwnedParts);

            foreach (MaterialStack s in world.Materials.Stacks)
            {
                save.Materials.Add(new MaterialStack { MaterialId = s.MaterialId, SourceCase = s.SourceCase, Count = s.Count });
            }

            foreach (LedgerEntry e in world.Ledger)
            {
                save.Ledger.Add(new LedgerEntry { Direction = e.Direction, Channel = e.Channel, Amount = e.Amount, Reason = e.Reason, Tick = e.Tick });
            }

            save.Blueprints.AddRange(world.Blueprints);
            save.UnlockedForms.AddRange(world.UnlockedForms);
            foreach (var current in world.CurrentForms)
            {
                if (!string.IsNullOrEmpty(current.Value))
                {
                    save.CurrentForms.Add(new CurrentFormSave { HostPart = current.Key, FormId = current.Value });
                }
            }

            foreach (CaseState c in world.Cases)
            {
                save.Cases.Add(new CaseSave
                {
                    Id = c.Config.Id,
                    Name = c.Config.Name,
                    Batch = c.Config.Batch,
                    TotalStages = c.Config.TotalStages,
                    Kind = c.Kind,
                    CurrentStage = c.CurrentStage,
                    BatchCounted = c.BatchCounted,
                    RewardDelivered = c.RewardDelivered,
                });
            }

            foreach (TaskState t in world.Tasks)
            {
                save.Tasks.Add(new TaskSave
                {
                    Id = t.Config.Id,
                    Name = t.Config.Name,
                    RewardFee = t.Config.RewardFee,
                    IsMain = t.Config.IsMain,
                    TotalSteps = t.TotalSteps,
                    Kind = t.Kind,
                    CurrentStep = t.CurrentStep,
                    LastUpdated = t.LastUpdated,
                });
            }

            return save;
        }

        public static WorldState Restore(WorldSave save)
        {
            var world = new WorldState(save.Day, save.Period)
            {
                RepairFee = save.RepairFee,
                BatchNumber = save.BatchNumber,
                TutorialComplete = save.TutorialComplete,
                HomeState = save.HomeState,
            };

            world.OwnedParts.AddRange(save.OwnedParts);

            foreach (MaterialStack s in save.Materials)
            {
                world.Materials.Add(s.MaterialId, s.Count, s.SourceCase);
            }

            foreach (LedgerEntry e in save.Ledger)
            {
                world.Ledger.Add(new LedgerEntry { Direction = e.Direction, Channel = e.Channel, Amount = e.Amount, Reason = e.Reason, Tick = e.Tick });
            }

            world.Blueprints.AddRange(save.Blueprints);
            world.UnlockedForms.AddRange(save.UnlockedForms);
            foreach (CurrentFormSave current in save.CurrentForms)
            {
                world.CurrentForms[current.HostPart] = current.FormId;
            }

            foreach (PlaceConfig config in PlaceCatalog.FirstBatch())
            {
                PlaceNodeStatus status = FindSavedStatus(save, config.Id);
                if (status == PlaceNodeStatus.Undiscovered && config.UnlockSource == PlaceUnlockSource.Start)
                {
                    status = PlaceNodeStatus.Unlocked;
                }

                world.Map.Register(config, status);
            }

            foreach (CaseSave c in save.Cases)
            {
                var state = new CaseState(new CaseConfig(c.Id, c.Name, c.Batch, c.TotalStages))
                {
                    Kind = c.Kind,
                    CurrentStage = c.CurrentStage,
                    BatchCounted = c.BatchCounted,
                    RewardDelivered = c.RewardDelivered,
                };
                world.Cases.Add(state);
            }

            foreach (TaskSave t in save.Tasks)
            {
                var state = new TaskState(new TaskConfig(t.Id, t.Name, t.RewardFee, null, t.IsMain, "", "", t.TotalSteps))
                {
                    Kind = t.Kind,
                    CurrentStep = t.CurrentStep,
                    LastUpdated = t.LastUpdated,
                };
                world.Tasks.Add(state);
            }

            return world;
        }

        private static PlaceNodeStatus FindSavedStatus(WorldSave save, string placeId)
        {
            foreach (PlaceSave place in save.Places)
            {
                if (place.Id == placeId)
                {
                    return place.Status;
                }
            }

            return PlaceNodeStatus.Undiscovered;
        }
    }
}
