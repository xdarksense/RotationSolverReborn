﻿namespace RotationSolver.RebornRotations.PVPRotations.Ranged;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.35")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Ranged/BRD_Default.PvP.cs")]

public sealed class BRD_DefaultPvP : BardRotation
{
    #region Configurations

    [RotationConfig(CombatType.PvP, Name = "Use Warden's Paean on other players")]
    public bool BRDEsuna2 { get; set; } = false;

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

        if (BRDEsuna2 && TheWardensPaeanPvP.CanUse(out action))
        {
            return true;
        }
        if (Player.HasStatus(false, StatusHelper.PurifyPvPStatuses))
        {
            if (TheWardensPaeanPvP.CanUse(out action))
            {
                if (TheWardensPaeanPvP.Target.Target == Player)
                {
                    return true;
                }
            }
        }

        if (PurifyPvP.CanUse(out action))
        {
            return true;
        }

        if (BraveryPvP.CanUse(out action))
        {
            if (InCombat)
            {
                return true;
            }
        }

        if (DervishPvP.CanUse(out action))
        {
            if (InCombat)
            {
                return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out action);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (RepellingShotPvP.CanUse(out action))
        {
            if (!Player.HasStatus(true, StatusID.Repertoire))
            {
                return true;
            }
        }

        if (SilentNocturnePvP.CanUse(out action))
        {
            if (!Player.HasStatus(true, StatusID.Repertoire))
            {
                return true;
            }
        }

        if (EagleEyeShotPvP.CanUse(out action))
        {
            return true;
        }

        if (EncoreOfLightPvP.CanUse(out action, skipAoeCheck: true))
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

        if (HarmonicArrowPvP.CanUse(out action))
        {
            return true;
        }

        if (PitchPerfectPvP.CanUse(out action))
        {
            return true;
        }

        if (BlastArrowPvP.CanUse(out action))
        {
            return true;
        }

        if (ApexArrowPvP.CanUse(out action))
        {
            return true;
        }

        if (PowerfulShotPvP.CanUse(out action))
        {
            return true;
        }

        return base.GeneralGCD(out action);
    }
    #endregion
}