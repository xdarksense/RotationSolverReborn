namespace RebornRotations.PVPRotations.Tank;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.15", Description = "Beta Rotation")]
[SourceCode(Path = "main/BasicRotations/PVPRotations/Tank/GNB_Default.PvP.cs")]
[Api(4)]
public sealed class GNB_DefaultPvP : GunbreakerRotation
{
    [RotationConfig(CombatType.PvP, Name = "Sprint")]
    public bool UseSprintPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Recuperate")]
    public bool UseRecuperatePvP { get; set; } = false;

    [Range(1, 100, ConfigUnitType.Percent, 1)]
    [RotationConfig(CombatType.PvP, Name = "RecuperateHP%%?")]
    public int RCValue { get; set; } = 75;

    [RotationConfig(CombatType.PvP, Name = "Use Purify")]
    public bool UsePurifyPvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Stun")]
    public bool Use1343PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on DeepFreeze")]
    public bool Use3219PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on HalfAsleep")]
    public bool Use3022PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Sleep")]
    public bool Use1348PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Bind")]
    public bool Use1345PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Heavy")]
    public bool Use1344PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Use Purify on Silence")]
    public bool Use1347PvP { get; set; } = false;

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool GuardCancel { get; set; } = false;

    private bool TryPurify(out IAction? action)
    {
        action = null;
        if (!UsePurifyPvP) return false;

        var purifyStatuses = new Dictionary<int, bool>
        {
            { 1343, Use1343PvP },
            { 3219, Use3219PvP },
            { 3022, Use3022PvP },
            { 1348, Use1348PvP },
            { 1345, Use1345PvP },
            { 1344, Use1344PvP },
            { 1347, Use1347PvP }
        };

        foreach (var status in purifyStatuses)
        {
            if (status.Value && Player.HasStatus(true, (StatusID)status.Key))
            {
                return PurifyPvP.CanUse(out action);
            }
        }

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;
        if (TryPurify(out act)) return true;
        if (UseRecuperatePvP && Player.CurrentHp / Player.MaxHp * 100 < RCValue && RecuperatePvP.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (HeartOfCorundumPvP.CanUse(out act)) return true;

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        if (HypervelocityPvP.CanUse(out act)) return true;
        if (JugularRipPvP.CanUse(out act)) return true;
        if (AbdomenTearPvP.CanUse(out act)) return true;
        if (EyeGougePvP.CanUse(out act)) return true;
        if (FatedBrandPvP.CanUse(out act)) return true;

        if (BlastingZonePvP.CanUse(out act)) return true;
        if (RoughDividePvP.CanUse(out act, usedUp: true)) return true;

        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;

        return base.GeneralAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        act = null;
        // Early exits for Guard status or Sprint usage
        if (GuardCancel && Player.HasStatus(true, StatusID.Guard)) return false;
        if (!Player.HasStatus(true, StatusID.Guard) && UseSprintPvP && !Player.HasStatus(true, StatusID.Sprint) && !InCombat && SprintPvP.CanUse(out act)) return true;

        if (!JugularRipPvPReady && !AbdomenTearPvPReady && !EyeGougePvPReady && !FatedBrandPvPReady && !HypervelocityPvPReady
            && FatedCirclePvP.CanUse(out act)) return true;

        if (WickedTalonPvP.CanUse(out act)) return true;
        if (SavageClawPvP.CanUse(out act)) return true;
        if (GnashingFangPvP.CanUse(out act, usedUp: true)) return true;

        if (!SavageClawPvPReady && !WickedTalonPvPReady)
        {
            if (BurstStrikePvP.CanUse(out act)) return true;
            if (SolidBarrelPvP.CanUse(out act)) return true;
            if (BrutalShellPvP.CanUse(out act)) return true;
            if (KeenEdgePvP.CanUse(out act)) return true;
        }

        return base.GeneralGCD(out act);
    }
}