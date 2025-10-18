﻿namespace RotationSolver.RebornRotations.PVPRotations.Melee;

[Rotation("Default", CombatType.PvP, GameVersion = "7.35")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Melee/MNK_Default.PVP.cs")]

public sealed class MNK_DefaultPvP : MonkRotation
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
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.EmergencyAbility(nextGCD, out action);
        }

        if (PurifyPvP.CanUse(out action))
        {
            return true;
        }

        if (RiddleOfEarthPvP.CanUse(out action) && InCombat && Player.GetHealthRatio() < 0.8)
        {
            return true;
        }

        if (EarthsReplyPvP.CanUse(out action))
        {
            if (Player.HasStatus(true, StatusID.EarthResonance) && Player.WillStatusEnd(1, true, StatusID.EarthResonance))
            {
                if (Player.GetHealthRatio() < 0.5 || Player.WillStatusEnd(1, true, StatusID.EarthResonance))
                {
                    return true;
                }
            }
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

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.AttackAbility(nextGCD, out action);
        }

        if (NumberOfHostilesInRangeOf(6) > 0 && RisingPhoenixPvP.CanUse(out action, usedUp: true) && InCombat)
        {
            return true;
        }

        if (EarthsReplyPvP.CanUse(out action, usedUp: true) && HasHostilesInRange)
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out action);
    }

    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.MoveForwardAbility(nextGCD, out action);
        }

        return base.MoveForwardAbility(nextGCD, out action);
    }
    #endregion

    #region GCDs
    protected override bool GeneralGCD(out IAction? action)
    {
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return base.GeneralGCD(out action);
        }

        if (PhantomRushPvP.CanUse(out action))
        {
            return true;
        }

        if (FiresReplyPvP.CanUse(out action))
        {
            return true;
        }

        if (WindsReplyPvP.CanUse(out action))
        {
            return true;
        }

        if (FlintsReplyPvP.CanUse(out action, usedUp: true))
        {
            return true;
        }

        if (PouncingCoeurlPvP.CanUse(out action))
        {
            return true;
        }

        if (RisingRaptorPvP.CanUse(out action))
        {
            return true;
        }

        if (LeapingOpoPvP.CanUse(out action))
        {
            return true;
        }

        if (DemolishPvP.CanUse(out action))
        {
            return true;
        }

        if (TwinSnakesPvP.CanUse(out action))
        {
            return true;
        }

        if (DragonKickPvP.CanUse(out action))
        {
            return true;
        }

        return base.GeneralGCD(out action);
    }
    #endregion
}