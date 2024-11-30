namespace RotationSolver.Basic.Rotations;

partial class CustomRotation
{
    private static DateTime _nextTimeToHeal = DateTime.MinValue;
    private static readonly Random _random = new Random();

    private IAction? GCD()
    {
        var act = DataCenter.CommandNextAction;

        IBaseAction.ForceEnable = true;
        if (act is IBaseAction a && a.Info.IsRealGCD
            && a.CanUse(out _, usedUp: true, skipAoeCheck: true)) return act;
        IBaseAction.ForceEnable = false;

        try
        {
            IBaseAction.ShouldEndSpecial = true;

            if (DataCenter.MergedStatus.HasFlag(AutoStatus.LimitBreak)
                && UseLimitBreak(out act)) return act;

            IBaseAction.ShouldEndSpecial = false;

            if (EmergencyGCD(out act)) return act;

            if (DataCenter.MergedStatus.HasFlag(AutoStatus.MoveForward)
                && MoveForwardGCD(out act))
            {
                if (act is IBaseAction b && ObjectHelper.DistanceToPlayer(b.Target.Target) > 5) return act;
            }

            IBaseAction.TargetOverride = TargetType.Heal;

            if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaSpell))
            {
                if (HealAreaGCD(out act)) return act;
            }
            if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealAreaSpell)
                && CanHealAreaSpell)
            {
                IBaseAction.AutoHealCheck = true;
                if (HealAreaGCD(out act)) return act;
                IBaseAction.AutoHealCheck = false;
            }
            if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleSpell)
                && CanHealSingleSpell)
            {
                if (HealSingleGCD(out act)) return act;
            }
            if (DataCenter.AutoStatus.HasFlag(AutoStatus.HealSingleSpell))
            {
                IBaseAction.AutoHealCheck = true;
                if (HealSingleGCD(out act)) return act;
                IBaseAction.AutoHealCheck = false;
            }

            IBaseAction.TargetOverride = null;

            if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseArea)
                && DefenseAreaGCD(out act)) return act;

            IBaseAction.TargetOverride = TargetType.BeAttacked;

            if (DataCenter.MergedStatus.HasFlag(AutoStatus.DefenseSingle)
                && DefenseSingleGCD(out act)) return act;

            IBaseAction.TargetOverride = TargetType.Dispel;
            if (DataCenter.MergedStatus.HasFlag(AutoStatus.Dispel)
                && DispelGCD(out act)) return act;

            IBaseAction.TargetOverride = TargetType.Death;

            if (RaiseSpell(out act, false)) return act;

            if (Service.Config.RaisePlayerByCasting && SwiftcastPvE.Cooldown.IsCoolingDown && RaiseSpell(out act, true)) return act;

            IBaseAction.TargetOverride = null;

            IBaseAction.ShouldEndSpecial = false;
            IBaseAction.TargetOverride = null;

            if (GeneralGCD(out var action)) return action;

            if (Service.Config.HealWhenNothingTodo && InCombat)
            {
                // Please don't tell me someone's fps is less than 1!!
                if (DateTime.Now - _nextTimeToHeal > TimeSpan.FromSeconds(1))
                {
                    var min = Service.Config.HealWhenNothingTodoDelay.X;
                    var max = Service.Config.HealWhenNothingTodoDelay.Y;
                    _nextTimeToHeal = DateTime.Now + TimeSpan.FromSeconds(_random.NextDouble() * (max - min) + min);
                }
                else if (_nextTimeToHeal < DateTime.Now)
                {
                    _nextTimeToHeal = DateTime.Now;

                    if (PartyMembersMinHP < Service.Config.HealWhenNothingTodoBelow)
                    {
                        IBaseAction.TargetOverride = TargetType.Heal;

                        if (DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference
                            && DataCenter.PartyMembersHP.Count(i => i < 1) > 2
                            && HealAreaGCD(out act)) return act;
                        if (HealSingleGCD(out act)) return act;

                        IBaseAction.TargetOverride = null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Exception in GCD method: {ex.Message}");
        }
        finally
        {
            // Ensure these are reset
            IBaseAction.ShouldEndSpecial = false;
            IBaseAction.TargetOverride = null;
        }

        return null;
    }


    private bool UseLimitBreak(out IAction? act)
    {
        act = null;

        return LimitBreakLevel switch
        {
            1 => (DataCenter.IsPvP ? LimitBreakPvP?.CanUse(out act, skipAoeCheck: true) : LimitBreak1?.CanUse(out act, skipAoeCheck: true)) ?? false,
            2 => LimitBreak2?.CanUse(out act, skipAoeCheck: true) ?? false,
            3 => LimitBreak3?.CanUse(out act, skipAoeCheck: true) ?? false,
            _ => false,
        };
    }

    private bool RaiseSpell(out IAction? act, bool mustUse)
    {
        act = null;

        if (DataCenter.CommandStatus.HasFlag(AutoStatus.Raise))
        {
            IBaseAction.ShouldEndSpecial = true;
            if (RaiseGCD(out act) || RaiseAction(out act, false)) return true;
        }
        IBaseAction.ShouldEndSpecial = false;

        if (!DataCenter.AutoStatus.HasFlag(AutoStatus.Raise)) return false;

        if (RaiseGCD(out act)) return true;

        if (RaiseAction(out act, true))
        {
            if (HasSwift) return true;

            if (Service.Config.RaisePlayerBySwift && !SwiftcastPvE.Cooldown.IsCoolingDown && SwiftcastPvE.CanUse(out act))
            {
                return true;
            }

            if (mustUse && !IsMoving)
            {
                return true;
            }
        }

        return false;

        bool RaiseAction(out IAction act, bool ignoreCastingCheck)
        {
            if (Player.CurrentMp > Service.Config.LessMPNoRaise && (Raise?.CanUse(out act, skipCastingCheck: ignoreCastingCheck) ?? false)) return true;

            act = null!;
            return false;
        }
    }

    /// <summary>
    /// Attempts to use the Raise GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    protected virtual bool RaiseGCD(out IAction? act)
    {
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.Raise))
        {
            IBaseAction.ShouldEndSpecial = true;
        }
        if (DataCenter.RightNowDutyRotation?.RaiseGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null; return false;
    }

    /// <summary>
    /// Attempts to use the Dispel GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    protected virtual bool DispelGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.Dispel))
        {
            IBaseAction.ShouldEndSpecial = true;
        }
        if (!HasSwift && EsunaPvE.CanUse(out act)) return true;
        if (DataCenter.RightNowDutyRotation?.DispelGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        return false;
    }

    /// <summary>
    /// Attempts to use the Emergency GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    protected virtual bool EmergencyGCD(out IAction? act)
    {
        #region PvP
        if (GuardPvP.CanUse(out act) && (Player.GetHealthRatio() <= Service.Config.HealthForGuard || DataCenter.CommandStatus.HasFlag(AutoStatus.Raise | AutoStatus.Shirk))) return true;

        if (StandardissueElixirPvP.CanUse(out act)) return true;
        #endregion

        act = null;
        if (ShouldSkipAction()) return false;

        if (DataCenter.RightNowDutyRotation?.EmergencyGCD(out act) ?? false) return true;

        act = null!; return false;
    }

    /// <summary>
    /// Attempts to use the Move Forward GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    [RotationDesc(DescType.MoveForwardGCD)]
    protected virtual bool MoveForwardGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.MoveForward))
        {
            IBaseAction.ShouldEndSpecial = true;
        }

        if (DataCenter.RightNowDutyRotation?.MoveForwardGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null; return false;
    }

    /// <summary>
    /// Attempts to use the Heal Single GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    [RotationDesc(DescType.HealSingleGCD)]
    protected virtual bool HealSingleGCD(out IAction? act)
    {
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealSingleSpell))
        {
            IBaseAction.ShouldEndSpecial = true;
        }

        if (DataCenter.RightNowDutyRotation?.HealSingleGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null; return false;
    }

    /// <summary>
    /// Attempts to use the Heal Area GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    [RotationDesc(DescType.HealAreaGCD)]
    protected virtual bool HealAreaGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.HealAreaSpell))
        {
            IBaseAction.ShouldEndSpecial = true;
        }

        if (DataCenter.RightNowDutyRotation?.HealAreaGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null!; return false;
    }

    /// <summary>
    /// Attempts to use the Defense Single GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    [RotationDesc(DescType.DefenseSingleGCD)]
    protected virtual bool DefenseSingleGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseSingle))
        {
            IBaseAction.ShouldEndSpecial = true;
        }

        if (DataCenter.RightNowDutyRotation?.DefenseSingleGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null!; return false;
    }

    /// <summary>
    /// Attempts to use the Defense Area GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    [RotationDesc(DescType.DefenseAreaGCD)]
    protected virtual bool DefenseAreaGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;
        if (DataCenter.CommandStatus.HasFlag(AutoStatus.DefenseArea))
        {
            IBaseAction.ShouldEndSpecial = true;
        }

        if (DataCenter.RightNowDutyRotation?.DefenseAreaGCD(out act) ?? false) return true;
        IBaseAction.ShouldEndSpecial = false;
        act = null; return false;
    }

    /// <summary>
    /// Attempts to use the General GCD action.
    /// </summary>
    /// <param name="act">The action to be performed.</param>
    /// <returns>True if the action can be used; otherwise, false.</returns>
    protected virtual bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (ShouldSkipAction()) return false;

        if (DataCenter.RightNowDutyRotation?.GeneralGCD(out act) ?? false) return true;
        act = null; return false;
    }

    private bool ShouldSkipAction()
    {
        return DataCenter.CommandStatus.HasFlag(AutoStatus.Raise) && Role is JobRole.Healer && HasSwift;
    }
}
