namespace RebornRotations.Magical;

[Rotation("DOES NOT WORK", CombatType.PvE, GameVersion = "7.2")]
[SourceCode(Path = "main/BasicRotations/Limited Jobs/BLU_Default.cs")]
[Api(4)]
public sealed class Blue_Default : BlueMageRotation
{
    #region Countdown logic

    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        return base.CountDownAction(remainTime);
    }

    #endregion

    #region Emergency Logic

    // Determines emergency actions to take based on the next planned GCD action.
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, AngelWhisperPvE) && SwiftcastPvE.CanUse(out act))
        {
            return true;
        }

        return nextGCD.IsTheSameTo(true, TheRoseOfDestructionPvE) && OffguardPvE.CanUse(out act) || base.EmergencyAbility(nextGCD, out act);
    }

    #endregion

    #region Move oGCD Logic

    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        return base.MoveForwardAbility(nextGCD, out act);
    }

    protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
    {
        return base.MoveBackAbility(nextGCD, out act);
    }

    protected override bool SpeedAbility(IAction nextGCD, out IAction? act)
    {
        return base.SpeedAbility(nextGCD, out act);
    }

    #endregion

    #region Heal/Defense oGCD Logic

    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        return base.HealSingleAbility(nextGCD, out act);
    }

    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        return base.DefenseSingleAbility(nextGCD, out act);
    }

    #endregion

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (SurpanakhaPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomFlurryPvE.CanUse(out act))
        {
            return true;
        }

        if (JKickPvE.CanUse(out act))
        {
            return true;
        }

        if (BeingMortalPvE.CanUse(out act))
        {
            return true;
        }

        if (NightbloomPvE.CanUse(out act, skipStatusProvideCheck: true))
        {
            return true;
        }

        if (FeatherRainPvE.CanUse(out act))
        {
            return true;
        }

        return ShockStrikePvE.CanUse(out act) || base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if (Player.CurrentMp < 6000 && InCombat && LucidDreamingPvE.CanUse(out act))
        {
            return true;
        }

        return AethericMimicryPvE_19239.CanUse(out act) || base.GeneralAbility(nextGCD, out act);
    }

    #endregion

    #region GCD Logic

    protected override bool EmergencyGCD(out IAction? act)
    {
        return base.EmergencyGCD(out act);
    }

    protected override bool MyInterruptGCD(out IAction? act)
    {
        return FlyingSardinePvE.CanUse(out act) || base.MyInterruptGCD(out act);
    }

    protected override bool DefenseAreaGCD(out IAction? act)
    {
        if (ColdFogPvE.CanUse(out act))
        {
            return true;
        }

        return GobskinPvE.CanUse(out act) || base.DefenseAreaGCD(out act);
    }

    protected override bool DefenseSingleGCD(out IAction? act)
    {
        return base.DefenseSingleGCD(out act);
    }

    protected override bool HealAreaGCD(out IAction? act)
    {
        if (Player.GetHealthRatio() > 0.5 && WhiteWindPvE.CanUse(out act))
        {
            return true;
        }

        return StotramPvE.CanUse(out act) || base.HealAreaGCD(out act);
    }

    protected override bool HealSingleGCD(out IAction? act)
    {
        return PomCurePvE.CanUse(out act) || base.HealSingleGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        return base.MoveForwardGCD(out act);
    }

    protected override bool DispelGCD(out IAction? act)
    {
        return base.DispelGCD(out act);
    }

    protected override bool RaiseGCD(out IAction? act)
    {
        return AngelWhisperPvE.CanUse(out act) || base.RaiseGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (MightyGuardPvE.CanUse(out act))
        {
            return true;
        }

        if (BasicInstinctPvE.CanUse(out act))
        {
            return true;
        }

        if (WhiteDeathPvE.CanUse(out act))
        {
            return true;
        }

        if (BreathOfMagicPvE.CanUse(out act) &&
            (BreathOfMagicPvE.Target.Target?.WillStatusEnd(2, true,
                BreathOfMagicPvE.Setting.TargetStatusProvide ?? []) ?? false))
        {
            return true;
        }

        if (SongOfTormentPvE.CanUse(out act) && !IsLastAbility(ActionID.NightbloomPvE))
        {
            return true;
        }

        if (MatraMagicPvE.CanUse(out act))
        {
            return true;
        }

        if (TheRoseOfDestructionPvE.CanUse(out act))
        {
            return true;
        }

        if (TinglePvE.CanUse(out act) && TripleTridentPvE.Cooldown.WillHaveOneChargeGCD(2) &&
            !Player.HasStatus(true, StatusID.Tingling) && !IsLastGCD(ActionID.TinglePvE))
        {
            return true;
        }

        if (WhistlePvE.CanUse(out act) && Player.HasStatus(true, StatusID.Tingling))
        {
            return true;
        }

        if (TripleTridentPvE.CanUse(out act) && IsLastGCD(ActionID.WhistlePvE) &&
            Player.HasStatus(true, StatusID.Tingling))
        {
            return true;
        }

        if (SonicBoomPvE.CanUse(out act))
        {
            return true;
        }

        return FlyingSardinePvE.CanUse(out act) || base.GeneralGCD(out act);
    }

    #endregion

    protected override IBaseAction[] ActiveActions => [
                WaterCannonPvE
            ];
}