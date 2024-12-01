namespace DefaultRotations.Magical;

[Rotation("DOES NOT WORK", CombatType.PvE, GameVersion = "7.11")]
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
        act = null;

        return base.EmergencyAbility(nextGCD, out act);
    }
    #endregion

    #region Move oGCD Logic
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.MoveForwardAbility(nextGCD, out act);
    }

    protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.MoveBackAbility(nextGCD, out act);
    }

    protected override bool SpeedAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.SpeedAbility(nextGCD, out act);
    }
    #endregion

    #region Heal/Defense oGCD Logic
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.HealSingleAbility(nextGCD, out act);
    }

    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.DefenseAreaAbility(nextGCD, out act);
    }

    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;


        return base.DefenseSingleAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (JKickPvE.CanUse(out act))
        {
            if (Player.HasStatus(true, StatusID.Harmonized)) return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;

        if (AethericMimicryPvE_19239.CanUse(out act)) return true;
        return base.GeneralAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool EmergencyGCD(out IAction? act)
    {
        act = null;

        return base.EmergencyGCD(out act);
    }

    protected override bool MyInterruptGCD(out IAction? act)
    {
        if (FlyingSardinePvE.CanUse(out act)) return true;
        return base.MyInterruptGCD(out act);
    }

    protected override bool DefenseAreaGCD(out IAction? act)
    {
        if (ColdFogPvE.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    protected override bool DefenseSingleGCD(out IAction? act)
    {
        act = null;

        return base.DefenseSingleGCD(out act);
    }

    protected override bool HealAreaGCD(out IAction? act)
    {
        if (WhiteWindPvE.CanUse(out act)) return true;
        return base.HealAreaGCD(out act);
    }

    protected override bool HealSingleGCD(out IAction? act)
    {
        if (WhiteWindPvE.CanUse(out act)) return true;
        return base.HealSingleGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        act = null;
        
        return base.MoveForwardGCD(out act);
    }

    protected override bool DispelGCD(out IAction? act)
    {
        act = null;

        return base.DispelGCD(out act);
    }

    protected override bool RaiseGCD(out IAction? act)
    {
        if (AngelWhisperPvE.CanUse(out act)) return true;
        return base.RaiseGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (WhiteDeathPvE.CanUse(out act)) return true;

        if (BreathOfMagicPvE.CanUse(out act)) return true;
        if (SongOfTormentPvE.CanUse(out act)) return true;

        if (MatraMagicPvE.CanUse(out act)) return true;
        if (TheRoseOfDestructionPvE.CanUse(out act)) return true;

        if (TripleTridentPvE.CanUse(out act) && IsLastGCD(ActionID.TinglePvE)) return true;
        if (TinglePvE.CanUse(out act)) return true;
        if (SonicBoomPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }
    #endregion
}