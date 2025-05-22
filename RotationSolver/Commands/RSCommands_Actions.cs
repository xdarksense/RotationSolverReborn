using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using RotationSolver.Basic.Configuration;
using RotationSolver.Helpers;
using RotationSolver.Updaters;

namespace RotationSolver.Commands
{
    public static partial class RSCommands
    {
        private static DateTime _lastClickTime = DateTime.MinValue;
        private static bool _lastState;
        private static bool started = false;
        internal static DateTime _lastUsedTime = DateTime.MinValue;
        internal static uint _lastActionID;
        private static float _lastCountdownTime = 0;
        private static Job _previousJob = Job.ADV;
        private static readonly Random random = new();

        public static void IncrementState()
        {
            if (!DataCenter.State) { DoStateCommandType(StateCommandType.Auto); return; }
            if (DataCenter.State && !DataCenter.IsManual && DataCenter.TargetingType == TargetingType.Big) { DoStateCommandType(StateCommandType.Auto); return; }
            if (DataCenter.State && !DataCenter.IsManual) { DoStateCommandType(StateCommandType.Manual); return; }
            if (DataCenter.State && DataCenter.IsManual) { DoStateCommandType(StateCommandType.Off); return; }
        }

        internal static unsafe bool CanDoAnAction(bool isGCD)
        {
            // Cache frequently accessed properties to avoid redundant calls
            bool currentState = DataCenter.State;

            if (!_lastState || !currentState)
            {
                _lastState = currentState;
                return false;
            }
            _lastState = currentState;

            if (!Player.AvailableThreadSafe)
            {
                return false;
            }

            // Precompute the delay range to avoid recalculating it multiple times
            TimeSpan delayRange = TimeSpan.FromMilliseconds(random.Next(
                (int)(Service.Config.ClickingDelay.X * 1000),
                (int)(Service.Config.ClickingDelay.Y * 1000)));

            if (DateTime.Now - _lastClickTime < delayRange)
            {
                return false;
            }

            _lastClickTime = DateTime.Now;

            // Avoid unnecessary checks if isGCD is true
            return isGCD || ActionUpdater.NextAction is not IBaseAction nextAction || !nextAction.Info.IsRealGCD;
        }

        public static void DoAction()
        {
            if (!Player.AvailableThreadSafe)
            {
                return;
            }

            if (Player.Object.StatusList == null)
            {
                return;
            }

            StatusID[] noCastingStatusArray;
            HashSet<uint> noCastingStatus = OtherConfiguration.NoCastingStatus;
            if (noCastingStatus != null)
            {
                noCastingStatusArray = new StatusID[noCastingStatus.Count];
                int index = 0;
                foreach (uint status in noCastingStatus)
                {
                    noCastingStatusArray[index++] = (StatusID)status;
                }
            }
            else
            {
                noCastingStatusArray = Array.Empty<StatusID>();
            }

            float minStatusTime = float.MaxValue;
            int statusTimesCount = 0;
            foreach (float t in Player.Object.StatusTimes(false, noCastingStatusArray))
            {
                statusTimesCount++;
                if (t < minStatusTime)
                {
                    minStatusTime = t;
                }
            }

            if (statusTimesCount > 0)
            {
                float remainingCastTime = Player.Object.TotalCastTime - Player.Object.CurrentCastTime;
                if (minStatusTime > remainingCastTime && minStatusTime < 5)
                {
                    return;
                }
            }

            IAction? nextAction = ActionUpdater.NextAction;
            if (nextAction == null)
            {
                return;
            }

#if DEBUG
            // if (nextAction is BaseAction debugAct)
            //     PluginLog.Debug($"Will Do {debugAct}");
#endif

            if (nextAction is BaseAction baseAct)
            {
                if (baseAct.Target.Target is IBattleChara target && target != Player.Object && target.IsEnemy())
                {
                    DataCenter.HostileTarget = target;
                    if (!DataCenter.IsManual &&
                        (Service.Config.SwitchTargetFriendly || ((Svc.Targets.Target?.IsEnemy() ?? true)
                        || Svc.Targets.Target?.GetObjectKind() == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Treasure)))
                    {
                        Svc.Targets.Target = target;
                    }
                }
            }

            if (Service.Config.KeyBoardNoise)
            {
                MiscUpdater.PulseActionBar(nextAction.AdjustedID);
            }

            if (nextAction.Use())
            {
                if (Service.Config.EnableClickingCount)
                {
                    OtherConfiguration.RotationSolverRecord.ClickingCount++;
                }

                _lastActionID = nextAction.AdjustedID;
                _lastUsedTime = DateTime.Now;

                if (nextAction is BaseAction finalAct)
                {
                    if (Service.Config.KeyBoardNoise)
                    {
                        PulseSimulation(nextAction.AdjustedID);
                    }

                    if (finalAct.Setting.EndSpecial)
                    {
                        ResetSpecial();
                    }
                }
            }
            else if (Service.Config.InDebug)
            {
                PluginLog.Verbose($"Failed to use the action {nextAction} ({nextAction.AdjustedID})");
            }
        }

