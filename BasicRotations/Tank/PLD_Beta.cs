namespace DefaultRotations.Tank;

[Rotation("Beta", CombatType.PvE, GameVersion = "7.15")]
[SourceCode(Path = "main/BasicRotations/Tank/PLD_Beta.cs")]
[Api(4)]

public sealed class PLD_Beta : PaladinRotation
{
    #region Config Options

    [RotationConfig(CombatType.PvE, Name = "Prevent actions while you have Passage of Arms up")]
    public bool PassageProtec { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Hallowed Ground with Cover")]
    private bool HallowedWithCover { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use up both stacks of Intervene during burst window")]
    private bool UseInterveneFight { get; set; } = true;

    [Range(0, 100, ConfigUnitType.Pixels)]
    [RotationConfig(CombatType.PvE, Name = "Use Sheltron at minimum X Oath to prevent over cap (Set to 0 to disable)")]
    private int WhenToSheltron { get; set; } = 100;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Health threshold for Intervention (Set to 0 to disable)")]
    private float InterventionRatio { get; set; } = 0.6f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Health threshold for Cover (Set to 0 to disable)")]
    private float CoverRatio { get; set; } = 0.3f;
    #endregion

    private const ActionID ConfiteorPvEActionId = (ActionID)16459;
    private new readonly IBaseAction ConfiteorPvE = new BaseAction(ConfiteorPvEActionId);

    #region Countdown Logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < HolySpiritPvE.Info.CastTime + CountDownAhead
            && HolySpiritPvE.CanUse(out var act)) return act;

        if (remainTime < 15 && DivineVeilPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Additional oGCD Logic

    [RotationDesc(ActionID.CoverPvE)]
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (Player.HasStatus(true, StatusID.Cover) && HallowedWithCover && HallowedGroundPvE.CanUse(out act)) return true;

        if (HallowedGroundPvE.CanUse(out act)
        && Player.GetHealthRatio() <= HealthForDyingTanks) return true;

        if ((Player.HasStatus(true, StatusID.Rampart) || Player.HasStatus(true, StatusID.Sentinel)) &&
            InterventionPvE.CanUse(out act) &&
            InterventionPvE.Target.Target?.GetHealthRatio() < 0.6) return true;

        if (CoverPvE.CanUse(out act) && CoverPvE.Target.Target?.DistanceToPlayer() < 10 &&
            CoverPvE.Target.Target?.GetHealthRatio() < CoverRatio) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (IntervenePvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.DivineVeilPvE, ActionID.PassageOfArmsPvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (DivineVeilPvE.CanUse(out act)) return true;

        if (PassageOfArmsPvE.CanUse(out act)) return true;

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SentinelPvE, ActionID.RampartPvE, ActionID.BulwarkPvE, ActionID.SheltronPvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        // If the player has the Hallowed Ground status, don't use any abilities.
        if (!Player.HasStatus(true, StatusID.HallowedGround))
        {
            // If Bulwark can be used, use it and return true.
            if (BulwarkPvE.CanUse(out act, skipAoeCheck: true)) return true;

            // If Oath can be used, use it and return true.
            if (UseOath(out act)) return true;

            // If Rampart is not cooling down or has been cooling down for more than 60 seconds, and Sentinel can be used, use Sentinel and return true.
            if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && SentinelPvE.CanUse(out act)) return true;

            // If Sentinel is at an enough level and is cooling down for more than 60 seconds, or if Sentinel is not at an enough level, and Rampart can be used, use Rampart and return true.
            if ((SentinelPvE.EnoughLevel && SentinelPvE.Cooldown.IsCoolingDown && SentinelPvE.Cooldown.ElapsedAfter(60) || !SentinelPvE.EnoughLevel) && RampartPvE.CanUse(out act)) return true;

            // If Reprisal can be used, use it and return true.
            if (ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;

        }
        return base.DefenseSingleAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic

    [RotationDesc(ActionID.FightOrFlightPvE)]
    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (InCombat && OathGauge >= WhenToSheltron && WhenToSheltron > 0 && UseOath(out act)) return true;
        return base.GeneralAbility(nextGCD, out act);
    }
    [RotationDesc(ActionID.SpiritsWithinPvE)]
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (BladeOfHonorPvE.CanUse(out act)) return true;

        if (!RiotBladePvE.EnoughLevel && FightOrFlightPvE.CanUse(out act)) return true;
        if (!RageOfHalonePvE.EnoughLevel && nextGCD.IsTheSameTo(true, RiotBladePvE) && FightOrFlightPvE.CanUse(out act)) return true;
        if (!SwordOathTrait.EnoughLevel && nextGCD.IsTheSameTo(true, RoyalAuthorityPvE) && FightOrFlightPvE.CanUse(out act)) return true;
        if (SwordOathTrait.EnoughLevel && Player.HasStatus(true, StatusID.AtonementReady) && FightOrFlightPvE.CanUse(out act)) return true;

        if (IsLastAbility(true, FightOrFlightPvE) && ImperatorPvE.CanUse(out act)) return true;
        if (IsLastAbility(true, FightOrFlightPvE) && RequiescatPvE.CanUse(out act)) return true;
         
        if (FightOrFlightPvE.Cooldown.IsCoolingDown && CircleOfScornPvE.CanUse(out act)) return true;
        if (FightOrFlightPvE.Cooldown.IsCoolingDown && ExpiacionPvE.CanUse(out act)) return true;
        if (FightOrFlightPvE.Cooldown.IsCoolingDown && SpiritsWithinPvE.CanUse(out act)) return true;
        if (!IsMoving && IntervenePvE.CanUse(out act, usedUp: UseInterveneFight && HasFightOrFlight)) return true;
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    [RotationDesc(ActionID.ClemencyPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;
        if (ClemencyPvE.CanUse(out act)) return true;
        return base.HealSingleGCD(out act);
    }

    [RotationDesc(ActionID.ShieldBashPvE)]
    protected override bool MyInterruptGCD(out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        if (LowBlowPvE.Cooldown.IsCoolingDown && ShieldBashPvE.CanUse(out act)) return true;
        return base.MyInterruptGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (PassageProtec && Player.HasStatus(true, StatusID.PassageOfArms)) return false;

        // Confiteor Combo
        if (BladeOfValorPvE.CanUse(out act)) return true;
        if (BladeOfTruthPvE.CanUse(out act)) return true;
        if (BladeOfFaithPvE.CanUse(out act)) return true;
        if (Player.HasStatus(true, StatusID.Requiescat) && ConfiteorPvE.CanUse(out act, usedUp: true)) return true;

        if (GoringBladePvE.CanUse(out act)) return true;

        if (SepulchrePvE.CanUse(out act)) return true;
        if (SupplicationPvE.CanUse(out act)) return true;
        if (AtonementPvE.CanUse(out act)) return true;

        //AoE
        if (Player.HasStatus(true, StatusID.DivineMight) && HolyCirclePvE.CanUse(out act)) return true;
        if (ProminencePvE.CanUse(out act)) return true;
        if (TotalEclipsePvE.CanUse(out act)) return true;

        //Single Target
        if (Player.HasStatus(true, StatusID.DivineMight) && HolySpiritPvE.CanUse(out act)) return true;

        if (RoyalAuthorityPvE.CanUse(out act)) return true;
        if (RageOfHalonePvE.CanUse(out act)) return true;
        if (RiotBladePvE.CanUse(out act)) return true;
        if (FastBladePvE.CanUse(out act)) return true;

        //Ranged
        if (HolySpiritPvE.CanUse(out act)) return true;
        if (ShieldLobPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }

    #endregion

    #region Extra Methods

    private bool UseOath(out IAction? act)
    {
        act = null;
        if ((InterventionPvE.Target.Target?.GetHealthRatio() <= InterventionRatio) && InterventionPvE.CanUse(out act)) return true;
        if (HolySheltronPvE.CanUse(out act)) return true;
        if (SheltronPvE.CanUse(out act)) return true;
        return false;
    }
    #endregion
}
