﻿namespace RotationSolver.RebornRotations.PVPRotations.Melee;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.35")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Tank/NIN_Default.PvP.cs")]

public sealed class NIN_DefaultPvP : NinjaRotation
{
    #region Configurations

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool RespectGuard { get; set; } = true;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvP, Name = "Player health threshold needed for Bloodbath use")]
    public float BloodBathPvPPercent { get; set; } = 0.75f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvP, Name = "Enemy health threshold needed for Smite use")]
    public float SmitePvPPercent { get; set; } = 0.25f;
    #endregion

    #region oGCDs
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? action)
    {
        if (Player.HasStatus(true, StatusID.Hidden_1316))
        {
            return base.EmergencyAbility(nextGCD, out action);
        }

        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.EmergencyAbility(nextGCD, out action);
        }

        if (PurifyPvP.CanUse(out action))
        {
            return true;
        }

        if (BloodbathPvP.CanUse(out action) && Player.GetHealthRatio() < BloodBathPvPPercent)
        {
            return true;
        }

        if (SwiftPvP.CanUse(out action))
        {
            return true;
        }

        if (SmitePvP.CanUse(out action) && SmitePvP.Target.Target.GetHealthRatio() <= SmitePvPPercent)
        {
            return false;
        }

        return base.EmergencyAbility(nextGCD, out action);
    }

    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? action)
    {
        if (Player.HasStatus(true, StatusID.Hidden_1316))
        {
            return base.DefenseSingleAbility(nextGCD, out action);
        }

        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.DefenseSingleAbility(nextGCD, out action);
        }

        return base.DefenseSingleAbility(nextGCD, out action);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        if (Player.HasStatus(true, StatusID.Hidden_1316))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (DokumoriPvP.CanUse(out action))
        {
            return true;
        }

        if (BunshinPvP.CanUse(out action) && !Player.HasStatus(true, StatusID.ThreeMudra) && HasHostilesInMaxRange)
        {
            return true;
        }

        if (ThreeMudraPvP.CanUse(out action, usedUp: true) && !Player.HasStatus(true, StatusID.ThreeMudra) && HasHostilesInMaxRange)
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out action);
    }

    #endregion

    #region GCDs
    protected override bool GeneralGCD(out IAction? action)
    {
        if (Player.HasStatus(true, StatusID.Hidden_1316))
        {
            return AssassinatePvP.CanUse(out action);
        }
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.GeneralGCD(out action);
        }

        if (ZeshoMeppoPvP.CanUse(out action))
        {
            return true;
        }

        if (Player.GetHealthRatio() < .5)
        {
            if (MeisuiPvP.CanUse(out action))
            {
                return true;
            }
        }

        if (ForkedRaijuPvP.CanUse(out action))
        {
            return true;
        }

        if (FleetingRaijuPvP.CanUse(out action))
        {
            return true;
        }

        if (GokaMekkyakuPvP.CanUse(out action))
        {
            return true;
        }

        if (HyoshoRanryuPvP.CanUse(out action))
        {
            return true;
        }

        if (Player.WillStatusEnd(1, true, StatusID.ThreeMudra) && HutonPvP.CanUse(out action))
        {
            return true;
        }

        if (FumaShurikenPvP.CanUse(out action, usedUp: true))
        {
            return true;
        }

        if (AeolianEdgePvP.CanUse(out action))
        {
            return true;
        }

        if (GustSlashPvP.CanUse(out action))
        {
            return true;
        }

        if (SpinningEdgePvP.CanUse(out action))
        {
            return true;
        }

        return base.GeneralGCD(out action);
    }
    #endregion
}