        private static void PulseSimulation(uint id)
        {
            if (started)
            {
                return;
            }

            started = true;
            try
            {
                int pulseCount = random.Next(Service.Config.KeyboardNoise.X, Service.Config.KeyboardNoise.Y);
                PulseAction(id, pulseCount);
            }
            catch (Exception ex)
            {
                PluginLog.Warning($"Pulse Failed!: {ex.Message}");
                WarningHelper.AddSystemWarning($"Action bar failed to pulse because: {ex.Message}");
            }
            finally
            {
                started = false;
            }
        }

        private static void PulseAction(uint id, int remainingPulses)
        {
            if (remainingPulses <= 0)
            {
                started = false;
                return;
            }

            MiscUpdater.PulseActionBar(id);
            double time = Service.Config.ClickingDelay.X + (random.NextDouble() * (Service.Config.ClickingDelay.Y - Service.Config.ClickingDelay.X));
            _ = Svc.Framework.RunOnTick(() =>
            {
                PulseAction(id, remainingPulses - 1);
            }, TimeSpan.FromSeconds(time));
        }

        internal static void ResetSpecial()
        {
            DoSpecialCommandType(SpecialCommandType.EndSpecial, false);
        }

        internal static void CancelState()
        {
            DataCenter.ResetAllRecords();
            if (DataCenter.State)
            {
                DoStateCommandType(StateCommandType.Off);
            }
        }

        internal static void UpdateRotationState()
        {
            try
            {
                // Avoid redundant checks for AutoCancelTime
                if (ActionUpdater.AutoCancelTime != DateTime.MinValue &&
                    (!DataCenter.State || DataCenter.InCombat))
                {
                    ActionUpdater.AutoCancelTime = DateTime.MinValue;
                }

                if (!Player.AvailableThreadSafe)
                {
                    return;
                }

                // Combine conditions to reduce redundant checks
                if (Svc.Condition[ConditionFlag.LoggingOut] ||
                    (Service.Config.AutoOffWhenDead && !(DataCenter.Territory?.IsPvP ?? false) && Player.Object.CurrentHp == 0) ||
                    (Service.Config.AutoOffWhenDeadPvP && (DataCenter.Territory?.IsPvP ?? false) && Player.Object.CurrentHp == 0) ||
                    (Service.Config.AutoOffPvPMatchEnd && Svc.Condition[ConditionFlag.PvPDisplayActive]) ||
                    (Service.Config.AutoOffCutScene && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]) ||
                    (Service.Config.AutoOffSwitchClass && Player.Job != _previousJob) ||
                    (Service.Config.AutoOffBetweenArea && (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])) ||
                    (Service.Config.CancelStateOnCombatBeforeCountdown && Service.CountDownTime > 0.2f && DataCenter.InCombat) ||
                    (ActionUpdater.AutoCancelTime != DateTime.MinValue && DateTime.Now > ActionUpdater.AutoCancelTime) ||
                    (DataCenter.CurrentConditionValue.SwitchCancelConditionSet?.IsTrue(DataCenter.CurrentRotation) ?? false))
                {
                    CancelState();
                    if (Player.Job != _previousJob)
                    {
                        _previousJob = Player.Job;
                    }

                    ActionUpdater.AutoCancelTime = DateTime.MinValue;
                    return;
                }

                // Simplify PvP match start condition
                if (Service.Config.AutoOnPvPMatchStart &&
                    Svc.Condition[ConditionFlag.BetweenAreas] &&
                    Svc.Condition[ConditionFlag.BoundByDuty] &&
                    (DataCenter.Territory?.IsPvP ?? false))
                {
                    DoStateCommandType(StateCommandType.Auto);
                    return;
                }

                if (Service.Config.StartOnAttackedBySomeone && DataCenter.AllHostileTargets.FirstOrDefault(t => t != null && t is IBattleChara battleChara && battleChara.TargetObjectId == Player.Object.GameObjectId) is IBattleChara target && !ObjectHelper.IsDummy(target))
                {
                    if (!DataCenter.State)
                    {
                        DoStateCommandType(StateCommandType.Manual);
                    }
                    return;
                }

                if (Service.Config.StartOnCountdown)
                {
                    if (Service.CountDownTime > 0)
                    {
                        _lastCountdownTime = Service.CountDownTime;
                        if (!DataCenter.State)
                        {
                            DoStateCommandType(Service.Config.CountdownStartsManualMode
                                ? StateCommandType.Manual
                                : StateCommandType.Auto);
                        }
                        return;
                    }
                    else if (Service.CountDownTime == 0 && _lastCountdownTime > 0.2f)
                    {
                        _lastCountdownTime = 0;
                        CancelState();
                        return;
                    }
                }

                // Combine manual and auto condition checks
                if (DataCenter.CurrentConditionValue.SwitchManualConditionSet?.IsTrue(DataCenter.CurrentRotation) ?? false)
                {
                    if (!DataCenter.State)
                    {
                        DoStateCommandType(StateCommandType.Manual);
                    }
                }
                else if (DataCenter.CurrentConditionValue.SwitchAutoConditionSet?.IsTrue(DataCenter.CurrentRotation) ?? false)
                {
                    if (!DataCenter.State)
                    {
                        DoStateCommandType(StateCommandType.Auto);
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Exception in UpdateRotationState: {ex.Message}");
            }
        }

    }
}
