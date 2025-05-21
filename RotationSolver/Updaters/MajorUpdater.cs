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
    public static bool IsValid
    {
        get
        {
            if (!Player.AvailableThreadSafe)
                return false;

            // Directly check if there are any conditions present
            var conditions = Svc.Condition.AsReadOnlySet();
            if (conditions.Count == 0)
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

    public static void TransitionSafeCommands()
    {
        RSCommands.UpdateRotationState();
        ActionUpdater.UpdateLifetime();
    }

    private unsafe static void RSRUpdate(IFramework framework)
    {
        HotbarHighlightManager.HotbarIDs.Clear();
        RotationSolverPlugin.UpdateDisplayWindow();

        if (!IsValid)
        {
            TransitionSafeCommands();
            ActionUpdater.ClearNextAction();
            CustomRotation.MoveTarget = null;
            ActionUpdater.NextAction = ActionUpdater.NextGCDAction = null;
            return;
        }

        // Handle system warnings
        if (DataCenter.SystemWarnings.Count > 0)
        {
            var now = DateTime.Now;
            var keysToRemove = new List<string>();

            foreach (var kvp in DataCenter.SystemWarnings)
            {
                if (kvp.Value + TimeSpan.FromMinutes(10) < now)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                DataCenter.SystemWarnings.Remove(key);
            }
        }

        if (DataCenter.IsActivated() && Player.Object != null && Player.Available)
        {
            try
            {
                TargetUpdater.UpdateTargets();
                StateUpdater.UpdateState();
                ActionUpdater.UpdateActionInfo();
                ActionUpdater.UpdateNextAction();
                var canDoAction = ActionUpdater.CanDoAction();
                MovingUpdater.UpdateCanMove(canDoAction);

                if (canDoAction)
                {
                    RSCommands.DoAction();
                }

                MacroUpdater.UpdateMacro();
                ActionSequencerUpdater.UpdateActionSequencerAction();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"Main Thread Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                        BasicWarningHelper.AddSystemWarning("Main Thread Exception");
                }
            }

            // Handle Teaching Mode Highlighting
            if (Service.Config.TeachingMode && ActionUpdater.NextAction is not null)
            {
                try
                {
                    var nextAction = ActionUpdater.NextAction;
                    HotbarID? hotbar = null;
                    if (nextAction is IBaseItem item)
                    {
                        hotbar = new HotbarID(HotbarSlotType.Item, item.ID);
                    }
                    else if (nextAction is IBaseAction baseAction)
                    {
                        if (baseAction.Action.ActionCategory.RowId == 10 || baseAction.Action.ActionCategory.RowId == 11)
                        {
                            hotbar = GetGeneralActionHotbarID(baseAction);
                        }
                        else
                        {
                            hotbar = new HotbarID(HotbarSlotType.Action, baseAction.AdjustedID);
                        }
                    }

                    if (hotbar.HasValue)
                    {
                        HotbarHighlightManager.HotbarIDs.Add(hotbar.Value);
                    }
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        PluginLog.Error($"UpdateHighlight Exception: {ex.Message}");
                        if (Service.Config.InDebug)
                            BasicWarningHelper.AddSystemWarning("UpdateHighlight Exception");
                    }
                }
            }
        }

        // Handle Miscellaneous Updates
        try
        {
            MiscUpdater.UpdateMisc();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"UpdatePreview Exception: {ex.Message}");
                if (Service.Config.InDebug)
                    BasicWarningHelper.AddSystemWarning("UpdatePreview Exception");
            }
        }

        // Handle Update Work in a separate try-catch block to avoid blocking the main thread
        try
        {
            if (DataCenter.VfxDataQueue.Count > 0)
                DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));

            if (Service.Config.AutoReloadRotations)
            {
                RotationUpdater.LocalRotationWatcher();
            }

            RSCommands.UpdateRotationState();
            HotbarHighlightManager.UpdateSettings();
            RotationUpdater.UpdateRotation();
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Inner Worker Exception: {ex.Message}");
            if (Service.Config.InDebug)
                WarningHelper.AddSystemWarning("Inner Worker Exception");
        }
    }

    private static HotbarID? GetGeneralActionHotbarID(IBaseAction baseAction)
    {
        var generalActions = Svc.Data.GetExcelSheet<GeneralAction>();
        if (generalActions == null) return null;

        foreach (var gAct in generalActions)
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