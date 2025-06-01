using RotationSolver.Basic.Rotations.Duties;

namespace RebornRotations.Duty;

[Rotation("Phantom Jobs Loaded", CombatType.PvE)]

public sealed class PhantomDefault : PhantomRotation
{
    public override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (CleansingPvE.CanUse(out act))
        {
            return true;
        }

        if (StarfallPvE.CanUse(out act))
        {
            return true;
        }

        if (VigilancePvE.CanUse(out act))
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    public override bool InterruptAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (OccultFalconPvE.CanUse(out act))
        {
            return true;
        }

        if (CleansingPvE.CanUse(out act))
        {
            return true;
        }

        return base.InterruptAbility(nextGCD, out act);
    }

    public override bool DispelAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (RecuperationPvE.CanUse(out act))
        {
            return true;
        }

        return base.DispelAbility(nextGCD, out act);
    }

    public override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (InCombat && OffensiveAriaPvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && HerosRimePvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && PhantomAimPvE.CanUse(out act))
        {
            return true;
        }

        if (!IsMoving && OccultSprintPvE.CanUse(out act))
        {
            return true;
        }

        if (!InCombat && SuspendPvE.CanUse(out act))
        {
            return true;
        }

        if (RingingRespitePvE.CanUse(out act))
        {
            return true;
        }

        return base.GeneralAbility(nextGCD, out act);
    }

    public override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (PhantomDoomPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultQuickPvE.CanUse(out act))
        {
            return true;
        }

        if (ZeninagePvE.CanUse(out act))
        {
            return true;
        }
        
        if (PhantomKickPvE.CanUse(out act))
        {
            return true;
        }

        //if (OccultCounterPvE.CanUse(out act))
        //{
        //    return true;
        //}

        if (BattleBellPvE.CanUse(out act))
        {
            return true;
        }

        if (StealPvE.CanUse(out act))
        {
            return true;
        }
        return base.AttackAbility(nextGCD, out act);
    }

    public override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (PhantomGuardPvE.CanUse(out act))
        {
            return true;
        }

        if (InvulnerabilityPvE.CanUse(out act))
        {
            return true;
        }

        if (ShirahadoriPvE.CanUse(out act))
        {
            return true;
        }

        if (CounterstancePvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    public override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (MightyMarchPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultUnicornPvE.CanUse(out act))
        {
            return true;
        }

        if (BlessingPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomRejuvenationPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultMageMasherPvE.CanUse(out act))
        {
            return true;
        }

        if (PilferWeaponPvE.CanUse(out act))
        {
            return true;
        }

        if (TrapDetectionPvE.CanUse(out act))
        {
            return true;
        }

        if (ShirahadoriPvE.CanUse(out act))
        {
            return true;
        }

        if (CounterstancePvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    public override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (OccultHealPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomJudgmentPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultChakraPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealSingleAbility(nextGCD, out act);
    }

    public override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (BlessingPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomJudgmentPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealAreaAbility(nextGCD, out act);
    }

    public override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (OccultFeatherfootPvE.CanUse(out act))
        {
            return true;
        }

        return base.MoveForwardAbility(nextGCD, out act);
    }

    public override bool MyInterruptGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (MineuchiPvE.CanUse(out act))
        {
            return true;
        }

        return base.MyInterruptGCD(out act);
    }

    public override bool RaiseGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (RevivePvE.CanUse(out act))
        {
            return true;
        }

        return base.RaiseGCD(out act);
    }

    public override bool DispelGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (OccultDispelPvE.CanUse(out act))
        {
            return true;
        }

        return base.DispelGCD(out act);
    }

    public override bool HealSingleGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (OccultResuscitationPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealSingleGCD(out act);
    }

    public override bool DefenseSingleGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated))
        {
            return false;
        }

        if (PrayPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleGCD(out act);
    }

    public override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.Reawakened, StatusID.Overheated, StatusID.InnerRelease))
        {
            return false;
        }

        if (DeadlyBlowPvE.CanUse(out act, skipComboCheck: true))
        {
            if (BerserkerLevel == 2)
            {
                return true;
            }
            if (BerserkerLevel >= 3 && (!RagePvE.IsEnabled || Player.WillStatusEndGCD(1, 0, true, StatusID.PentupRage) || (RagePvE.Cooldown.IsCoolingDown && !Player.HasStatus(true, StatusID.PentupRage))))
            {
                return true;
            }
        }

        if (PredictPvE.CanUse(out act))
        {
            if (InCombat)
            {
                return true;
            }
        }

        if (SilverCannonPvE.CanUse(out act))
        {
            return true;
        }

        //TODO: If enemy is undead should we prioritize this over SilverCannon?
        //TODO2: Figure out a way to identify target is undead
        if (HolyCannonPvE.CanUse(out act))
        {
            return true;
        }

        // Only one of shock or dark can be used, prioritize Shock
        if (ShockCannonPvE.CanUse(out act))
        {
            return true;
        }

        if (DarkCannonPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomFirePvE.CanUse(out act))
        {
            return true;
        }

        if (OccultCometPvE.CanUse(out act))
        {
            return true;
        }

        if (IainukiPvE.CanUse(out act))
        {
            return true;
        }
        return base.GeneralGCD(out act);
    }
}
