using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using RotationSolver.Commands;
using RotationSolver.UI.HighlightTeachingMode;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace RotationSolver.Updaters;

internal static class MajorUpdater
{
    private static TimeSpan _timeSinceUpdate = TimeSpan.Zero;
    public static bool IsValid
    {
        get
        {
            if (!Player.AvailableThreadSafe)
                return false;

            // Replace Svc.Condition.Any() with manual check
            bool anyCondition = false;
            foreach (var conditionFlag in Svc.Condition.AsReadOnlySet())
            {
                if (Svc.Condition.AsReadOnlySet().Contains(conditionFlag))
                {
                    anyCondition = true;
                    break;
                }
            }
            if (!anyCondition)
                return false;

            if (Svc.Condition[ConditionFlag.BetweenAreas])
                return false;
            if (Svc.Condition[ConditionFlag.BetweenAreas51])
                return false;
            if (Svc.Condition[ConditionFlag.LoggingOut])
                return false;

            return true;
        }
    }

    private static Exception? _threadException;

    public static void Enable()
    {
        ActionSequencerUpdater.Enable(Svc.PluginInterface.ConfigDirectory.FullName + "\\Conditions");
        Svc.Framework.Update += RSRUpdate;
    }
    private static void RSRUpdate(IFramework framework)
    {
        _timeSinceUpdate += framework.UpdateDelta;
        if (_timeSinceUpdate < TimeSpan.FromSeconds(Service.Config.MinUpdatingTime))
        {
            return;
        }
        else
        {
            _timeSinceUpdate = TimeSpan.Zero;
        }

        if (Service.Config.TeachingMode)
        {
            try
            {
                HotbarHighlightManager.HotbarIDs.Clear();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"HotbarHighlightManager.HotbarIDs.Clear Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("HotbarHighlightManager.HotbarIDs.Clear Exception");
                    }
                }
            }
        }

        // Transistion safe commands
        if (!IsValid)
        {
            try
            {
                RSCommands.UpdateRotationState();
                ActionUpdater.ClearNextAction();
                MiscUpdater.UpdateEntry();
                CustomRotation.MoveTarget = null;
                ActionUpdater.NextAction = ActionUpdater.NextGCDAction = null;
                return;
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRInvalidUpdate Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRInvalidUpdate Exception");
                    }
                }
            }
        }

        if (DataCenter.IsActivated())
        {
            try
            {
                bool canDoAction = ActionUpdater.CanDoAction();
                MovingUpdater.UpdateCanMove(canDoAction);

                if (canDoAction)
                {
                    RSCommands.DoAction();
                }

                MacroUpdater.UpdateMacro();
                TargetUpdater.UpdateTargets();
                StateUpdater.UpdateState();
                ActionSequencerUpdater.UpdateActionSequencerAction();
                ActionUpdater.UpdateNextAction();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdate DC Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdate DC Exception");
                    }
                }
            }

            // Handle Teaching Mode Highlighting
            if (Service.Config.TeachingMode && ActionUpdater.NextAction is not null)
            {
                try
                {
                    IAction nextAction = ActionUpdater.NextAction;
                    HotbarID? hotbar = null;
                    if (nextAction is IBaseItem item)
                    {
                        hotbar = new HotbarID(HotbarSlotType.Item, item.ID);
                    }
                    else if (nextAction is IBaseAction baseAction)
                    {
                        hotbar = baseAction.Action.ActionCategory.RowId is 10 or 11
                                ? GetGeneralActionHotbarID(baseAction)
                                : new HotbarID(HotbarSlotType.Action, baseAction.AdjustedID);
                    }

                    if (hotbar.HasValue)
                    {
                        _ = HotbarHighlightManager.HotbarIDs.Add(hotbar.Value);
                    }
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        PluginLog.Error($"UpdateHighlight Exception: {ex.Message}");
                        if (Service.Config.InDebug)
                        {
                            _ = BasicWarningHelper.AddSystemWarning("UpdateHighlight Exception");
                        }
                    }
                }
            }
        }

        try
        {
            // Update various combat tracking perameters,
            // combat time, blue mage/dutyaction slot info, player movement time, player dead status and MP timer.
            ActionUpdater.UpdateCombatInfo();

            // Update displaying the additional UI windows
            RotationSolverPlugin.UpdateDisplayWindow();

            // Handle system warnings
            if (DataCenter.SystemWarnings.Count > 0)
            {
                DateTime now = DateTime.Now;
                List<string> keysToRemove = [];

                foreach (KeyValuePair<string, DateTime> kvp in DataCenter.SystemWarnings)
                {
                    if (kvp.Value + TimeSpan.FromMinutes(10) < now)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    _ = DataCenter.SystemWarnings.Remove(key);
                }
            }

            // Clear old VFX data
            if (DataCenter.VfxDataQueue.Count > 0)
            {
                _ = DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));
            }

            // Check local rotation files
            if (Service.Config.AutoReloadRotations)
            {
                RotationUpdater.LocalRotationWatcher();
            }

            // Change loaded rotation based on job
            RotationUpdater.UpdateRotation();

            // Change RS state
            RSCommands.UpdateRotationState();

            if (Service.Config.TeachingMode)
            {
                try
                {
                    HotbarHighlightManager.UpdateSettings();
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        PluginLog.Error($"HotbarHighlightManager.UpdateSettings Exception: {ex.Message}");
                        if (Service.Config.InDebug)
                        {
                            _ = BasicWarningHelper.AddSystemWarning("HotbarHighlightManager.UpdateSettings Exception");
                        }
                    }
                }
            }

            MiscUpdater.UpdateMisc();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"Secondary RSRUpdate Exception: {ex.Message}");
                if (Service.Config.InDebug)
                {
                    _ = BasicWarningHelper.AddSystemWarning("Secondary RSRUpdate Exception");
                }
            }
        }
    }

    private static HotbarID? GetGeneralActionHotbarID(IBaseAction baseAction)
    {
        Lumina.Excel.ExcelSheet<GeneralAction> generalActions = Svc.Data.GetExcelSheet<GeneralAction>();
        if (generalActions == null)
        {
            return null;
        }

        foreach (GeneralAction gAct in generalActions)
        {
            if (gAct.Action.RowId == baseAction.ID)
            {
                return new HotbarID(HotbarSlotType.GeneralAction, gAct.RowId);
            }
        }

        return null;
    }

    public static void Dispose()
    {
        Svc.Framework.Update -= RSRUpdate;
        MiscUpdater.Dispose();
        ActionSequencerUpdater.SaveFiles();
        ActionUpdater.ClearNextAction();
    }
}