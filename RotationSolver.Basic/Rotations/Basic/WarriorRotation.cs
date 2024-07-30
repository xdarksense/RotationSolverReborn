namespace RotationSolver.Basic.Rotations.Basic;

partial class WarriorRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Strength;


    /// <summary>
    /// 
    /// </summary>
    public static byte BeastGauge => JobGauge.BeastGauge;

    private sealed protected override IBaseAction TankStance => DefiancePvE;

    static partial void ModifyHeavySwingPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyMaimPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.HeavySwingPvE];
    }

    static partial void ModifyBerserkPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasHostilesInRange && !ActionID.InnerReleasePvE.IsCoolingDown();
        setting.StatusProvide = [StatusID.Berserk];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 0,
        };
    }

    static partial void ModifyOverpowerPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyDefiancePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyReleaseDefiancePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyTomahawkPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MeleeRange;
        setting.UnlockedByQuestID = 65852;
    }

    static partial void ModifyStormsPathPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge <= 80;
        setting.ComboIds = [ActionID.MaimPvE];
    }

    static partial void ModifyThrillOfBattlePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ThrillOfBattle];
        setting.UnlockedByQuestID = 65855;
    }

    static partial void ModifyInnerBeastPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.UnlockedByQuestID = 66586;
    }

    static partial void ModifyVengeancePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.RampartStatus;
        setting.ActionCheck = Player.IsTargetOnSelf;
    }

    static partial void ModifyMythrilTempestPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.OverpowerPvE];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
        setting.UnlockedByQuestID = 66587;

    }

    static partial void ModifyHolmgangPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Holmgang_409];
        setting.TargetType = TargetType.Self;
        setting.ActionCheck = () => InCombat;
    }

    static partial void ModifySteelCyclonePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
        setting.UnlockedByQuestID = 66589;
    }

    static partial void ModifyStormsEyePvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.MaimPvE];
        setting.StatusProvide = [StatusID.SurgingTempest];
        setting.CreateConfig = () => new ActionConfig()
        {
            StatusGcdCount = 9,
        };
    }

    static partial void ModifyInfuriatePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.NascentChaos];
        setting.ActionCheck = () => HasHostilesInRange && BeastGauge <= 50 && InCombat;
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 0,
        };
        setting.UnlockedByQuestID = 66590;
    }

    static partial void ModifyFellCleavePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.UnlockedByQuestID = 66124;
        setting.StatusProvide = [StatusID.BurgeoningFury];
    }

    static partial void ModifyRawIntuitionPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = Player.IsTargetOnSelf;
        setting.StatusProvide = [StatusID.RawIntuition];
        setting.UnlockedByQuestID = 66132;
    }

    static partial void ModifyEquilibriumPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 66134;
        setting.ActionCheck = Player.IsTargetOnSelf;
        setting.StatusProvide = [StatusID.Equilibrium];
    }

    static partial void ModifyDecimatePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.UnlockedByQuestID = 66137;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyOnslaughtPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyUpheavalPvE(ref ActionSetting setting)
    {
        //TODO: Why is that status? Answer: This is Warrior's 10% damage buff. Don't want to cast Upheaval at the start of combat without the buff. 
        setting.StatusNeed = [StatusID.SurgingTempest];
    }

    static partial void ModifyShakeItOffPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ShakeItOff, StatusID.ShakeItOffOverTime];
    }

    static partial void ModifyInnerReleasePvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 0,
        };
        setting.UnlockedByQuestID = 68440;
        setting.StatusProvide = [StatusID.InnerRelease, StatusID.PrimalRendReady, StatusID.InnerStrength];
    }

    static partial void ModifyChaoticCyclonePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.StatusNeed = [StatusID.NascentChaos];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyNascentFlashPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.NascentFlash];
        setting.TargetStatusProvide = [StatusID.NascentGlint];
        setting.IsFriendly = true;
    }

    static partial void ModifyInnerChaosPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease);
        setting.StatusNeed = [StatusID.NascentChaos];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyBloodwhettingPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.StemTheTide, StatusID.StemTheFlow];
    }

    static partial void ModifyOrogenyPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyPrimalRendPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PrimalRendReady];
        setting.StatusProvide = [StatusID.PrimalRuinationReady];
        setting.SpecialType = SpecialActionType.MovingForward;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyDamnationPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.PrimevalImpulse];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyPrimalWrathPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Wrathful];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    static partial void ModifyPrimalRuinationPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PrimalRuinationReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    // PvP
    static partial void ModifyPrimalRendPvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyOnslaughtPvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyFellCleavePvP(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.InnerRelease_1303];
    }

    static partial void ModifyChaoticCyclonePvP(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.NascentChaos_1992];
    }

    /// <inheritdoc/>
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (HolmgangPvE.CanUse(out act)
            && Player.GetHealthRatio() <= Service.Config.HealthForDyingTanks) return true;
        return base.EmergencyAbility(nextGCD, out act);
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.OnslaughtPvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (OnslaughtPvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.PrimalRendPvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (PrimalRendPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.MoveForwardGCD(out act);
    }
}
