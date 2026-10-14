using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>存档中的单枚实体（P2-010）：种类、格位、能量与运行时状态。</summary>
    public sealed class SavedEntity
    {
        public int Id;
        public EntityKind Kind;
        public PartType PartType;
        public ObstacleType ObstacleType;
        public HexCoord Coord;
        public int Energy;
        public int EnergyCapacity;
        public int Durability;
        public int MaxDurability;
        public bool IsOpen;
        public int PassDirection = -1;
        public int ClampedEntityId;
        public int ClampedByEntityId;
        public bool PendingClose;
        public int ReleaseWindowRemaining;
        public int ReleasedEntityId;
        public int GrowthRootEntityId;
        public int ProtectedElementId;
        public List<int> LinkedElementIds = new List<int>();
        public bool SealActive;
        public int RepairProgress;
        public bool RepairCompleted;
        public int RepairRequired;
        public int RepairEnergyCost;
        public int RepairScore;
        public List<PartType> CompatibleParts = new List<PartType>();
        public bool IsLocked;
        public bool Movable;
        public int AnchorLabel;
        public string FormId;
        public int FuseDueTap;
        public AnomalyType AnomalyType;
        public SavedGoal Goal;
        public int Charge;
        public bool IsCopy;
        public bool ReleaseMode;
        public List<SavedEntity> Stored = new List<SavedEntity>();
        public int DisguiseLayers;
        public bool StabilizesSettling;
        public int WaterLoad;
        public int LoadAmount;
        public bool AcceptsCalibration;
        public bool AcceptsLoad;
        public bool DischargeMode;
        public string SoundEndpoint;
        public MoveSource LastMoveSource;
    }

    public sealed class SavedGoal
    {
        public string Id;
        public GoalKind Kind;
        public int EntityId;
        public int AnchorLabel;
        public int Required;
        public List<int> OrderLabels = new List<int>();
    }

    public sealed class SavedCell
    {
        public HexCoord Coord;
        public TerrainKind Terrain;
        public int EndpointLabel;
        public bool HasTerrain;
        public bool HasEndpoint;
    }

    public sealed class SavedAnomaly
    {
        public int Id;
        public AnomalyType Type;
        public List<HexCoord> Cells = new List<HexCoord>();
        public HexCoord ExitCell;
        public List<HexCoord> ExpansionCells = new List<HexCoord>();
        public bool DeflectionActive;
        public bool Silent;
        public int UseCount;
        public int ExpansionCount;
        public int Heat;
        public int Cores;
        public int TapCounter;
        public int EchoRemaining;
        public int RedrawCursor;
        public int LastRedrawRound = -1;
        public bool RedrawStopped;
        public bool SettlingStopped;
        public bool FreezeNextReversal;
        public List<int> CarrierIds = new List<int>();
    }

    public sealed class SavedRedShoe
    {
        public HexCoord Coord;
        public HexDirection Direction;
        public bool IsBound;
        public int DiversionProgress;
        public int RestraintLoad;
        public bool IsSealed;
        public bool IsRemoved;
        public int RestraintLimit;
        public List<HexCoord> CountedGuides = new List<HexCoord>();
    }

    /// <summary>存档中的本局 Buff（P2-010）。</summary>
    public sealed class SavedBuff
    {
        public string Id;
        public int Stacks;
    }

    /// <summary>
    /// 维修尝试存档（P2-010）：实例/关卡/轮次/拍数/盘面/分数/能量/机械臂/Buff/随机状态。
    /// 纯数据载体（DTO），序列化由表现层或持久层负责。本类型不引用引擎。
    /// </summary>
    public sealed class RepairAttemptSave
    {
        public const int CurrentVersion = 3;
        public int Version = CurrentVersion;
        public int BoardRadius;
        public int NextEntityId;
        public int NextAnomalyId = 1;
        public int LastAnomalyTap = -1;
        public int StructureAccidents;
        public int LastObstacleTap = -1;
        public HexDirection Gravity;
        public uint RandomState;
        public string InstanceId;
        public string LevelId;
        public int RoundIndex;
        public int TapsUsed;
        public int Score;
        public int PublicRepairEnergy;
        public int ArmMoves;
        public int Seed;
        public int RandomConsumed;
        public int TapSerial;
        public List<SavedEntity> Entities = new List<SavedEntity>();
        public List<SavedBuff> Buffs = new List<SavedBuff>();
        public List<SavedCell> Cells = new List<SavedCell>();
        public List<SavedAnomaly> Anomalies = new List<SavedAnomaly>();
        public List<int> GoalProgress = new List<int>();
        public SavedRedShoe RedShoe;
    }
}
