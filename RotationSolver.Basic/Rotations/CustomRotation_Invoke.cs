using ECommons.Logging;

namespace RotationSolver.Basic.Rotations;

partial class CustomRotation
{
    /// <inheritdoc/>
    public bool TryInvoke(out IAction? newAction, out IAction? gcdAction)
    {
        newAction = gcdAction = null;
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            UpdateInfo();

            IBaseAction.ActionPreview = true;
            UpdateActions(Role);
            IBaseAction.ActionPreview = false;

            CountingOfLastUsing = CountingOfCombatTimeUsing = 0;
            newAction = Invoke(out gcdAction);
            if (InCombat || CountOfTracking == 0)
            {
                AverageCountOfLastUsing =
                    (AverageCountOfLastUsing * CountOfTracking + CountingOfLastUsing)
                    / ++CountOfTracking;
                MaxCountOfLastUsing = Math.Max(MaxCountOfLastUsing, CountingOfLastUsing);

                AverageCountOfCombatTimeUsing =
                    (AverageCountOfCombatTimeUsing * (CountOfTracking - 1) + CountingOfCombatTimeUsing)
                    / CountOfTracking;
                MaxCountOfCombatTimeUsing = Math.Max(MaxCountOfCombatTimeUsing, CountingOfCombatTimeUsing);
            }

            if (!IsValid) IsValid = true;
        }
        catch (Exception? ex)
        {
            WhyNotValid = "Failed to invoke the next action, please contact support.";

            while (ex != null)
            {
                if (!string.IsNullOrEmpty(ex.Message)) WhyNotValid += "\n" + ex.Message;
                if (!string.IsNullOrEmpty(ex.StackTrace)) WhyNotValid += "\n" + ex.StackTrace;
                ex = ex.InnerException;
            }

            // Log the exception details
            PluginLog.Error(WhyNotValid);

            IsValid = false;
        }

