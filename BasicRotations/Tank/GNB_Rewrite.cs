namespace RebornRotations.Tank;

[Rotation("Testing Build", CombatType.PvE, GameVersion = "7.21")]
[SourceCode(Path = "main/BasicRotations/Tank/GNB_Testing.cs")]
[Api(4)]
public class GNB_Testing : GunbreakerRotation
{
    private static bool InBurstStatus => !Player.WillStatusEnd(0, true, StatusID.NoMercy);

    #region Config Options

    [RotationConfig(CombatType.PvE, Name = "Use tincture in opener (experimental)")]
    public bool UsePots { get; set; } = false;

    #endregion

    #region Countdown Logic

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 0.7 && LightningShotPvE.CanUse(out var act)) return act;
        return base.CountDownAction(remainTime);
    }

    #endregion

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (UsePots && InCombat && CombatElapsedLessGCD(3) && IsLastGCD(true, KeenEdgePvE) && BloodfestPvE.Cooldown.IsCoolingDown && UseBurstMedicine(out act)) return true;

        if (AbdomenTearPvE.CanUse(out act)) return true;
        if (EyeGougePvE.CanUse(out act)) return true;
        if (JugularRipPvE.CanUse(out act)) return true;

        if (DangerZonePvE.CanUse(out act))
        {
            if (!DoubleDownPvE.EnoughLevel)
            {
                if (InBurstStatus || NoMercyPvE.Cooldown.IsCoolingDown) return true;
            }
            else
            {
                if (InBurstStatus && IsLastGCD(ActionID.DoubleDownPvE) && DangerZonePvE.CanUse(out act)) return true;

                if (NoMercyPvE.Cooldown.IsCoolingDown && BloodfestPvE.Cooldown.IsCoolingDown && DangerZonePvE.CanUse(out act)) return true;
            }
        }
        if (BowShockPvE.CanUse(out act))
        {
            if (!JugularRipPvE.EnoughLevel)
            {
                if (InBurstStatus && IsLastGCD(ActionID.GnashingFangPvE)) return true;
            }
            else
            {
                if (InBurstStatus && IsLastGCD(ActionID.GnashingFangPvE) && IsLastAbility(false, JugularRipPvE)) return true;
            }
        }
        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        // AOE No Mercy Logic
        if (!DemonSlaughterPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.DemonSlicePvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!FatedCirclePvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.DemonSlaughterPvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!DoubleDownPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.FatedCirclePvE) && NoMercyPvE.CanUse(out act)) return true;
        if (nextGCD.IsTheSameTo(true, ActionID.DoubleDownPvE) && NoMercyPvE.CanUse(out act)) return true;

        // ST No Mercy Logic
        if (!BrutalShellPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.KeenEdgePvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!SolidBarrelPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.BrutalShellPvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!BurstStrikePvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.SolidBarrelPvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!GnashingFangPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.BurstStrikePvE) && NoMercyPvE.CanUse(out act)) return true;
        if (!GnashingFangPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ActionID.BurstStrikePvE) && NoMercyPvE.CanUse(out act)) return true;
        if (nextGCD.IsTheSameTo(false, ActionID.GnashingFangPvE) && NoMercyPvE.CanUse(out act)) return true;

        if (HypervelocityPvE.CanUse(out act)) return true;
        if (FatedBrandPvE.CanUse(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    #endregion

    #region Additional oGCD Logic
    [RotationDesc(ActionID.HeartOfLightPvE, ActionID.ReprisalPvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (!InBurstStatus && HeartOfLightPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (!InBurstStatus && ReprisalPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.HeartOfStonePvE, ActionID.NebulaPvE, ActionID.RampartPvE, ActionID.CamouflagePvE, ActionID.ReprisalPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        //10
        if (CamouflagePvE.CanUse(out act)) return true;
        //15
        if (HeartOfStonePvE.CanUse(out act)) return true;

        //30
        if ((!RampartPvE.Cooldown.IsCoolingDown || RampartPvE.Cooldown.ElapsedAfter(60)) && NebulaPvE.CanUse(out act)) return true;
        //20
        if (NebulaPvE.Cooldown.IsCoolingDown && NebulaPvE.Cooldown.ElapsedAfter(60) && RampartPvE.CanUse(out act)) return true;

        if (ReprisalPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AuroraPvE)]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (AuroraPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(nextGCD, out act);
    }

    #endregion

    #region GCD Logic

    protected override bool GeneralGCD(out IAction? act)
    {
        // Ensures Reign combo is used at the end of burst/opener
        if (!InGnashingFang && GnashingFangPvE.Cooldown.IsCoolingDown)
        {
            if (LionHeartPvE.CanUse(out act, skipComboCheck: true)) return true;
            if (NobleBloodPvE.CanUse(out act, skipComboCheck: true)) return true;
            if (!InGnashingFang && ReignOfBeastsPvE.CanUse(out act, skipComboCheck: true)) return true;
        }

        if (AmmoComboStep == 1 && DoubleDownPvE.CanUse(out act, skipComboCheck: true)) return true;

        if ((!GnashingFangPvE.EnoughLevel || (AmmoComboStep == 1 && DoubleDownPvE.Cooldown.IsCoolingDown)) 
            && SonicBreakPvE.CanUse(out act, skipComboCheck: true)) return true;

        if (WickedTalonPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (SavageClawPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (NoMercyPvE.Cooldown.IsCoolingDown && GnashingFangPvE.Cooldown.HasOneCharge && GnashingFangPvE.CanUse(out act)) return true;

        if (((IsLastGCD((ActionID)BrutalShellPvE.ID) && IsAmmoCapped) || (IsAmmoCapped && !NoMercyPvE.Cooldown.IsCoolingDown)
            || InBurstStatus) && BurstStrikePvE.CanUse(out act, skipComboCheck: true)) return true;

        // AOE 12
        if (DemonSlaughterPvE.CanUse(out act)) return true;
        if (DemonSlicePvE.CanUse(out act)) return true;

        // ST 123
        if ((!HasReadyToReign || GnashingFangPvE.Cooldown.IsCoolingDown) && SolidBarrelPvE.CanUse(out act)) return true;
        if ((!HasReadyToReign || GnashingFangPvE.Cooldown.IsCoolingDown) && BrutalShellPvE.CanUse(out act)) return true;
        if (KeenEdgePvE.CanUse(out act)) return true;

        // Ranged
        if (LightningShotPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }

    #endregion
}
