using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;
using RotationSolver.Updaters;

namespace RotationSolver.Commands
{
    public static partial class RSCommands
    {
        static DateTime _lastClickTime = DateTime.MinValue;
        static bool _lastState;
        static bool started = false;
        internal static DateTime _lastUsedTime = DateTime.MinValue;
        internal static uint _lastActionID;
        static float _lastCountdownTime = 0;
        static Job _previousJob = Job.ADV;

        public static void IncrementState()
        {
            if (!DataCenter.State) { DoStateCommandType(StateCommandType.Auto); return; }
            if (DataCenter.State && !DataCenter.IsManual && DataCenter.TargetingType == TargetingType.Big) { DoStateCommandType(StateCommandType.Auto); return; }
            if (DataCenter.State && !DataCenter.IsManual) { DoStateCommandType(StateCommandType.Manual); return; }
            if (DataCenter.State && DataCenter.IsManual) { DoStateCommandType(StateCommandType.Off); return; }
        }

        internal static unsafe bool CanDoAnAction(bool isGCD)
        {
            if (!_lastState || !DataCenter.State)
            {
                _lastState = DataCenter.State;
                return false;
            }
            _lastState = DataCenter.State;

            if (!Player.Available) return false;

            // Do not click the button in random time.
            if (DateTime.Now - _lastClickTime < TimeSpan.FromMilliseconds(new Random().Next(
                (int)(Service.Config.ClickingDelay.X * 1000), (int)(Service.Config.ClickingDelay.Y * 1000)))) return false;
            _lastClickTime = DateTime.Now;

            if (!isGCD && ActionUpdater.NextAction is IBaseAction act1 && act1.Info.IsRealGCD) return false;

            return true;
        }

        public static void DoAction()
        {
            var statusTimes = Player.Object.StatusTimes(false, [.. OtherConfiguration.NoCastingStatus.Select(i => (StatusID)i)]);

            if (statusTimes.Any() && statusTimes.Min() > Player.Object.TotalCastTime - Player.Object.CurrentCastTime && statusTimes.Min() < 5)
            {
                return;
            }

            var nextAction = ActionUpdater.NextAction;
            if (nextAction == null) return;

#if DEBUG
            // if (nextAction is BaseAction acti)
            //     Svc.Log.Debug($"Will Do {acti}");
#endif

            if (Service.Config.KeyBoardNoise)
            {
                PreviewUpdater.PulseActionBar(nextAction.AdjustedID);
            }

            if (nextAction.Use())
            {
                OtherConfiguration.RotationSolverRecord.ClickingCount++;

                _lastActionID = nextAction.AdjustedID;
                _lastUsedTime = DateTime.Now;

                if (nextAction is BaseAction act)
                {
                    if (Service.Config.KeyBoardNoise)
                        PulseSimulation(nextAction.AdjustedID);

                    if (act.Setting.EndSpecial) ResetSpecial();
#if DEBUG
                    // Svc.Chat.Print(act.Name);

                    // if (act.Target != null)
                    // {
                    //     Svc.Chat.Print(act.Target.Value.Target?.Name.TextValue ?? string.Empty);
                    //     foreach (var item in act.Target.Value.AffectedTargets)
                    //     {
                    //         Svc.Chat.Print(item?.Name.TextValue ?? string.Empty);
                    //     }
                    // }
#endif
                    // Change Target
                    var tar = act.Target.Target == Player.Object
                        ? act.Target.AffectedTargets.FirstOrDefault() : act.Target.Target;

                    if (tar != null && tar != Player.Object && tar.IsEnemy())
                    {
                        DataCenter.HostileTarget = tar;
                        if (!DataCenter.IsManual
                            && (Service.Config.SwitchTargetFriendly || ((Svc.Targets.Target?.IsEnemy() ?? true)
                            || Svc.Targets.Target?.GetObjectKind() == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Treasure)))
                        {
                            Svc.Targets.Target = tar;
                        }
                    }
                }

            }
            else if (Service.Config.InDebug)
            {
                Svc.Log.Verbose($"Failed to use the action {nextAction} ({nextAction.AdjustedID})");
            }
        }

