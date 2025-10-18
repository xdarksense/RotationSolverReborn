namespace RotationSolver.RebornRotations.PVPRotations.Tank;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.35")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Tank/DRK_Default.PvP.cs")]

public sealed class DRK_DefaultPvP : DarkKnightRotation
{
    #region Configurations

    [Range(1, 100, ConfigUnitType.Percent, 1)]
    [RotationConfig(CombatType.PvP, Name = "Shadowbringer Threshold")]
    public int ShadowbringerThreshold { get; set; } = 50;

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool RespectGuard { get; set; } = true;
    #endregion

    #region oGCDs
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.EmergencyAbility(nextGCD, out action);
        }

        if (PurifyPvP.CanUse(out action))
        {
            return true;
        }

        if (TheBlackestNightPvP.CanUse(out action) && TheBlackestNightPvP.Cooldown.CurrentCharges == 2 && InCombat)
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out action);
    }

    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.DefenseSingleAbility(nextGCD, out act);
        }

        if (RampartPvP.CanUse(out act))
        {
            return true;
        }

        if (TheBlackestNightPvP.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (RampagePvP.CanUse(out action))
        {
            return true;
        }

        if (FullSwingPvP.CanUse(out action))
        {
            return true;
        }

        if (HasHostilesInRange && SaltedEarthPvP.CanUse(out action))
        {
            return true;
        }

        if (PlungePvP.CanUse(out action))
        {
            return true;
        }

        if (!Player.HasStatus(true, StatusID.Blackblood) && ((Player.GetHealthRatio() * 100) > ShadowbringerThreshold || Player.HasStatus(true, StatusID.DarkArts_3034)) && ShadowbringerPvP.CanUse(out action))
        {
            return true;
        }

        if (SaltAndDarknessPvP.CanUse(out action))
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out action);
    }
    #endregion

    #region GCDs
    protected override bool GeneralGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.GeneralGCD(out action);
        }

        if (DisesteemPvP.CanUse(out action))
        {
            return true;
        }

        if ((Player.GetHealthRatio() * 100) < 60 && ImpalementPvP.CanUse(out action))
        {
            return true;
        }

        if (TorcleaverPvP.CanUse(out action))
        {
            return true;
        }

        if (ComeuppancePvP.CanUse(out action))
        {
            return true;
        }

        if (ScarletDeliriumPvP.CanUse(out action))
        {
            return true;
        }

        if (SouleaterPvP.CanUse(out action))
        {
            return true;
        }

        if (SyphonStrikePvP.CanUse(out action))
        {
            return true;
        }

        if (HardSlashPvP.CanUse(out action))
        {
            return true;
        }

        return base.GeneralGCD(out action);
    }
    #endregion
}