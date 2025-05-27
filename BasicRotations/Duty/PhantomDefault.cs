using Dalamud.Game.Text.SeStringHandling.Payloads;
using RotationSolver.Basic.Rotations.Duties;

namespace RebornRotations.Duty;

[Rotation("Phantom Default", CombatType.PvE)]

public sealed class PhantomDefault : PhantomRotation
{
    public override bool InterruptAbility(IAction nextGCD, out IAction? act)
    {
        if (OccultFalconPvE.CanUse(out act))
        {
            return true;
        }

        return base.InterruptAbility(nextGCD, out act);
    }

    public override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat && OffensiveAriaPvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && PhantomAimPvE.CanUse(out act))
        {
            return true;
        }

        return base.GeneralAbility(nextGCD, out act);
    }

    public override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (OffensiveAriaPvE.CanUse(out act))
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }

    public override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (MightyMarchPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultUnicornPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    public override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (OccultHealPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealSingleAbility(nextGCD, out act);
    }

    public override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (OccultFeatherfootPvE.CanUse(out act))
        {
            return true;
        }

        return base.MoveForwardAbility(nextGCD, out act);
    }

    public override bool DefenseSingleGCD(out IAction? act)
    {
        if (PrayPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleGCD(out act);
    }
}
