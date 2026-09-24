using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 盘面棋子实体。实体是占用层的单元，每格最多一个实体；
    /// 固定设施（IsFixed）锁盘面，不可被移动。位置由 <see cref="BoardState"/> 维护。
    /// 零件（Kind==Part）携带零件种类与能量；维修对象（Kind==RepairTarget）携带维修配置与进度。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class BoardEntity
    {
        /// <summary>局内唯一标识。</summary>
        public int Id { get; }

        /// <summary>实体类别。</summary>
        public EntityKind Kind { get; }

        /// <summary>是否固定：固定设施锁盘面，不可移动。</summary>
        public bool IsFixed { get; }

        /// <summary>是否可移动：非固定且未锁定实体可受定势／能力／机械臂移动。</summary>
        public bool IsMovable => !IsFixed && !IsLocked;

        /// <summary>是否已锁定（门轴联系标记等被普通推移送入校正格后停止移动）。</summary>
        public bool IsLocked { get; private set; }

        /// <summary>当前坐标，由 <see cref="BoardState"/> 维护。</summary>
        public HexCoord Coord { get; internal set; }

        /// <summary>零件种类（Kind==Part）；非零件或惰性零件为 None。</summary>
        public PartType PartType { get; }

        /// <summary>零件能量容量（Kind==Part）。</summary>
        public int EnergyCapacity { get; }

        /// <summary>当前零件能量（Kind==Part，跨拍保留）。</summary>
        public int Energy { get; internal set; }

        /// <summary>本关形态 ID（空串=基础形态；盘面建置时按当前形态写入，D-060）。</summary>
        public string FormId { get; internal set; }

        /// <summary>定时线圈待爆到期拍序号（0=未待爆，>0=到期拍序号，D-060）。</summary>
        public int FuseDueTap { get; internal set; }

        /// <summary>维修对象配置（Kind==RepairTarget）。</summary>
        public RepairTargetConfig RepairConfig { get; }

        /// <summary>维修进度（Kind==RepairTarget，初始 0）。</summary>
        public int RepairProgress { get; internal set; }

        /// <summary>维修是否已完成（达到进度要求后置真）。</summary>
        public bool RepairCompleted { get; internal set; }

        /// <summary>维修进度要求，来自配置。</summary>
        public int RepairRequired => RepairConfig == null ? 0 : RepairConfig.RequiredProgress;

        /// <summary>障碍设施种类（Kind==Facility 且非 None）。</summary>
        public ObstacleType ObstacleType { get; private set; } = ObstacleType.None;

        /// <summary>障碍最大耐久。</summary>
        public int MaxDurability { get; private set; }

        /// <summary>障碍当前耐久（跨拍保留）。</summary>
        public int Durability { get; internal set; }

        /// <summary>闸门是否开启（LinkedGate 初始关）。</summary>
        public bool IsOpen { get; internal set; }

        /// <summary>轨道／百叶的通行方向（相位方向）。</summary>
        public HexDirection PassDirection { get; internal set; }

        /// <summary>夹持座当前锁定的实体 ID（0=未夹持）。</summary>
        public int ClampedEntityId { get; internal set; }

        /// <summary>增生封条根是否仍在生长（O-007，切根后 false）。</summary>
        public bool SealActive { get; internal set; }

        /// <summary>棋盘异常种类（Kind==Facility 且非 None）。</summary>
        public AnomalyType AnomalyType { get; private set; } = AnomalyType.None;

        /// <summary>任务标记的特殊目标配置（Kind==TaskMarker）。</summary>
        public GoalConfig Goal { get; private set; }

        /// <summary>蓄量（P-011 蓄能飞轮，跨拍保留）。</summary>
        public int Charge { get; private set; }

        /// <summary>是否铸模复制体（P-006：复制体不能再作铸模源）。</summary>
        public bool IsCopy { get; private set; }

        /// <summary>增加蓄量（飞轮受击蓄能）。</summary>
        public void AddCharge(int delta)
        {
            Charge += delta;
        }

        /// <summary>清空蓄量（飞轮满蓄释放）。</summary>
        public void ResetCharge()
        {
            Charge = 0;
        }

        /// <summary>标记为铸模复制体。</summary>
        public void SetCopy()
        {
            IsCopy = true;
        }

        /// <summary>胃袋内部存储（P-007，收纳跨拍保留，仅存本局盘面实体引用）。</summary>
        private System.Collections.Generic.List<BoardEntity> _stored = new System.Collections.Generic.List<BoardEntity>();

        /// <summary>胃袋是否处于释放模式（false=收纳，true=释放；准备阶段切换）。</summary>
        public bool ReleaseMode { get; set; }

        /// <summary>胃袋已收纳实体（只读）。</summary>
        public System.Collections.Generic.IReadOnlyList<BoardEntity> Stored => _stored;

        /// <summary>胃袋收纳一个实体（清空其盘面事件，仅移出盘面不销毁）。</summary>
        public void Store(BoardEntity entity)
        {
            if (entity != null)
            {
                _stored.Add(entity);
            }
        }

        /// <summary>胃袋取出最早收纳者（FIFO）。</summary>
        public BoardEntity TakeStored()
        {
            if (_stored.Count == 0)
            {
                return null;
            }

            BoardEntity first = _stored[0];
            _stored.RemoveAt(0);
            return first;
        }

        /// <summary>伪装层层数（P-013 照明揭层目标，0=无伪装）。</summary>
        public int DisguiseLayers { get; private set; }

        /// <summary>设定伪装层层数。</summary>
        public void SetDisguiseLayers(int layers)
        {
            DisguiseLayers = layers < 0 ? 0 : layers;
        }

        /// <summary>揭去一层伪装，返回是否确有伪装可揭。</summary>
        public bool RemoveDisguiseLayer()
        {
            if (DisguiseLayers <= 0)
            {
                return false;
            }

            DisguiseLayers--;
            return true;
        }

        /// <summary>水负荷（P-016 排水叶轮处理目标，带水负荷标签的容器）。</summary>
        public int WaterLoad { get; private set; }

        /// <summary>增加水负荷。</summary>
        public void AddWater(int units)
        {
            WaterLoad += units;
        }

        /// <summary>排掉最多 maxUnits 单位水，返回实际排掉量。</summary>
        public int DrainWater(int maxUnits)
        {
            int drained = WaterLoad < maxUnits ? WaterLoad : maxUnits;
            WaterLoad -= drained;
            return drained;
        }

        /// <summary>是否接受校准（P-018 校准探针目标节点标记）。</summary>
        public bool AcceptsCalibration { get; private set; }

        /// <summary>标记为接受校准的节点。</summary>
        public void SetAcceptsCalibration()
        {
            AcceptsCalibration = true;
        }

        /// <summary>设备负荷量（P-017 缓冲囊，容量 3）。</summary>
        public int LoadAmount { get; private set; }

        /// <summary>增加设备负荷。</summary>
        public void AddLoad(int units)
        {
            LoadAmount += units;
        }

        /// <summary>移走最多 maxUnits 单位负荷，返回实际移走量。</summary>
        public int DrainLoad(int maxUnits)
        {
            int drained = LoadAmount < maxUnits ? LoadAmount : maxUnits;
            LoadAmount -= drained;
            return drained;
        }

        /// <summary>是否兼容负荷处置端（P-017 排放目标）。</summary>
        public bool AcceptsLoad { get; private set; }

        /// <summary>标记为兼容负荷处置端。</summary>
        public void SetAcceptsLoad()
        {
            AcceptsLoad = true;
        }

        /// <summary>缓冲囊是否处于排放模式（false=接收，true=排放；准备阶段切换）。</summary>
        public bool DischargeMode { get; set; }

        /// <summary>声音端点标记（P-015，null=非端点；非空即该端点的标识）。</summary>
        public string SoundEndpoint { get; private set; }

        /// <summary>标记为声音现场端点。</summary>
        public void SetSoundEndpoint(string id)
        {
            SoundEndpoint = id;
        }

        public BoardEntity(
            int id,
            EntityKind kind,
            bool isFixed,
            PartType partType = PartType.None,
            int energy = 0,
            int energyCapacity = 0,
            RepairTargetConfig repairConfig = null)
        {
            Id = id;
            Kind = kind;
            IsFixed = isFixed;
            PartType = partType;
            Energy = energy;
            EnergyCapacity = energyCapacity > 0 ? energyCapacity : energy;
            RepairConfig = repairConfig;
        }

        /// <summary>直接设定零件能量（供充电／恢复／测试使用，钳制到 [0, 容量]）。</summary>
        public void SetEnergy(int value)
        {
            if (EnergyCapacity <= 0)
            {
                Energy = value < 0 ? 0 : value;
                return;
            }

            if (value < 0)
            {
                Energy = 0;
            }
            else if (value > EnergyCapacity)
            {
                Energy = EnergyCapacity;
            }
            else
            {
                Energy = value;
            }
        }

        /// <summary>创建一个可移动零件（按种类初始化能量），可选携带本关形态（D-060）。</summary>
        public static BoardEntity Part(int id, PartType partType, string formId = null)
        {
            if (partType == PartType.None)
            {
                return new BoardEntity(id, EntityKind.Part, false);
            }

            PartConfig config = PartCatalog.Get(partType);
            var entity = new BoardEntity(id, EntityKind.Part, false, partType, config.EnergyCapacity, config.EnergyCapacity);
            entity.FormId = formId;
            return entity;
        }

        /// <summary>设定本关形态（存档恢复 / 盘面临时切换）。</summary>
        public void SetForm(string formId)
        {
            FormId = formId;
        }

        /// <summary>设定定时线圈待爆到期拍（0=解除待爆）。</summary>
        public void SetFuseDueTap(int dueTap)
        {
            FuseDueTap = dueTap;
        }

        /// <summary>创建一个固定设施。</summary>
        public static BoardEntity Facility(int id)
        {
            return new BoardEntity(id, EntityKind.Facility, true);
        }

        /// <summary>创建一个任务标记（默认固定，不接受普通移动），可携带特殊目标配置。</summary>
        public static BoardEntity TaskMarker(int id, GoalConfig goal = null)
        {
            var marker = new BoardEntity(id, EntityKind.TaskMarker, true);
            marker.Goal = goal;
            return marker;
        }

        /// <summary>创建一个可移动任务标记（如门轴联系标记），可携带特殊目标配置；受定势／推移／机械臂影响。</summary>
        public static BoardEntity MovableTaskMarker(int id, GoalConfig goal = null)
        {
            var marker = new BoardEntity(id, EntityKind.TaskMarker, false);
            marker.Goal = goal;
            return marker;
        }

        /// <summary>锁定实体：停止移动（供门轴联系标记被推移进校正格等使用）。</summary>
        public void Lock()
        {
            IsLocked = true;
        }

        /// <summary>恢复锁定状态（存档恢复用）。</summary>
        public void SetLocked(bool locked)
        {
            IsLocked = locked;
        }

        /// <summary>创建一个待维修对象（按配置初始化进度）。</summary>
        public static BoardEntity RepairTarget(int id, RepairTargetConfig config)
        {
            return new BoardEntity(id, EntityKind.RepairTarget, true, repairConfig: config);
        }

        /// <summary>创建一个障碍设施（固定，按配置初始化耐久／方向／闸门状态）。</summary>
        public static BoardEntity Obstacle(int id, ObstacleConfig config)
        {
            var entity = new BoardEntity(id, EntityKind.Facility, true);
            entity.ObstacleType = config.Type;
            entity.MaxDurability = config.MaxDurability;
            entity.Durability = config.MaxDurability;
            if (config.Type == ObstacleType.LinkedGate)
            {
                entity.IsOpen = false; // O-003 初始关闭。
            }

            if (config.Type == ObstacleType.ProliferatingSeal)
            {
                entity.SealActive = true; // O-007 根初始生长中。
            }

            if (config.PassDirection >= 0)
            {
                entity.PassDirection = HexDirections.FromIndex(config.PassDirection);
            }

            return entity;
        }

        /// <summary>创建一个棋盘异常区域锚点（固定，按类型登记）。</summary>
        public static BoardEntity Anomaly(int id, AnomalyType type)
        {
            var entity = new BoardEntity(id, EntityKind.Facility, true);
            entity.AnomalyType = type;
            return entity;
        }

        /// <summary>存档恢复：设定障碍／维修对象的运行时状态（供 Board 层之外的存档恢复入口）。</summary>
        public void RestoreState(int durability, bool isOpen, int passDirectionIndex, int clampedEntityId, bool sealActive, int repairProgress, bool repairCompleted)
        {
            Durability = durability;
            IsOpen = isOpen;
            PassDirection = passDirectionIndex >= 0 ? HexDirections.FromIndex(passDirectionIndex) : HexDirection.D0;
            ClampedEntityId = clampedEntityId;
            SealActive = sealActive;
            RepairProgress = repairProgress;
            RepairCompleted = repairCompleted;
        }

        public override string ToString()
        {
            return "#" + Id + " " + Kind + (PartType == PartType.None ? string.Empty : ":" + PartType) + "@" + Coord;
        }
    }
}
