using RotationSolver.Basic.Actions;
using RotationSolver.Basic.Attributes;
using RotationSolver.Basic.Data;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.DummyRotations;

[Rotation("TestingRotation", CombatType.PvE, GameVersion = "7.05")]
[Api(3)]

public sealed class TestingRotation : PictomancerRotation
{
    #region Additional oGCD Logic

    [RotationDesc]
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        
        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool MoveBackAbility(IAction nextGCD, out IAction? act)
    {

        return base.MoveBackAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {

        return base.HealAreaAbility(nextGCD, out act);
    }

    [RotationDesc]
    protected override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {

        return base.HealSingleAbility(nextGCD, out act);
    }
    #endregion

    #region oGCD Logic

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {

        return base.GeneralAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        
        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool MoveForwardGCD(out IAction? act)
    {
       
        return base.MoveForwardGCD(out act);
    }

    protected override bool DefenseAreaGCD(out IAction? act)
    {
        
        return base.DefenseAreaGCD(out act);
    }

    protected override bool DefenseSingleGCD(out IAction? act)
    {
        
        return base.DefenseSingleGCD(out act);
    }

    protected override bool HealAreaGCD(out IAction? act)
    {
        
        return base.HealAreaGCD(out act);
    }

    protected override bool HealSingleGCD(out IAction? act)
    {
        
        return base.HealSingleGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        
        return base.GeneralGCD(out act);
    }

    #endregion
}
