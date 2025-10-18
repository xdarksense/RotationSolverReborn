﻿namespace RotationSolver.RebornRotations.PVPRotations.Healer;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.35")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Healer/AST_Default.PVP.cs")]

public class AST_DefaultPVP : AstrologianRotation
{
    #region Configurations

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

        if (AspectedBeneficPvP_29247.CanUse(out action, usedUp: true))
        {
            return true;
        }

        if (Player.WillStatusEnd(1, true, StatusID.Macrocosmos_3104) && MicrocosmosPvP.CanUse(out action))
        {
            return true;
        }

        if (Player.WillStatusEnd(1, true, StatusID.LadyOfCrowns_4328) && LadyOfCrownsPvE.CanUse(out action))
        {
            return true;
        }

        if (Player.GetHealthRatio() < 0.5 && MicrocosmosPvP.CanUse(out action))
        {
            return true;
        }

        if (OraclePvP.CanUse(out action))
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out action);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (DiabrosisPvP.CanUse(out action))
        {
            return true;
        }

        if (MinorArcanaPvP.CanUse(out action))
        {
            return true;
        }

        if (LordOfCrownsPvP.CanUse(out action))
        {
            return true;
        }

        if (MacrocosmosPvP.CanUse(out action))
        {
            return true;
        }

        if (GravityIiPvP_29248.CanUse(out action, usedUp: true))
        {
            return true;
        }

        if (FallMaleficPvP_29246.CanUse(out action, usedUp: true))
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out action);
    }
    #endregion

    #region GCDs
    protected override bool DefenseSingleGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.DefenseSingleGCD(out action);
        }

        if (StoneskinIiPvP.CanUse(out action))
        {
            return true;
        }

        return base.DefenseSingleGCD(out action);
    }

    protected override bool HealSingleGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.HealSingleGCD(out action);
        }

        if (HaelanPvP.CanUse(out action))
        {
            return true;
        }

        if (AspectedBeneficPvP.CanUse(out action, usedUp: true))
        {
            return true;
        }

        return base.HealSingleGCD(out action);
    }

    protected override bool HealAreaGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.HealAreaGCD(out action);
        }

        if (LadyOfCrownsPvP.CanUse(out action))
        {
            return true;
        }

        return base.HealAreaGCD(out action);
    }

    protected override bool GeneralGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.GeneralGCD(out action);
        }

        if (GravityIiPvP.CanUse(out action))
        {
            return true;
        }

        if (FallMaleficPvP.CanUse(out action))
        {
            return true;
        }

        return base.GeneralGCD(out action);
    }
    #endregion
}