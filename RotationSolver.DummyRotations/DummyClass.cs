using RotationSolver.Basic.Actions;
using RotationSolver.Basic.Attributes;
using RotationSolver.Basic.Data;
using RotationSolver.Basic.Rotations.Basic;

namespace RotationSolver.DummyRotations
{
    [Rotation("Samurai", CombatType.PvE, GameVersion = "7.05")]
    [SourceCode(Path = "main/DefaultRotations/Melee/SAM_Testing.cs")]
    [Api(3)]
    public class SAM_Testing : SamuraiRotation
    {
        #region Config Options

        [Range(0, 85, ConfigUnitType.None, 5)]
        [RotationConfig(CombatType.PvE, Name = "Use Kenki above.")]
        public int AddKenki { get; set; } = 50;

        #endregion

        #region Countdown Logic

        protected override IAction? CountDownAction(float remainTime)
        {
            return base.CountDownAction(remainTime);
        }

        #endregion

        #region Additional oGCD Logic
        protected sealed override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
        {
            return base.MoveForwardAbility(nextGCD, out act);
        }

        protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
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
            return base.AttackAbility(nextGCD, out act);
        }

        protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        #endregion

        #region GCD Logic

        protected override bool GeneralGCD(out IAction? act)
        {
            return base.GeneralGCD(out act);
        }

        #endregion
    }
}
