using RotationSolver.Basic.Rotations.Duties;

namespace RebornRotations.Duty;

[Rotation("Phantom Default", CombatType.PvE)]

internal class PhantomDefault : PhantomRotation
{
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

        return base.DefenseAreaAbility(nextGCD, out act);
    }
}
