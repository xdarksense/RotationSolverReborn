using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Lumina.Excel.GeneratedSheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Configuration.Conditions;
using RotationSolver.Basic.Rotations.Duties;
using Action = Lumina.Excel.GeneratedSheets.Action;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;

namespace RotationSolver.Basic;

internal static class DataCenter
{
    private static ulong _hostileTargetId = 0;

    public static bool IsActivated() => State || IsManual || Service.Config.TeachingMode;

    internal static IBattleChara? HostileTarget
    {
        get => Svc.Objects.SearchById(_hostileTargetId) as IBattleChara;
        set => _hostileTargetId = value?.GameObjectId ?? 0;
    }

    internal static List<uint> PrioritizedNameIds { get; set; } = new();
    internal static List<uint> BlacklistedNameIds { get; set; } = new();

    internal static Queue<MapEffectData> MapEffects { get; } = new(64);
    internal static Queue<ObjectEffectData> ObjectEffects { get; } = new(64);
    internal static Queue<VfxNewData> VfxNewData { get; } = new(64);

    /// <summary>
    /// This one never be null.
    /// </summary>
    public static MajorConditionSet RightSet
    {
        get
        {
            if (ConditionSets == null || !ConditionSets.Any())
            {
                ConditionSets = [new MajorConditionSet()];
            }

            var index = Service.Config.ActionSequencerIndex;
            if (index < 0 || index >= ConditionSets.Length)
            {
                Service.Config.ActionSequencerIndex = index = 0;
            }

            return ConditionSets[index];
        }
    }

    internal static MajorConditionSet[] ConditionSets { get; set; } = [];

    /// <summary>
    /// Only recorded 15s hps.
    /// </summary>
    public const int HP_RECORD_TIME = 240;

    internal static Queue<(DateTime time, SortedList<ulong, float> hpRatios)> RecordedHP { get; } =
        new(HP_RECORD_TIME + 1);

    public static ICustomRotation? RightNowRotation { get; internal set; }
    public static DutyRotation? RightNowDutyRotation { get; internal set; }

    public static Dictionary<string, DateTime> SystemWarnings { get; set; } = new();

    internal static bool NoPoslock => Svc.Condition[ConditionFlag.OccupiedInEvent]
                                      || !Service.Config.PoslockCasting
                                      //Key cancel.
                                      || Svc.KeyState[Service.Config.PoslockModifier.ToVirtual()]
                                      //Gamepad cancel.
                                      || Svc.GamepadState.Raw(Dalamud.Game.ClientState.GamePad.GamepadButtons.R1) >=
                                      0.5f;

    internal static DateTime EffectTime { private get; set; } = DateTime.Now;
    internal static DateTime EffectEndTime { private get; set; } = DateTime.Now;

    internal const int ATTACKED_TARGETS_COUNT = 48;
    internal static Queue<(ulong id, DateTime time)> AttackedTargets { get; } = new(ATTACKED_TARGETS_COUNT);

    internal static bool InEffectTime => DateTime.Now >= EffectTime && DateTime.Now <= EffectEndTime;
    internal static Dictionary<ulong, uint> HealHP { get; set; } = [];
    internal static Dictionary<ulong, uint> ApplyStatus { get; set; } = [];
    internal static uint MPGain { get; set; }

    internal static bool HasApplyStatus(ulong id, StatusID[] ids)
    {
        if (InEffectTime && ApplyStatus.TryGetValue(id, out var statusId))
        {
            if (ids.Any(s => (ushort)s == statusId)) return true;
        }

        return false;
    }

    public static TerritoryType? Territory { get; set; }

    public static string TerritoryName => Territory?.PlaceName?.Value?.Name?.RawString ?? "Territory";

    public static bool IsPvP => Territory?.IsPvpZone ?? false;

    public static ContentFinderCondition? ContentFinder => Territory?.ContentFinderCondition?.Value;

    public static string ContentFinderName => ContentFinder?.Name?.RawString ?? "Duty";

    public static bool IsInHighEndDuty => ContentFinder?.HighEndDuty ?? false;

