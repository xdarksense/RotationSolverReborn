using RotationSolver.Basic.Rotations.Duties;

namespace RebornRotations.Duty;

[Rotation("Variant Default", CombatType.PvE)]

internal class VariantDefault : VariantRotation
{
    public override bool ProvokeAbility(IAction nextGCD, out IAction? act)
    {
        return VariantUltimatumPvE.CanUse(out act) || base.ProvokeAbility(nextGCD, out act);
    }

    public override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        if (VariantSpiritDartPvE.CanUse(out act, skipAoeCheck: true))
        {
            return true;
        }

        if (VariantSpiritDartPvE_33863.CanUse(out act, skipAoeCheck: true))
        {
            return true;
        }

        if (VariantRampartPvE.CanUse(out act))
        {
            return true;
        }

        return VariantRampartPvE_33864.CanUse(out act) || base.AttackAbility(nextGCD, out act);
    }

    public override bool HealSingleGCD(out IAction? act)
    {
        if (VariantCurePvE.CanUse(out act, skipStatusProvideCheck: true))
        {
            return true;
        }

        return VariantCurePvE_33862.CanUse(out act, skipStatusProvideCheck: true) || base.HealSingleGCD(out act);
    }

    public override bool RaiseGCD(out IAction? act)
    {
        if (VariantRaisePvE.CanUse(out act))
        {
            return true;
        }

        return VariantRaiseIiPvE.CanUse(out act) || base.RaiseGCD(out act);
    }
}