        static void PulseSimulation(uint id)
        {
            if (started) return;
            started = true;
            try
            {
                int pulseCount = new Random().Next((int)Service.Config.KeyboardNoise.X, (int)Service.Config.KeyboardNoise.Y);
                PulseAction(id, pulseCount);
            }
            catch (Exception ex)
            {
                Svc.Log.Warning(ex, "Pulse Failed!");
#pragma warning disable 0436
                WarningHelper.AddSystemWarning($"Action bar failed to pulse because: {ex.Message}");
            }
            finally
            {
                started = false;
            }
        }

        static void PulseAction(uint id, int remainingPulses)
        {
            if (remainingPulses <= 0)
            {
                started = false;
                return;
            }

            PreviewUpdater.PulseActionBar(id);
            var time = Service.Config.ClickingDelay.X + new Random().NextDouble() * (Service.Config.ClickingDelay.Y - Service.Config.ClickingDelay.X);
            Svc.Framework.RunOnTick(() =>
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
            if (DataCenter.State) DoStateCommandType(StateCommandType.Off);
        }

        internal static void UpdateRotationState()
        {
            if (ActionUpdater.AutoCancelTime != DateTime.MinValue &&
                (!DataCenter.State || DataCenter.InCombat))
            {
                ActionUpdater.AutoCancelTime = DateTime.MinValue;
            }

            var target = DataCenter.AllHostileTargets.FirstOrDefault(t => t.TargetObjectId == Player.Object.GameObjectId);

            if (Svc.Condition[ConditionFlag.LoggingOut])
            {
                CancelState();
            }
            else if (Service.Config.AutoOffWhenDead
                && Player.Available
                && Player.Object.CurrentHp == 0)
            {
                CancelState();
            }
            else if (Service.Config.AutoOffCutScene
                && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent])
            {
                CancelState();
            }
            else if (Service.Config.AutoOffSwitchClass
                && Player.Job != _previousJob)
            {
                _previousJob = Player.Job;
                CancelState();
            }
            else if (Service.Config.AutoOffBetweenArea
                && (Svc.Condition[ConditionFlag.BetweenAreas]
                || Svc.Condition[ConditionFlag.BetweenAreas51]))
            {
                CancelState();
            }

            // Auto manual on being attacked by someone.
            else if (Service.Config.StartOnAttackedBySomeone
                && target != null
                && !target.IsDummy())
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Manual);
                }
            }

            // Auto start at count down.
            else if (Service.Config.StartOnCountdown
                && Service.CountDownTime > 0)
            {
                _lastCountdownTime = Service.CountDownTime;
                if (!DataCenter.State)
                {
                    if (Service.Config.CountdownStartsManualMode)
                    {
                        DoStateCommandType(StateCommandType.Manual);
                    }
                    else
                    {
                        DoStateCommandType(StateCommandType.Auto);
                    }
                }
            }

            // Holds state if countdown is running and setting is enabled
            else if (Service.Config.StartOnCountdown && Service.CountDownTime == 0 && _lastCountdownTime > 0.2f)
            {
                _lastCountdownTime = 0;
                CancelState();
            }

            // Cancel when after combat.
            else if (ActionUpdater.AutoCancelTime != DateTime.MinValue
                && DateTime.Now > ActionUpdater.AutoCancelTime)
            {
                CancelState();
                ActionUpdater.AutoCancelTime = DateTime.MinValue;
            }

            // Auto switch conditions.
            else if (DataCenter.RightSet.SwitchCancelConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                CancelState();
            }
            else if (DataCenter.RightSet.SwitchManualConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Manual);
                }
            }
            else if (DataCenter.RightSet.SwitchAutoConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Auto);
                }
            }
        }

    }
}