    public static ushort TerritoryID => Svc.ClientState.TerritoryType;
    public static bool IsInUCoB => TerritoryID == 733;
    public static bool IsInUwU => TerritoryID == 777;
    public static bool IsInTEA => TerritoryID == 887;
    public static bool IsInDSR => TerritoryID == 968;
    public static bool IsInTOP => TerritoryID == 1122;

    public static TerritoryContentType TerritoryContentType =>
        (TerritoryContentType)(ContentFinder?.ContentType?.Value?.RowId ?? 0);

    public static AutoStatus MergedStatus => AutoStatus | CommandStatus;

    public static AutoStatus AutoStatus { get; set; } = AutoStatus.None;
    public static AutoStatus CommandStatus { get; set; } = AutoStatus.None;

    public static HashSet<uint> DisabledActionSequencer { get; set; } = [];

    private static List<NextAct> NextActs = [];
    public static IAction? ActionSequencerAction { private get; set; }

    public static IAction? CommandNextAction
    {
        get
        {
            var next = NextActs.FirstOrDefault();

            while (next != null && NextActs.Count > 0 &&
                   (next.DeadTime < DateTime.Now || IActionHelper.IsLastAction(true, next.Act)))
            {
                NextActs.RemoveAt(0);
                next = NextActs.FirstOrDefault();
            }

            return next?.Act ?? ActionSequencerAction;
        }
    }

    public static Job Job => Player.Job;

    public static JobRole Role => Service.GetSheet<ClassJob>().GetRow((uint)Job)?.GetJobRole() ?? JobRole.None;

    internal static void AddCommandAction(IAction act, double time)
    {
        var index = NextActs.FindIndex(i => i.Act.ID == act.ID);
        var newItem = new NextAct(act, DateTime.Now.AddSeconds(time));
        if (index < 0)
        {
            NextActs.Add(newItem);
        }
        else
        {
            NextActs[index] = newItem;
        }

        NextActs = [.. NextActs.OrderBy(i => i.DeadTime)];
    }

    public static TargetHostileType RightNowTargetToHostileType => Service.Config.HostileType;
    public static TinctureUseType RightNowTinctureUseType => Service.Config.TinctureType;

    public static unsafe ActionID LastComboAction => (ActionID)ActionManager.Instance()->Combo.Action;
    public static unsafe float ComboTime => ActionManager.Instance()->Combo.Timer;

    public static TargetingType TargetingType
    {
        get
        {
            if (Service.Config.TargetingTypes.Count == 0)
            {
                Service.Config.TargetingTypes.Add(TargetingType.LowHP);
                Service.Config.TargetingTypes.Add(TargetingType.HighHP);
                Service.Config.TargetingTypes.Add(TargetingType.Small);
                Service.Config.TargetingTypes.Add(TargetingType.Big);
                Service.Config.Save();
            }

            return Service.Config.TargetingTypes[Service.Config.TargetingIndex % Service.Config.TargetingTypes.Count];
        }
    }

    public static bool IsMoving { get; internal set; }

    internal static float StopMovingRaw { get; set; }

