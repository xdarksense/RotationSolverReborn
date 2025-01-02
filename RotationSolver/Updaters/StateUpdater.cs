using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Configuration.Conditions;

namespace RotationSolver.Updaters;
internal static class StateUpdater
{
    private static bool CanUseHealAction =>
        // PvP
        DataCenter.IsPvP
        // Job
        || ((DataCenter.Role == JobRole.Healer || Service.Config.UseHealWhenNotAHealer)
        && Service.Config.AutoHeal
        && (DataCenter.InCombat && CustomRotation.IsLongerThan(Service.Config.AutoHealTimeToKill)
            || Service.Config.HealOutOfCombat));

    public static void UpdateState()
    {
        DataCenter.CommandStatus = StatusFromCmdOrCondition();
        DataCenter.AutoStatus = StatusFromAutomatic();
    }

    private static AutoStatus StatusFromAutomatic()
    {
        AutoStatus status = AutoStatus.None;

        // Get the user-defined order of AutoStatus flags
        var autoStatusOrder = OtherConfiguration.AutoStatusOrder;

        foreach (var autoStatus in autoStatusOrder)
        {
            switch (autoStatus)
            {
                case (uint)AutoStatus.Dispel:
                    if (ShouldAddDispel())
                        status |= AutoStatus.Dispel;
                    break;

                case (uint)AutoStatus.Interrupt:
                    if (ShouldAddInterrupt())
                        status |= AutoStatus.Interrupt;
                    break;

                case (uint)AutoStatus.AntiKnockback:
                    if (ShouldAddAntiKnockback())
                        status |= AutoStatus.AntiKnockback;
                    break;

                case (uint)AutoStatus.Positional:
                    if (ShouldAddPositional())
                        status |= AutoStatus.Positional;
                    break;

                case (uint)AutoStatus.HealAreaAbility:
                    if (ShouldAddHealAreaAbility())
                        status |= AutoStatus.HealAreaAbility;
                    break;

                case (uint)AutoStatus.HealAreaSpell:
                    if (ShouldAddHealAreaSpell())
                        status |= AutoStatus.HealAreaSpell;
                    break;

                case (uint)AutoStatus.HealSingleAbility:
                    if (ShouldAddHealSingleAbility())
                        status |= AutoStatus.HealSingleAbility;
                    break;

                case (uint)AutoStatus.HealSingleSpell:
                    if (ShouldAddHealSingleSpell())
                        status |= AutoStatus.HealSingleSpell;
                    break;

                case (uint)AutoStatus.DefenseArea:
                    if (ShouldAddDefenseArea())
                        status |= AutoStatus.DefenseArea;
                    break;

                case (uint)AutoStatus.DefenseSingle:
                    if (ShouldAddDefenseSingle())
                        status |= AutoStatus.DefenseSingle;
                    break;

                case (uint)AutoStatus.Raise:
                    if (ShouldAddRaise())
                        status |= AutoStatus.Raise;
                    break;

                case (uint)AutoStatus.Provoke:
                    if (ShouldAddProvoke())
                        status |= AutoStatus.Provoke;
                    break;

                case (uint)AutoStatus.TankStance:
                    if (ShouldAddTankStance())
                        status |= AutoStatus.TankStance;
                    break;

                case (uint)AutoStatus.Speed:
                    if (ShouldAddSpeed())
                        status |= AutoStatus.Speed;
                    break;

                // Add other cases as needed
                default:
                    break;
            }
        }

        return status;
    }

    // Condition methods for each AutoStatus flag

    private static bool ShouldAddDispel()
    {
        if (DataCenter.DispelTarget != null)
        {
            if (DataCenter.DispelTarget.StatusList.Any(StatusHelper.IsDangerous))
            {
                return true;
            }
            else if (!DataCenter.HasHostilesInRange || Service.Config.DispelAll
                || DataCenter.IsPvP)
            {
                return true;
            }
        }
        return false;
    }

    private static bool ShouldAddRaise()
    {
        return DataCenter.DeathTarget != null;
    }