        return newAction != null;
    }

    private void UpdateActions(JobRole role)
    {
        ActionMoveForwardGCD = MoveForwardGCD(out var act) ? act : null;

        UpdateHealingActions(role, out act);
        UpdateDefenseActions(out act);
        UpdateDispelAndRaiseActions(role, out act);
        UpdatePositionalActions(role, out act);
        UpdateMovementActions(out act);
    }

    private void UpdateHealingActions(JobRole role, out IAction? act)
    {
        act = null; // Ensure 'act' is assigned before any return

        try
        {
            if (!DataCenter.HPNotFull && role == JobRole.Healer)
            {
                ActionHealAreaGCD = ActionHealAreaAbility = ActionHealSingleGCD = ActionHealSingleAbility = null;
            }
            else
            {
                ActionHealAreaGCD = HealAreaGCD(out act) ? act : null;
                ActionHealSingleGCD = HealSingleGCD(out act) ? act : null;

                ActionHealAreaAbility = HealAreaAbility(AddlePvE, out act) ? act : null;
                ActionHealSingleAbility = HealSingleAbility(AddlePvE, out act) ? act : null;
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            PluginLog.Error($"Exception in UpdateHealingActions method: {ex.Message}");
            // Optionally, set actions to null in case of an exception
            ActionHealAreaGCD = ActionHealAreaAbility = ActionHealSingleGCD = ActionHealSingleAbility = null;
        }
    }

    private void UpdateDefenseActions(out IAction? act)
    {
        act = null; // Ensure 'act' is assigned before any return

        IBaseAction.TargetOverride = TargetType.BeAttacked;
        ActionDefenseAreaGCD = DefenseAreaGCD(out act) ? act : null;
        ActionDefenseSingleGCD = DefenseSingleGCD(out act) ? act : null;
        IBaseAction.TargetOverride = null;

        try
        {
            ActionDefenseAreaAbility = DefenseAreaAbility(AddlePvE, out act) ? act : null;
            ActionDefenseSingleAbility = DefenseSingleAbility(AddlePvE, out act) ? act : null;
        }
        catch (MissingMethodException ex)
        {
            // Log the exception or handle it as needed
            BasicWarningHelper.AddSystemWarning($"Exception in UpdateDefenseActions method: {ex.Message}");
            // Optionally, set actions to null in case of an exception
            ActionDefenseAreaAbility = ActionDefenseSingleAbility = null;
        }
    }

    private void UpdateDispelAndRaiseActions(JobRole role, out IAction? act)
    {
        act = null; // Ensure 'act' is assigned before any return

        IBaseAction.TargetOverride = TargetType.Death;

        ActionDispelStancePositionalGCD = role switch
        {
            JobRole.Healer => DataCenter.DispelTarget != null && DispelGCD(out act) ? act : null,
            _ => null,
        };

        ActionRaiseShirkGCD = role switch
        {
            JobRole.Healer => DataCenter.DeathTarget != null && RaiseSpell(out act, true) ? act : null,
            _ => null,
        };

        IBaseAction.TargetOverride = null;
    }

    private void UpdatePositionalActions(JobRole role, out IAction? act)
    {
        act = null; // Ensure 'act' is assigned before any return

        ActionDispelStancePositionalAbility = role switch
        {
            JobRole.Melee => TrueNorthPvE.CanUse(out act) ? act : null,
            JobRole.Tank => TankStance?.CanUse(out act) ?? false ? act : null,
            _ => null,
        };

        ActionRaiseShirkAbility = role switch
        {
            JobRole.Tank => ShirkPvE.CanUse(out act) ? act : null,
            _ => null,
        };

        ActionAntiKnockbackAbility = AntiKnockback(role, AddlePvE, out act) ? act : null;
    }

    private void UpdateMovementActions(out IAction? act)
    {
        act = null; // Ensure 'act' is assigned before any return

        IBaseAction.TargetOverride = TargetType.Move;
        var movingTarget = MoveForwardAbility(AddlePvE, out act);
        IBaseAction.TargetOverride = null;
        ActionMoveForwardAbility = movingTarget ? act : null;

        if (movingTarget && act is IBaseAction a)
        {
            UpdateMoveTarget(a);
        }
        else
        {
            MoveTarget = null;
        }

        ActionMoveBackAbility = MoveBackAbility(AddlePvE, out act) ? act : null;
        ActionSpeedAbility = SpeedAbility(AddlePvE, out act) ? act : null;
    }

    private void UpdateMoveTarget(IBaseAction a)
    {
        if (a.PreviewTarget.HasValue && a.PreviewTarget.Value.Target != Player
            && a.PreviewTarget.Value.Target != null)
        {
            var dir = Player.Position - a.PreviewTarget.Value.Position;
            var length = dir?.Length() ?? 0;
            if (length != 0 && dir.HasValue)
            {
                var d = dir.Value / length;
                MoveTarget = a.PreviewTarget.Value.Position + d * MathF.Min(length, Player.HitboxRadius + a.PreviewTarget.Value.Target.HitboxRadius);
            }
            else
            {
                MoveTarget = a.PreviewTarget.Value.Position;
            }
        }
        else
        {
            if ((ActionID)a.ID == ActionID.EnAvantPvE)
            {
                var dir = new Vector3(MathF.Sin(Player.Rotation), 0, MathF.Cos(Player.Rotation));
                MoveTarget = Player.Position + dir * 10; // Consider defining 10 as a constant
            }
            else
            {
                MoveTarget = a.PreviewTarget?.Position == a.PreviewTarget?.Target?.Position ? null : a.PreviewTarget?.Position;
            }
        }
    }

    private IAction? Invoke(out IAction? gcdAction)
    {
        // Initialize the output parameter
        gcdAction = null;

        // Reset special action flags
        IBaseAction.ShouldEndSpecial = false;
        IBaseAction.IgnoreClipping = true;

        try
        {
            // Check for countdown and return the appropriate action if not in combat
            var countDown = Service.CountDownTime;
            if (countDown > 0 && !DataCenter.InCombat)
            {
                return CountDownAction(countDown);
            }

            // Reset target override
            IBaseAction.TargetOverride = null;

            // Attempt to get the GCD action
            gcdAction = GCD();
            IBaseAction.IgnoreClipping = false;

            // If a GCD action is available, determine if it can be used or if an ability should be used instead
            if (gcdAction != null)
            {
                if (ActionHelper.CanUseGCD)
                {
                    return gcdAction;
                }

                if (Ability(gcdAction, out var ability))
                {
                    return ability;
                }

                return gcdAction;
            }
            else
            {
                // If no GCD action is available, attempt to use an ability
                IBaseAction.IgnoreClipping = true;
                if (Ability(AddlePvE, out var ability))
                {
                    return ability;
                }
                IBaseAction.IgnoreClipping = false;

                return null;
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Exception in Invoke method: {ex.Message}");
            return null;
        }
        finally
        {
            // Ensure IgnoreClipping is reset
            IBaseAction.IgnoreClipping = false;
        }
    }

    /// <summary>
    /// The action in countdown.
    /// </summary>
    /// <param name="remainTime"></param>
    /// <returns></returns>
    protected virtual IAction? CountDownAction(float remainTime) => null;
}