    public static unsafe ushort FateId
    {
        get
        {
            try
            {
                if (Service.Config.ChangeTargetForFate && (IntPtr)FateManager.Instance() != IntPtr.Zero
                                                       && (IntPtr)FateManager.Instance()->CurrentFate != IntPtr.Zero
                                                       && Player.Level <= FateManager.Instance()->CurrentFate->MaxLevel)
                {
                    return FateManager.Instance()->CurrentFate->FateId;
                }
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex.StackTrace ?? ex.Message);
            }

            return 0;
        }
    }

    #region GCD
    // Returns the time remaining until the next GCD (Global Cooldown) after considering the current animation lock.
    public static float NextAbilityToNextGCD => DefaultGCDRemain - Math.Max(ActionManagerHelper.GetCurrentAnimationLock(), DataCenter.MinAnimationLock);

    // Returns the total duration of the default GCD.
    public static float DefaultGCDTotal => ActionManagerHelper.GetDefaultRecastTime();

    // Returns the remaining time for the default GCD by subtracting the elapsed time from the total recast time.
    public static float DefaultGCDRemain =>
        ActionManagerHelper.GetDefaultRecastTime() - ActionManagerHelper.GetDefaultRecastTimeElapsed();

    // Returns the elapsed time since the start of the default GCD.
    public static float DefaultGCDElapsed => ActionManagerHelper.GetDefaultRecastTimeElapsed();

    // Returns the action ahead time, which can be overridden by a configuration setting.
    public static float ActionAhead =>
        Service.Config.OverrideActionAheadTimer ? Service.Config.Action4Head : CalculatedActionAhead;

    // Returns the calculated action ahead time as 25% of the total GCD time.
    public static float CalculatedActionAhead => DefaultGCDTotal * 0.25f;

    // Calculates the total GCD time for a given number of GCDs and an optional offset.
    public static float GCDTime(uint gcdCount = 0, float offset = 0)
        => ActionManagerHelper.GetDefaultRecastTime() * gcdCount + offset;

    public static bool LastAbilityorNot => DataCenter.InCombat && (DataCenter.NextAbilityToNextGCD <= Math.Max(ActionManagerHelper.GetCurrentAnimationLock(), DataCenter.MinAnimationLock) + Service.Config.isLastAbilityTimer);
    public static bool FirstAbilityorNot => DataCenter.InCombat && (DataCenter.NextAbilityToNextGCD >= Math.Max(ActionManagerHelper.GetCurrentAnimationLock(), DataCenter.MinAnimationLock) + Service.Config.isFirstAbilityTimer);
    #endregion

    public static uint[] BluSlots { get; internal set; } = new uint[24];

    public static uint[] DutyActions { get; internal set; } = new uint[2];

    static DateTime _specialStateStartTime = DateTime.MinValue;
    private static double SpecialTimeElapsed => (DateTime.Now - _specialStateStartTime).TotalSeconds;
    public static double SpecialTimeLeft => Service.Config.SpecialDuration - SpecialTimeElapsed;

    static SpecialCommandType _specialType = SpecialCommandType.EndSpecial;

    internal static SpecialCommandType SpecialType
    {
        get => SpecialTimeLeft < 0 ? SpecialCommandType.EndSpecial : _specialType;
        set
        {
            _specialType = value;
            _specialStateStartTime = value == SpecialCommandType.EndSpecial ? DateTime.MinValue : DateTime.Now;
        }
    }

    public static bool State { get; set; } = false;

    public static bool IsManual { get; set; } = false;

    public static bool InCombat { get; set; }

    static RandomDelay _notInCombatDelay = new(() => Service.Config.NotInCombatDelay);

    /// <summary>
    /// Is out of combat.
    /// </summary>
    public static bool NotInCombatDelay => _notInCombatDelay.Delay(!InCombat);

    internal static float CombatTimeRaw { get; set; }
    private static DateTime _startRaidTime = DateTime.MinValue;

    internal static float RaidTimeRaw
    {
        get
        {
            // If the raid start time is not set, return 0.
            if (_startRaidTime == DateTime.MinValue) return 0;

            // Calculate and return the total seconds elapsed since the raid started.
            return (float)(DateTime.Now - _startRaidTime).TotalSeconds;
        }
        set
        {
            // If the provided value is negative, reset the raid start time.
            if (value < 0)
            {
                _startRaidTime = DateTime.MinValue;
            }
            else
            {
                // Set the raid start time to the current time minus the provided value in seconds.
                _startRaidTime = DateTime.Now - TimeSpan.FromSeconds(value);
            }
        }
    }

    public unsafe static IBattleChara[] PartyMembers => AllianceMembers.Where(ObjectHelper.IsParty)
        .Where(b => b.Character()->CharacterData.OnlineStatus != 15 && b.IsTargetable).ToArray();

    public unsafe static IBattleChara[] AllianceMembers => AllTargets.Where(ObjectHelper.IsAlliance)
        .Where(b => b.Character()->CharacterData.OnlineStatus != 15 && b.IsTargetable).ToArray();

    public static unsafe IBattleChara[] FriendlyNPCMembers
    {
        get
        {
            try
            {
                // Ensure Svc.Objects is not null
                if (Svc.Objects == null)
                {
                    Svc.Log.Error("Svc.Objects is null");
                    return Array.Empty<IBattleChara>();
                }

                // Filter and return friendly NPC members
                return AllTargets.Where(obj => obj.GetNameplateKind() == NameplateKind.FriendlyBattleNPC).Where(b => b.IsTargetable).ToArray();
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Error in get_FriendlyNPCMembers: {ex.Message}");
                return Array.Empty<IBattleChara>();
            }
        }
    }

    public static IBattleChara[] AllHostileTargets
    {
        get
        {
            return AllTargets.Where(b =>
            {
                // Check if the target is an enemy.
                if (!b.IsEnemy()) return false;

                // Check if the target is dead.
                if (b.CurrentHp <= 1) return false;

                // Check if the target is targetable.
                if (!b.IsTargetable) return false;

                // Check if the target is invincible.
                if (b.StatusList.Any(StatusHelper.IsInvincible)) return false;

                // If all checks pass, the target is considered hostile.
                return true;
            }).ToArray();
        }
    }

    public static IBattleChara? InterruptTarget =>
        AllHostileTargets.FirstOrDefault(ObjectHelper.CanInterrupt);

    public static IBattleChara? ProvokeTarget => AllHostileTargets.FirstOrDefault(ObjectHelper.CanProvoke);

    public static IBattleChara? DeathTarget
    {
        get
        {
            // Ensure AllianceMembers and PartyMembers are not null
            if (AllianceMembers == null || PartyMembers == null) return null;

            var deathAll = AllianceMembers.GetDeath();
            var deathParty = PartyMembers.GetDeath();

            if (deathParty.Any())
            {
                var deathT = deathParty.GetJobCategory(JobRole.Tank).ToList();
                var deathH = deathParty.GetJobCategory(JobRole.Healer).ToList();

                if (deathT.Count > 1)
                {
                    return deathT.FirstOrDefault();
                }

                if (deathH.Any()) return deathH.FirstOrDefault();

                if (deathT.Any()) return deathT.FirstOrDefault();

                return deathParty.FirstOrDefault();
            }

            if (deathAll.Any() && Service.Config.RaiseAll)
            {
                var deathAllH = deathAll.GetJobCategory(JobRole.Healer).ToList();
                var deathAllT = deathAll.GetJobCategory(JobRole.Tank).ToList();

                if (deathAllH.Any()) return deathAllH.FirstOrDefault();

                if (deathAllT.Any()) return deathAllT.FirstOrDefault();

                return deathAll.FirstOrDefault();
            }

            return null;
        }
    }

    public static IBattleChara? DispelTarget
    {
        get
        {
            var weakenPeople =
                DataCenter.PartyMembers.Where(o => o is IBattleChara b && b.StatusList.Any(StatusHelper.CanDispel));
            var weakenNPC =
                DataCenter.FriendlyNPCMembers.Where(o => o is IBattleChara b && b.StatusList.Any(StatusHelper.CanDispel));
            var dyingPeople =
                weakenPeople.Where(o => o is IBattleChara b && b.StatusList.Any(StatusHelper.IsDangerous));

            return dyingPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                   ?? weakenPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                   ?? weakenNPC.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault();
        }
    }

    public static IBattleChara[] AllTargets => Svc.Objects.OfType<IBattleChara>().GetObjectInRadius(30)
        .Where(o => !o.IsDummy() || !Service.Config.DisableTargetDummys).ToArray();

    public static ulong[] TreasureCharas
    {
        get
        {
            List<ulong> charas = new(5);
            //60687 - 60691 For treasure hunt.
            for (int i = 60687; i <= 60691; i++)
            {
                var b = AllTargets.FirstOrDefault(obj => obj.GetNamePlateIcon() == i);
                if (b == null || b.CurrentHp == 0) continue;
                charas.Add(b.GameObjectId);
            }

            return charas.ToArray();
        }
    }

    public static bool HasHostilesInRange => NumberOfHostilesInRange > 0;
    public static bool HasHostilesInMaxRange => NumberOfHostilesInMaxRange > 0;
    public static int NumberOfHostilesInRange => AllHostileTargets.Count(o => o.DistanceToPlayer() <= JobRange);
    public static int NumberOfHostilesInMaxRange => AllHostileTargets.Count(o => o.DistanceToPlayer() <= 25);
    public static int NumberOfAllHostilesInRange => AllHostileTargets.Count(o => o.DistanceToPlayer() <= JobRange);
    public static int NumberOfAllHostilesInMaxRange => AllHostileTargets.Count(o => o.DistanceToPlayer() <= 25);

    public static bool MobsTime => AllHostileTargets.Count(o => o.DistanceToPlayer() <= JobRange && o.CanSee())
                                   >= Service.Config.AutoDefenseNumber;

    public static bool AreHostilesCastingKnockback => AllHostileTargets.Any(IsHostileCastingKnockback);

    public static float JobRange
    {
        get
        {
            float radius = 25;
            if (!Player.Available) return radius;

            switch (DataCenter.Role)
            {
                case JobRole.Tank:
                case JobRole.Melee:
                    radius = 3;
                    break;
            }

            return radius;
        }
    }

    public static float AverageTimeToKill
    {
        get
        {
            // Select the time to kill for each hostile target and filter out NaN values.
            var validTimes = AllHostileTargets
                .Select(b => b.GetTimeToKill())
                .Where(v => !float.IsNaN(v))
                .ToList();

            // If there are valid times, return the average; otherwise, return 0.
            return validTimes.Any() ? validTimes.Average() : 0;
        }
    }

    public static bool IsHostileCastingAOE => IsCastingAreaVfx() || AllHostileTargets.Any(IsHostileCastingArea);

    public static bool IsHostileCastingToTank => IsCastingTankVfx() || AllHostileTargets.Any(IsHostileCastingTank);

    private static DateTime _petLastSeen = DateTime.MinValue;

    public static bool HasPet
    {
        get
        {
            var mayPet = AllTargets.OfType<IBattleNpc>().Where(npc => npc.OwnerId == Player.Object.GameObjectId);
            var hasPet = mayPet.Any(npc => npc.BattleNpcKind == BattleNpcSubKind.Pet);
            if (hasPet ||
                Svc.Condition[ConditionFlag.Mounted] ||
                Svc.Condition[ConditionFlag.Mounted2] ||
                Svc.Condition[ConditionFlag.BetweenAreas] ||
                Svc.Condition[ConditionFlag.BetweenAreas51] ||
                Svc.Condition[ConditionFlag.BeingMoved] ||
                Svc.Condition[ConditionFlag.WatchingCutscene] ||
                Svc.Condition[ConditionFlag.OccupiedInEvent] ||
                Svc.Condition[ConditionFlag.Occupied33] ||
                Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                Svc.Condition[ConditionFlag.Jumping61])
            {
                _petLastSeen = DateTime.Now;
                return true;
            }

            if (!hasPet && _petLastSeen.AddSeconds(3) < DateTime.Now)
            {
                return false;
            }

            return true;
        }
    }

    public static unsafe bool HasCompanion => (IntPtr)Player.BattleChara != IntPtr.Zero
                                              && (IntPtr)CharacterManager.Instance()->LookupBuddyByOwnerObject(
                                                  Player.BattleChara) != IntPtr.Zero;

    #region HP

    public static Dictionary<ulong, float> RefinedHP => PartyMembers
        .ToDictionary(p => p.GameObjectId, GetPartyMemberHPRatio);

    private static Dictionary<ulong, uint> _lastHp = [];

    private static float GetPartyMemberHPRatio(IBattleChara member)
    {
        if (member == null) throw new ArgumentNullException(nameof(member));

        if (!DataCenter.InEffectTime || !DataCenter.HealHP.TryGetValue(member.GameObjectId, out var healedHp))
        {
            return (float)member.CurrentHp / member.MaxHp;
        }

        var currentHp = member.CurrentHp;
        if (currentHp > 0)
        {
            if (!_lastHp.TryGetValue(member.GameObjectId, out var lastHp))
            {
                lastHp = currentHp;
            }

            if (currentHp - lastHp == healedHp)
            {
                DataCenter.HealHP.Remove(member.GameObjectId);
                return (float)currentHp / member.MaxHp;
            }

            return Math.Min(1, (healedHp + currentHp) / (float)member.MaxHp);
        }

        return (float)currentHp / member.MaxHp;
    }

    public static IEnumerable<float> PartyMembersHP => RefinedHP.Values.Where(r => r > 0);

    public static float PartyMembersMinHP
    {
        get
        {
            var partyMembersHP = PartyMembersHP.ToList();
            return partyMembersHP.Any() ? partyMembersHP.Min() : 0;
        }
    }

    public static float PartyMembersAverHP
    {
        get
        {
            var partyMembersHP = PartyMembersHP.ToList();
            return partyMembersHP.Any() ? partyMembersHP.Average() : 0;
        }
    }

    public static float PartyMembersDifferHP
    {
        get
        {
            var partyMembersHP = PartyMembersHP.ToList();
            if (!partyMembersHP.Any()) return 0;

            var averageHP = partyMembersHP.Average();
            return (float)Math.Sqrt(partyMembersHP.Average(d => Math.Pow(d - averageHP, 2)));
        }
    }

    public static bool HPNotFull => PartyMembersMinHP < 1;

    public static uint CurrentMp => Math.Min(10000, Player.Object.CurrentMp + MPGain);
    #endregion

    internal static Queue<MacroItem> Macros { get; } = new Queue<MacroItem>();

    #region Action Record
    public const float MinAnimationLock = 0.6f;

    const int QUEUECAPACITY = 32;
    private static readonly Queue<ActionRec> _actions = new(QUEUECAPACITY);
    private static readonly Queue<DamageRec> _damages = new(QUEUECAPACITY);

    public static float DPSTaken
    {
        get
        {
            try
            {
                var recs = _damages.Where(r => DateTime.Now - r.ReceiveTime < TimeSpan.FromMilliseconds(5));

                if (!recs.Any()) return 0;

                var damages = recs.Sum(r => r.Ratio);

                var time = recs.Last().ReceiveTime - recs.First().ReceiveTime + TimeSpan.FromMilliseconds(2.5f);

                return damages / (float)time.TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }
    }

    public static ActionRec[] RecordActions => _actions.Reverse().ToArray();
    private static DateTime _timeLastActionUsed = DateTime.Now;
    public static TimeSpan TimeSinceLastAction => DateTime.Now - _timeLastActionUsed;

    public static ActionID LastAction { get; private set; } = 0;

    public static ActionID LastGCD { get; private set; } = 0;

    public static ActionID LastAbility { get; private set; } = 0;

    internal static unsafe void AddActionRec(Action act)
    {
        if (!Player.Available) return;

        var id = (ActionID)act.RowId;

        //Record
        switch (act.GetActionCate())
        {
            case ActionCate.Spell:
            case ActionCate.Weaponskill:
                LastAction = LastGCD = id;
                break;
            case ActionCate.Ability:
                LastAction = LastAbility = id;
                break;
            default:
                return;
        }

        if (_actions.Count >= QUEUECAPACITY)
        {
            _actions.Dequeue();
        }

        _timeLastActionUsed = DateTime.Now;
        _actions.Enqueue(new ActionRec(_timeLastActionUsed, act));
    }

    internal static void ResetAllRecords()
    {
        LastAction = 0;
        LastGCD = 0;
        LastAbility = 0;
        _timeLastActionUsed = DateTime.Now;
        _actions.Clear();

        MapEffects.Clear();
        ObjectEffects.Clear();
        VfxNewData.Clear();
    }

    internal static void AddDamageRec(float damageRatio)
    {
        if (_damages.Count >= QUEUECAPACITY)
        {
            _damages.Dequeue();
        }

        _damages.Enqueue(new DamageRec(DateTime.Now, damageRatio));
    }

    internal static DateTime KnockbackFinished { get; set; } = DateTime.MinValue;
    internal static DateTime KnockbackStart { get; set; } = DateTime.MinValue;

    #endregion

    internal static SortedList<string, string> AuthorHashes { get; set; } = [];

    private static bool IsCastingTankVfx()
    {
        return IsCastingVfx(s =>
        {
            if (!s.Path.StartsWith("vfx/lockon/eff/tank_lockon")) return false;
            if (!Player.Available) return false;
            if (Player.Object.IsJobCategory(JobRole.Tank) && s.ObjectId != Player.Object.GameObjectId) return false;

            return true;
        });
    }

    private static bool IsCastingAreaVfx()
    {
        return IsCastingVfx(s => s.Path.StartsWith("vfx/lockon/eff/coshare"));
    }

    private static bool IsCastingVfx(Func<VfxNewData, bool> isVfx)
    {
        if (isVfx == null) return false;
        if (DataCenter.VfxNewData == null) return false;

        try
        {
            foreach (var item in DataCenter.VfxNewData.Reverse())
            {
                if (item.TimeDuration.TotalSeconds is > 1 and < 5)
                {
                    if (isVfx(item)) return true;
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Exception in IsCastingVfx");
        }

        return false;
    }

    private static bool IsHostileCastingTank(IBattleChara h)
    {
        return IsHostileCastingBase(h, (act) =>
        {
            Svc.Log.Information($"Checking action {act.RowId} for hostile casting tank.");
            Svc.Log.Information($"HostileCastingTank contains {act.RowId}: {OtherConfiguration.HostileCastingTank.Contains(act.RowId)}");
            Svc.Log.Information($"CastTargetObjectId: {h.CastTargetObjectId}, TargetObjectId: {h.TargetObjectId}");
            return OtherConfiguration.HostileCastingTank.Contains(act.RowId)
                   || h.CastTargetObjectId == h.TargetObjectId;
        });
    }

    private static bool IsHostileCastingArea(IBattleChara h)
    {
        return IsHostileCastingBase(h, (act) => { return OtherConfiguration.HostileCastingArea.Contains(act.RowId); });
    }

    public static bool IsHostileCastingKnockback(IBattleChara h)
    {
        return IsHostileCastingBase(h,
            (act) => act != null && OtherConfiguration.HostileCastingKnockback.Contains(act.RowId));
    }

    private static bool IsHostileCastingBase(IBattleChara h, Func<Action, bool> check)
    {
        // Check if h is null
        if (h == null) return false;

        // Check if the hostile character is casting
        if (!h.IsCasting) return false;

        // Check if the cast is interruptible
        if (h.IsCastInterruptible) return false;

        // Calculate the time since the cast started
        var last = h.TotalCastTime - h.CurrentCastTime;
        var t = last - DataCenter.DefaultGCDTotal;

        Svc.Log.Information($"TotalCastTime: {h.TotalCastTime}, CurrentCastTime: {h.CurrentCastTime}, DefaultGCDTotal: {DataCenter.DefaultGCDTotal}, t: {t}");
        // Check if the total cast time is greater than the minimum cast time and if the calculated time is within a valid range
        if (!(h.TotalCastTime > DataCenter.DefaultGCDTotal && t > 0 && t < DataCenter.GCDTime(1))) return false;

        // Get the action sheet
        var actionSheet = Service.GetSheet<Action>();
        if (actionSheet == null) return false; // Check if actionSheet is null

        // Get the action being cast
        var action = actionSheet.GetRow(h.CastActionId);
        if (action == null) return false; // Check if action is null

        Svc.Log.Information($"Action ID: {action.RowId}, Action Name: {action.Name}");

        // Invoke the check function on the action and return the result
        return check?.Invoke(action) ?? false; // Check if check is null
    }
}