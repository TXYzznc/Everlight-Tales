using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    public static class ChallengeAssemblyValidation
    {
        private static readonly List<string> Lines = new List<string>();
        [MenuItem("Game Framework/EverlightTales/Board/验证挑战配置装配")]
        public static void Run()
        {
            if (!Application.isPlaying || !GameConfigRuntime.EnsureLoaded()) throw new InvalidOperationException("从Launch进入Play Mode并等待配置加载。");
            Lines.Clear();
            Directory.CreateDirectory("Library/ChallengeAssemblyValidation");
            try { Validate(); Lines.Add("PASS ALL"); }
            catch(Exception e) { Lines.Add("FAIL " + e); throw; }
            finally
            {
                Directory.CreateDirectory("Library/ChallengeAssemblyValidation");
                File.WriteAllLines("Library/ChallengeAssemblyValidation/runtime.txt",Lines);
                Debug.Log("[ChallengeAssembly] " + Lines.Last());
            }
        }
        private static void Check(bool value,string label)
        { if(!value)throw new InvalidOperationException(label);Lines.Add("PASS "+label); }
        private static ChallengeConfig Copy(ChallengeConfig c,string level=null,string rule=null,string extra=null,string caseKey=null,string stage=null) =>
            new ChallengeConfig(c.Key,caseKey??c.CaseKey,c.Name,level??c.LevelKey,c.Kind,rule??c.RuleId,stage??c.Stage,c.GoalText,
                ExtraParamSet.Parse(extra??"","挑战 "+c.Key));

        private static void Validate()
        {
            GameConfigSnapshot snapshot=GameConfigRuntime.Current;
            ChallengeConfig original=ChallengeCatalog.Get("C-001");
            var levels=LevelPresetCatalog.All;var shoes=RedShoePresetCatalog.All;
            IReadOnlyList<ChallengeAssembly> Prepare(IReadOnlyList<ChallengeConfig> challenges=null,IReadOnlyList<CaseConfig> cases=null,
                IReadOnlyList<ReleaseEntry> releases=null,IReadOnlyList<LevelPreset> candidateLevels=null,IReadOnlyList<RedShoePreset> candidateShoes=null) =>
                ChallengeAssemblyCatalog.Prepare(challenges??snapshot.Challenges,cases??snapshot.Cases,releases??snapshot.Releases,
                    candidateLevels??levels,candidateShoes??shoes);
            ChallengeConfig[] Replace(ChallengeConfig c) => snapshot.Challenges.Select(x=>x.Key==c.Key?c:x).ToArray();
            void Reject(Action action,string key,string label)
            {
                bool rejected=false;
                try{action();}catch(Exception e){rejected=e.Message.Contains(key);}
                Check(rejected,label+" (diagnostic "+key+")");
            }
            var assembled=Prepare();
            Check(assembled.Count==1 && assembled[0].Challenge==original && assembled[0].Level.Key==original.LevelKey,
                "only C001 assembles through explicit challenge level and rule binding");
            Check(snapshot.Challenges.Count==15 && snapshot.Challenges.Skip(1).All(x=>!x.HasLevel && ChallengeAssemblyCatalog.GetByCase(x.CaseKey)==null),
                "C002 through C015 remain metadata without runtime or playable entry");
            Reject(()=>ChallengeAssemblyCatalog.RequireCase(snapshot.Challenges[1].CaseKey),snapshot.Challenges[1].CaseKey,
                "direct request for deferred case is rejected without default redshoe fallback");
            Reject(()=>Prepare(Replace(Copy(original,rule:"CR-UNKNOWN"))),"C-001","unknown enabled rule is rejected before runtime creation");
            Reject(()=>Prepare(Replace(Copy(original,level:"MISSING"))),"C-001","missing enabled level is rejected");
            Reject(()=>Prepare(Replace(Copy(original,level:"",extra:"Enabled=true"))),"C-001","explicit enabled challenge requires a level");
            Reject(()=>Prepare(Replace(Copy(original,extra:"Enabled=wrong"))),"C-001","invalid boolean ExtraData is rejected");
            Reject(()=>Prepare(Replace(Copy(original,extra:"Enabeld=true"))),"C-001","unknown ExtraData key is rejected");
            Reject(()=>Copy(original,extra:"Enabled=true;Enabled=false"),"C-001","duplicate ExtraData key is rejected");
            Reject(()=>Copy(original,extra:"Enabled"),"C-001","malformed ExtraData syntax is rejected");
            Reject(()=>Prepare(snapshot.Challenges.Concat(new[]{original}).ToArray()),"C-001","duplicate challenge key is rejected");
            ChallengeConfig second=Copy(snapshot.Challenges[1],caseKey:original.CaseKey);
            Reject(()=>Prepare(Replace(second)),"C-002","duplicate case binding is rejected even for deferred metadata");
            Reject(()=>Prepare(cases:Array.Empty<CaseConfig>()),"C-001","enabled challenge needs configured case");
            Reject(()=>Prepare(releases:snapshot.Releases.Where(x=>x.Id!=original.Key).ToArray()),"C-001","missing challenge release is rejected");
            var wrongRelease=snapshot.Releases.Select(x=>x.Id==original.Key?new ReleaseEntry(x.Id,ReleaseKind.Part,x.Stage,"",""):x).ToArray();
            Reject(()=>Prepare(releases:wrongRelease),"C-001","release kind cannot impersonate challenge");
            Reject(()=>Prepare(Replace(Copy(original,stage:"S2"))),"C-001","challenge stage must match release stage");
            Reject(()=>Prepare(candidateShoes:Array.Empty<RedShoePreset>()),"C-001","missing specialized rule parameters cannot fall back to defaults");
            RedShoePreset shoe=shoes.Single(x=>x.LevelKey==original.LevelKey);
            var invalidShoe=new RedShoePreset(shoe.LevelKey,shoe.StartCoord,shoe.StartDirection,shoe.GuideCells,shoe.TurnCells,
                shoe.BoxCoord,shoe.BoxRequired,shoe.EntryCoord,shoe.RequiredDiversion,shoe.RestraintLimit,0);
            Reject(()=>Prepare(candidateShoes:new[]{invalidShoe}),"C-001","required StepsPerTap zero is rejected");
            Check(Prepare(Replace(Copy(original,extra:"Enabled=false"))).Count==0,"explicit disabling closes configured challenge without exposing future metadata");
            Check(ChallengeAssemblyCatalog.RequireCase(original.CaseKey)==assembled[0]
                || ChallengeAssemblyCatalog.RequireCase(original.CaseKey).Challenge==original,
                "rejected candidate validation never replaces active assembly catalog");
            RedShoeLevel a=assembled[0].CreateRuntime(),b=assembled[0].CreateRuntime();
            BoardEntity first=a.Board.Entities.First(x=>x.PartType!=PartType.None);int before=b.Board.EntityAt(first.Coord).Energy;
            first.SetEnergy(0);a.RedShoe.DiversionProgress=3;a.Level.Session.Score=100;
            Check(b.Board.EntityAt(first.Coord).Energy==before && b.RedShoe.DiversionProgress==0 && b.Level.Session.Score==0,
                "two assembled instances isolate entities special state score and sessions");
            if(a.Config.RedShoe.TurnCells.Count>0)
            {
                HexDirection originalDirection=shoe.TurnCells[0].Direction;
                a.Config.RedShoe.TurnCells[0].Direction=HexDirections.Rotate(originalDirection,1);
                Check(b.Config.RedShoe.TurnCells[0].Direction==originalDirection && shoe.TurnCells[0].Direction==originalDirection,
                    "mutable turn DTOs are copied per instance and cannot modify catalog or sibling");
            }
            Check(b.Config.RedShoe.StepsPerTap==1 && b.Config.Rounds.Select(x=>x.TargetScore).SequenceEqual(new[]{30,100,170})
                && b.Config.Rounds.All(x=>x.TapCount==3),"current effective one-step three-round baseline remains unchanged");
            var helper=new NewtonsoftJsonHelper();
            File.WriteAllText("Library/ChallengeAssemblyValidation/baseline.json",helper.ToJson(new {
                original.Key,original.CaseKey,original.LevelKey,original.RuleId,original.Stage,
                b.Config.RedShoe.StartCoord,b.Config.RedShoe.StartDirection,b.Config.RedShoe.BoxCoord,b.Config.RedShoe.BoxRequired,
                b.Config.RedShoe.EntryCoord,b.Config.RedShoe.RequiredDiversion,b.Config.RedShoe.RestraintLimit,b.Config.RedShoe.StepsPerTap,
                Guides=b.Config.RedShoe.GuideCells,Turns=b.Config.RedShoe.TurnCells,Rounds=b.Config.Rounds,
                Source="ChallengeTable + StageReleaseTable + Level/Round/Goal/FixedElement/KeyPiece + RedShoe/Guide/Turn; Calibration does not overwrite runtime"
            }));
        }
    }
}
