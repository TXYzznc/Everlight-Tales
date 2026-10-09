using System;
using System.Collections.Generic;
using System.IO;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    public static class FirstCaseValidation
    {
        [MenuItem("Game Framework/EverlightTales/UI/首案接入/准备正式流程验收")]
        public static void Prepare() { FirstCaseRuntimeValidation.PreservePrefs(); UIValidationHarness.Disable(); }
        [MenuItem("Game Framework/EverlightTales/UI/首案接入/运行正式流程验收")]
        public static void Run() => FirstCaseRuntimeValidation.Run();
        [MenuItem("Game Framework/EverlightTales/UI/首案接入/恢复验收存档备份")]
        public static void Restore() => FirstCaseRuntimeValidation.RestorePrefs();
        [MenuItem("Game Framework/EverlightTales/UI/首案接入/验证规则与资源")]
        public static void Validate()
        {
            var lines = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(FormalResourceMigration.CatalogPath);
            foreach (string key in new[] { "玩家", "沈遥", "红舞鞋", "SCR-07-08-dance-night", "标记任务标记", "标记维修对象", "转向轨道" })
                Require(catalog.Get(key) != null, "Sprite " + key, lines);
            var board = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/BoardPage.prefab");
            Require(board.GetComponentsInChildren<FormalPortrait>(true).Length == 2, "Board two portraits", lines);
            var level = RedShoeLevelBuilder.Build(RedShoeLevelConfig.Tutorial());
            level.Level.Session.ArmMoves = level.Config.InitialArmMoves;
            var game = new BoardGame(level.Board, new SettleState(), level.Level.Session, level.Level, level);
            int[] directions = { 5, 1, 2, 1, 5, 0, 5, 5, 5 };
            for (int i = 0; i < directions.Length; i++)
            {
                while ((int)level.RedShoe.Direction != directions[i]) game.RotateRight();
                if (i == 4 || i == 5)
                {
                    level.Board.TryGetEntity(6, out BoardEntity pliers);
                    Require(game.ArmMove(pliers, i == 4 ? new HexCoord(0, 0) : new HexCoord(0, 2)) == ArmMoveResult.Ok, "Legal arm", lines);
                }
                RoundPassResult pass = game.Tap();
                lines.Add("tap=" + (i + 1) + " score=" + game.Session.Score + " guide=" + level.RedShoe.DiversionProgress
                    + " coord=" + level.RedShoe.Coord + " box=" + level.Board.EntityAt(level.Config.RedShoe.BoxCoord)?.RepairProgress + " energy=" + game.Session.PublicRepairEnergy + " pass=" + pass);
                if (game.Level.Round.TapQuotaRemaining == 0)
                {
                    Require(pass == RoundPassResult.Passed, "Round " + (game.Level.RoundIndex + 1), lines);
                    game.Level.AdvanceRound();
                    if (i == 2) { var buffs = new BuffSet(); buffs.Add(BuffCatalog.Get("BF-014")); game.Bonuses = BuffEffectService.Resolve(buffs); }
                }
            }
            Require(RedShoeRoundEvaluator.IsLevelComplete(level), "Full legal first case", lines);
            Directory.CreateDirectory("Library/FirstCaseValidation"); File.WriteAllLines("Library/FirstCaseValidation/rules.txt", lines);
            Debug.Log("[FirstCase][Rules] " + string.Join("; ", lines));
        }

        private static void Require(bool condition, string name, List<string> lines)
        {
            if (!condition) throw new InvalidOperationException("[FirstCase][Validation] FAIL " + name);
            lines.Add("PASS " + name);
        }
    }
}