    private static bool ShouldAddPositional()
    {
        if (DataCenter.Role == JobRole.Melee && ActionUpdater.NextGCDAction != null
            && Service.Config.AutoUseTrueNorth)
        {
            var id = ActionUpdater.NextGCDAction.ID;
            if (ConfigurationHelper.ActionPositional.TryGetValue((ActionID)id, out var positional)
                && positional != ActionUpdater.NextGCDAction.Target.Target?.FindEnemyPositional()
                && (ActionUpdater.NextGCDAction.Target.Target?.HasPositional() ?? false))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ShouldAddHealAreaAbility()
    {
        if (!DataCenter.HPNotFull || !CanUseHealAction)
            return false;

        var singleAbility = ShouldHealSingle(StatusHelper.SingleHots,
            Service.Config.HealthSingleAbility,
            Service.Config.HealthSingleAbilityHot);

        var canHealAreaAbility = singleAbility > 2;

        if (DataCenter.PartyMembers.Count() > 2)
        {
            var ratio = GetHealingOfTimeRatio(Player.Object, StatusHelper.AreaHots);

            if (!canHealAreaAbility)
                canHealAreaAbility = DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference
                && DataCenter.PartyMembersAverHP < Lerp(Service.Config.HealthAreaAbility, Service.Config.HealthAreaAbilityHot, ratio);
        }

        return canHealAreaAbility;
    }

    private static bool ShouldAddHealAreaSpell()
    {
        if (!DataCenter.HPNotFull || !CanUseHealAction)
            return false;

        var singleSpell = ShouldHealSingle(StatusHelper.SingleHots,
            Service.Config.HealthSingleSpell,
            Service.Config.HealthSingleSpellHot);

        var canHealAreaSpell = singleSpell > 2;

        if (DataCenter.PartyMembers.Count() > 2)
        {
            var ratio = GetHealingOfTimeRatio(Player.Object, StatusHelper.AreaHots);

            if (!canHealAreaSpell)
                canHealAreaSpell = DataCenter.PartyMembersDifferHP < Service.Config.HealthDifference
                && DataCenter.PartyMembersAverHP < Lerp(Service.Config.HealthAreaSpell, Service.Config.HealthAreaSpellHot, ratio);
        }

        return canHealAreaSpell;
    }

    private static bool ShouldAddHealSingleAbility()
    {
        if (!DataCenter.HPNotFull || !CanUseHealAction)
            return false;

        var onlyHealSelf = Service.Config.OnlyHealSelfWhenNoHealer
            && DataCenter.Role != JobRole.Healer;

        if (onlyHealSelf)
        {
            return ShouldHealSingle(Player.Object, StatusHelper.SingleHots,
                Service.Config.HealthSingleAbility, Service.Config.HealthSingleAbilityHot);
        }
        else
        {
            var singleAbility = ShouldHealSingle(StatusHelper.SingleHots,
                Service.Config.HealthSingleAbility,
                Service.Config.HealthSingleAbilityHot);

            return singleAbility > 0;
        }
    }

    private static bool ShouldAddHealSingleSpell()
    {
        if (!DataCenter.HPNotFull || !CanUseHealAction)
            return false;

        var onlyHealSelf = Service.Config.OnlyHealSelfWhenNoHealer
            && DataCenter.Role != JobRole.Healer;

        if (onlyHealSelf)
        {
            return ShouldHealSingle(Player.Object, StatusHelper.SingleHots,
                Service.Config.HealthSingleSpell, Service.Config.HealthSingleSpellHot);
        }
        else
        {
            var singleSpell = ShouldHealSingle(StatusHelper.SingleHots,
                Service.Config.HealthSingleSpell,
                Service.Config.HealthSingleSpellHot);

            return singleSpell > 0;
        }
    }

    private static bool ShouldAddDefenseArea()
    {
        if (!DataCenter.InCombat || !Service.Config.UseDefenseAbility)
            return false;

        return DataCenter.IsHostileCastingAOE;
    }

    private static bool ShouldAddDefenseSingle()
    {
        if (!DataCenter.InCombat || !Service.Config.UseDefenseAbility)
            return false;

        if (DataCenter.Role == JobRole.Healer)
        {
            if (DataCenter.PartyMembers.Any((tank) =>
            {
                var attackingTankObj = DataCenter.AllHostileTargets.Where(t => t.TargetObjectId == tank.GameObjectId);

                if (attackingTankObj.Count() != 1)
                    return false;

                return DataCenter.IsHostileCastingToTank;
            }))
            {
                return true;
            }
        }

        if (DataCenter.Role == JobRole.Tank)
        {
            var movingHere = (float)DataCenter.NumberOfHostilesInRange / DataCenter.NumberOfHostilesInMaxRange > 0.3f;

            var tarOnMe = DataCenter.AllHostileTargets.Where(t => t.DistanceToPlayer() <= 3
            && t.TargetObject == Player.Object);
            var tarOnMeCount = tarOnMe.Count();
            var attackedCount = tarOnMe.Count(ObjectHelper.IsAttacked);
            var attacked = (float)attackedCount / tarOnMeCount > 0.7f;

            if (tarOnMeCount >= Service.Config.AutoDefenseNumber
                && Player.Object.GetHealthRatio() <= Service.Config.HealthForAutoDefense
                && movingHere && attacked)
            {
                return true;
            }

            if (DataCenter.IsHostileCastingToTank)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ShouldAddAntiKnockback()
    {
        if (!DataCenter.InCombat || !Service.Config.UseKnockback)
            return false;

        return DataCenter.AreHostilesCastingKnockback;
    }

    private static bool ShouldAddProvoke()
    {
        if (!DataCenter.InCombat)
            return false;

        if (DataCenter.Role == JobRole.Tank
            && (Service.Config.AutoProvokeForTank
                || DataCenter.AllianceMembers.Count(o => o.IsJobCategory(JobRole.Tank)) < 2)
            && DataCenter.ProvokeTarget != null)
        {
            return true;
        }

        return false;
    }

    private static bool ShouldAddInterrupt()
    {
        if (!DataCenter.InCombat)
            return false;

        return DataCenter.InterruptTarget != null && Service.Config.InterruptibleMoreCheck;
    }

    private static bool ShouldAddTankStance()
    {
        if (!Service.Config.AutoTankStance || DataCenter.Role != JobRole.Tank)
            return false;

        if (!DataCenter.AllianceMembers.Any(t => t.IsJobCategory(JobRole.Tank) && t.CurrentHp != 0 && t.HasStatus(false, StatusHelper.TankStanceStatus))
            && !CustomRotation.HasTankStance)
        {
            return true;
        }

        return false;
    }

    private static bool ShouldAddSpeed()
    {
        return DataCenter.IsMoving && DataCenter.NotInCombatDelay && Service.Config.AutoSpeedOutOfCombat;
    }

    // Helper methods used in condition methods

    static float GetHealingOfTimeRatio(IBattleChara target, params StatusID[] statusIds)
    {
        const float buffWholeTime = 15;

        var buffTime = target.StatusTime(false, statusIds);

        return Math.Min(1, buffTime / buffWholeTime);
    }

    static int ShouldHealSingle(StatusID[] hotStatus, float healSingle, float healSingleHot)
        => DataCenter.PartyMembers.Count(p => ShouldHealSingle(p, hotStatus, healSingle, healSingleHot));

    static bool ShouldHealSingle(IBattleChara target, StatusID[] hotStatus, float healSingle, float healSingleHot)
    {
        if (target == null) return false;

        var ratio = GetHealingOfTimeRatio(target, hotStatus);

        var h = target.GetHealthRatio();
        if (h == 0 || !target.NeedHealing()) return false;

        return h < Lerp(healSingle, healSingleHot, ratio);
    }

    static float Lerp(float a, float b, float ratio)
    {
        return a + (b - a) * ratio;
    }

    private static AutoStatus StatusFromCmdOrCondition()
    {
        var status = DataCenter.SpecialType switch
        {
            SpecialCommandType.HealArea => AutoStatus.HealAreaSpell
                                | AutoStatus.HealAreaAbility,
            SpecialCommandType.HealSingle => AutoStatus.HealSingleSpell
                                | AutoStatus.HealSingleAbility,
            SpecialCommandType.DefenseArea => AutoStatus.DefenseArea,
            SpecialCommandType.DefenseSingle => AutoStatus.DefenseSingle,
            SpecialCommandType.DispelStancePositional => AutoStatus.Dispel
                                | AutoStatus.TankStance
                                | AutoStatus.Positional,
            SpecialCommandType.RaiseShirk => AutoStatus.Raise
                                | AutoStatus.Shirk,
            SpecialCommandType.MoveForward => AutoStatus.MoveForward,
            SpecialCommandType.MoveBack => AutoStatus.MoveBack,
            SpecialCommandType.AntiKnockback => AutoStatus.AntiKnockback,
            SpecialCommandType.Burst => AutoStatus.Burst,
            SpecialCommandType.Speed => AutoStatus.Speed,
            SpecialCommandType.LimitBreak => AutoStatus.LimitBreak,
            _ => AutoStatus.None,
        };

        AddStatus(ref status, AutoStatus.HealAreaSpell | AutoStatus.HealAreaAbility, DataCenter.RightSet.HealAreaConditionSet);
        AddStatus(ref status, AutoStatus.HealSingleSpell | AutoStatus.HealSingleAbility, DataCenter.RightSet.HealSingleConditionSet);
        AddStatus(ref status, AutoStatus.DefenseArea, DataCenter.RightSet.DefenseAreaConditionSet);
        AddStatus(ref status, AutoStatus.DefenseSingle, DataCenter.RightSet.DefenseSingleConditionSet);

        AddStatus(ref status, AutoStatus.Dispel | AutoStatus.TankStance | AutoStatus.Positional,
            DataCenter.RightSet.DispelStancePositionalConditionSet);
        AddStatus(ref status, AutoStatus.Raise | AutoStatus.Shirk, DataCenter.RightSet.RaiseShirkConditionSet);
        AddStatus(ref status, AutoStatus.MoveForward, DataCenter.RightSet.MoveForwardConditionSet);
        AddStatus(ref status, AutoStatus.MoveBack, DataCenter.RightSet.MoveBackConditionSet);
        AddStatus(ref status, AutoStatus.AntiKnockback, DataCenter.RightSet.AntiKnockbackConditionSet);

        if (!status.HasFlag(AutoStatus.Burst) && Service.Config.AutoBurst)
        {
            status |= AutoStatus.Burst;
        }
        AddStatus(ref status, AutoStatus.Speed, DataCenter.RightSet.SpeedConditionSet);
        AddStatus(ref status, AutoStatus.LimitBreak, DataCenter.RightSet.LimitBreakConditionSet);

        return status;
    }

    private static void AddStatus(ref AutoStatus status, AutoStatus flag, ConditionSet set)
    {
        AddStatus(ref status, flag, () => set.IsTrue(DataCenter.RightNowRotation));
    }

    private static void AddStatus(ref AutoStatus status, AutoStatus flag, Func<bool> getValue)
    {
        if (status.HasFlag(flag) || !getValue()) return;

        status |= flag;
    }
}