using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using RotationSolver.Commands;
using RotationSolver.UI.HighlightTeachingMode;
using System.Diagnostics;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace RotationSolver.Updaters;

internal static class MajorUpdater
{
    private static TimeSpan _timeSinceUpdate = TimeSpan.Zero;
    private static List<long> curTicks = [];
    public static List<List<long>> Ticks = [[], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []];
    public static List<string> Ticknames = [ "teaching mode", "isValid check", "UpdateRotationState", "AU ClearNextAction", "MU Update1", "DC Active", "AU CanDo", "Update Can Move", "RS DoAction", "Update Macro",
                                              "Update Targets", "Update State", "Update Sequencer", "AU Update Next Action", "Update Combat Info", "Update Display Windows", "Handle Warnings", "Clear VFX",
                                              "Check Local Rotations", "Update Rotations", "Update Rotation State", "MU Update2"];
    private static void UpdateCounter(long entry, int index, bool end = false)
    {
        long toAdd = entry - curTicks.LastOrDefault(0);
        curTicks.Add(entry);

        Ticks[index].Add(toAdd);

        if (end)
        {
            curTicks.Clear();
        }
    }

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
                if (conditionFlag == ConditionFlag.BetweenAreas || conditionFlag == ConditionFlag.BetweenAreas51 || conditionFlag == ConditionFlag.LoggingOut)
                {
                    return false;
                }
                anyCondition = true; // foreach doesn't execute on an empty enumerable, so this will only be true if there are conditions
            }

            return anyCondition;
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

        var _sw = new Stopwatch();
        _sw.Start();

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
        UpdateCounter(_sw.ElapsedTicks, 0);

        // Transistion safe commands
        if (!IsValid)
        {
            try
            {
                UpdateCounter(_sw.ElapsedTicks, 1);
                RSCommands.UpdateRotationState();
                UpdateCounter(_sw.ElapsedTicks, 2);
                ActionUpdater.ClearNextAction();
                UpdateCounter(_sw.ElapsedTicks, 3);
                MiscUpdater.UpdateEntry();
                UpdateCounter(_sw.ElapsedTicks, 4);
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
        UpdateCounter(_sw.ElapsedTicks, 1);

        if (DataCenter.IsActivated())
        {
            UpdateCounter(_sw.ElapsedTicks, 5);
            try
            {
                bool canDoAction = ActionUpdater.CanDoAction();
                UpdateCounter(_sw.ElapsedTicks, 6);
                MovingUpdater.UpdateCanMove(canDoAction);
                UpdateCounter(_sw.ElapsedTicks, 7);

                if (canDoAction)
                {
                    RSCommands.DoAction();
                    UpdateCounter(_sw.ElapsedTicks, 8);
                }

                MacroUpdater.UpdateMacro();
                UpdateCounter(_sw.ElapsedTicks, 9);
                TargetUpdater.UpdateTargets();
                UpdateCounter(_sw.ElapsedTicks, 10);
                StateUpdater.UpdateState();
                UpdateCounter(_sw.ElapsedTicks, 11);
                ActionSequencerUpdater.UpdateActionSequencerAction();
                UpdateCounter(_sw.ElapsedTicks, 12);
                ActionUpdater.UpdateNextAction();
                UpdateCounter(_sw.ElapsedTicks, 13);
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
            UpdateCounter(_sw.ElapsedTicks, 14);

            // Update displaying the additional UI windows
            RotationSolverPlugin.UpdateDisplayWindow();
            UpdateCounter(_sw.ElapsedTicks, 15);

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
            UpdateCounter(_sw.ElapsedTicks, 16);

            // Clear old VFX data
            if (DataCenter.VfxDataQueue.Count > 0)
            {
                _ = DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));
            }
            UpdateCounter(_sw.ElapsedTicks, 17);

            // Check local rotation files
            if (Service.Config.AutoReloadRotations)
            {
                RotationUpdater.LocalRotationWatcher();
            }
            UpdateCounter(_sw.ElapsedTicks, 18);

            // Change loaded rotation based on job
            RotationUpdater.UpdateRotation();
            UpdateCounter(_sw.ElapsedTicks, 19);

            // Change RS state
            RSCommands.UpdateRotationState();
            UpdateCounter(_sw.ElapsedTicks, 20);

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
            _sw.Stop();
            UpdateCounter(_sw.ElapsedTicks, 21, true);
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