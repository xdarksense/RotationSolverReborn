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
            {
                return false;
            }

            // Directly check if there are any conditions present
            IReadOnlySet<ConditionFlag> conditions = Svc.Condition.AsReadOnlySet();
            return conditions.Count != 0 && !Svc.Condition[ConditionFlag.Occupied]
               && !Svc.Condition[ConditionFlag.LoggingOut]
               && !Svc.Condition[ConditionFlag.Occupied30]
               && !Svc.Condition[ConditionFlag.Occupied33]
               && !Svc.Condition[ConditionFlag.Occupied38]
               && !Svc.Condition[ConditionFlag.Occupied39]
               && !Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
               && !Svc.Condition[ConditionFlag.OccupiedInEvent]
               && !Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
               && !Svc.Condition[ConditionFlag.OccupiedSummoningBell]
               && !Svc.Condition[ConditionFlag.WatchingCutscene]
               && !Svc.Condition[ConditionFlag.WatchingCutscene78]
               && !Svc.Condition[ConditionFlag.BetweenAreas]
               && !Svc.Condition[ConditionFlag.BetweenAreas51]
               && !Svc.Condition[ConditionFlag.InThatPosition]
               //|| Svc.Condition[ConditionFlag.TradeOpen]
               && !Svc.Condition[ConditionFlag.Crafting]
               && !Svc.Condition[ConditionFlag.ExecutingCraftingAction]
               && !Svc.Condition[ConditionFlag.PreparingToCraft]
               && !Svc.Condition[ConditionFlag.Unconscious]
               && !Svc.Condition[ConditionFlag.MeldingMateria]
               && !Svc.Condition[ConditionFlag.Gathering]
               && !Svc.Condition[ConditionFlag.OperatingSiegeMachine]
               && !Svc.Condition[ConditionFlag.CarryingItem]
               && !Svc.Condition[ConditionFlag.CarryingObject]
               && !Svc.Condition[ConditionFlag.BeingMoved]
               && !Svc.Condition[ConditionFlag.Mounted]
               && !Svc.Condition[ConditionFlag.Mounted2]
               && !Svc.Condition[ConditionFlag.Mounting]
               && !Svc.Condition[ConditionFlag.Mounting71]
               && !Svc.Condition[ConditionFlag.ParticipatingInCustomMatch]
               && !Svc.Condition[ConditionFlag.PlayingLordOfVerminion]
               && !Svc.Condition[ConditionFlag.ChocoboRacing]
               && !Svc.Condition[ConditionFlag.PlayingMiniGame]
               && !Svc.Condition[ConditionFlag.Performing]
               && !Svc.Condition[ConditionFlag.Fishing]
               //&& !Svc.Condition[ConditionFlag.Transformed] Dhon Meg boss enlarges you, making you transformed
               && !Svc.Condition[ConditionFlag.UsingHousingFunctions]
               && !Svc.Condition[ConditionFlag.Jumping61]
               && !Svc.Condition[ConditionFlag.SufferingStatusAffliction2]
               && !Svc.Condition[ConditionFlag.RolePlaying]
               && !Svc.Condition[ConditionFlag.InFlight]
               && !Svc.Condition[ConditionFlag.Diving]
               && !Svc.Condition[ConditionFlag.Swimming];
        }
    }

    private static Exception? _threadException;

    public static void Enable()
    {
        ActionSequencerUpdater.Enable(Svc.PluginInterface.ConfigDirectory.FullName + "\\Conditions");
        Svc.Framework.Update += RSRTeachingModeUpdate;
        Svc.Framework.Update += RSRInvalidUpdate;
        Svc.Framework.Update += RSRUpdateMoving;
        Svc.Framework.Update += RSRUpdateAction;
        Svc.Framework.Update += RSRUpdateMacro;
        Svc.Framework.Update += RSRUpdateTarget;
        Svc.Framework.Update += RSRUpdateState;
        Svc.Framework.Update += RSRUpdateActionSequencer;
        Svc.Framework.Update += RSRUpdateNextAction;
        Svc.Framework.Update += RSRUpdateHighlight;
        Svc.Framework.Update += RSRUpdateCombatInfo;
        Svc.Framework.Update += RSRUpdateDisplayWindow;
        Svc.Framework.Update += RSRUpdateSystemWarnings;
        Svc.Framework.Update += RSRUpdateVfxDataQueue;
        Svc.Framework.Update += RSRUpdateLocalRotationWatcher;
        Svc.Framework.Update += RSRUpdateRotation;
        Svc.Framework.Update += RSRUpdateRotationState;
        Svc.Framework.Update += RSRUpdateTeachingModeSettings;
        Svc.Framework.Update += RSRUpdateMisc;
    }

    private static void RSRTeachingModeUpdate(IFramework framework)
    {
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
    }

    private static void RSRInvalidUpdate(IFramework framework)
    {
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
                    PluginLog.Error($"RSRUpdate TSC Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdate TSC Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateMoving(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            if (Service.Config.PoslockCasting && ActionUpdater.CanDoAction() is not false)
            {
                try
                {
                    MovingUpdater.UpdateCanMove(ActionUpdater.CanDoAction());
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        PluginLog.Error($"MovingUpdater Exception: {ex.Message}");
                        if (Service.Config.InDebug)
                        {
                            _ = BasicWarningHelper.AddSystemWarning("MovingUpdater Exception");
                        }
                    }
                }
            }
        }
    }

    private static void RSRUpdateAction(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                if (ActionUpdater.CanDoAction())
                {
                    RSCommands.DoAction();
                }
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSCommands.DoAction Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSCommands.DoAction Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateMacro(IFramework framework)
    {
        try
        {
            MacroUpdater.UpdateMacro();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"MacroUpdater.UpdateMacro Exception: {ex.Message}");
                if (Service.Config.InDebug)
                {
                    _ = BasicWarningHelper.AddSystemWarning("MacroUpdater.UpdateMacro Exception");
                }
            }
        }
    }

    private static void RSRUpdateTarget(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                TargetUpdater.UpdateTargets();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"TargetUpdater.UpdateTargets Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("TargetUpdater.UpdateTargets Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateState(IFramework framework)
    {
        try
        {
            StateUpdater.UpdateState();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"StateUpdater.UpdateState Exception: {ex.Message}");
                if (Service.Config.InDebug)
                {
                    _ = BasicWarningHelper.AddSystemWarning("StateUpdater.UpdateState Exception");
                }
            }
        }
    }

    private static void RSRUpdateActionSequencer(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                ActionSequencerUpdater.UpdateActionSequencerAction();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"ActionSequencerUpdater.UpdateActionSequencerAction Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("ActionSequencerUpdater.UpdateActionSequencerAction Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateNextAction(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                ActionUpdater.UpdateNextAction();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"ActionUpdater.UpdateNextAction Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("ActionUpdater.UpdateNextAction Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateHighlight(IFramework framework)
    {
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

    private static void RSRUpdateCombatInfo(IFramework framework)
    {
        // Update various combat tracking parameters
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                ActionUpdater.UpdateCombatInfo();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"UpdateCombatInfo Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("UpdateCombatInfo Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateDisplayWindow(IFramework framework)
    {
        try
        {
            RotationSolverPlugin.UpdateDisplayWindow();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"RSRUpdateDisplayWindow Exception: {ex.Message}");
                if (Service.Config.InDebug)
                {
                    _ = BasicWarningHelper.AddSystemWarning("RSRUpdateDisplayWindow Exception");
                }
            }
        }
    }

    private static void RSRUpdateSystemWarnings(IFramework framework)
    {
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
    }

    private static void RSRUpdateVfxDataQueue(IFramework framework)
    {
        // Clear old VFX data
        if (DataCenter.VfxDataQueue.Count > 0)
        {
            _ = DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));
        }
    }

    private static void RSRUpdateLocalRotationWatcher(IFramework framework)
    {
        // Check local rotation files
        if (Service.Config.AutoReloadRotations)
        {
            RotationUpdater.LocalRotationWatcher();
        }
    }

    private static void RSRUpdateRotation(IFramework framework)
    {
        // Change loaded rotation based on job
        RotationUpdater.UpdateRotation();
    }

    private static void RSRUpdateRotationState(IFramework framework)
    {
        // Change RS state
        RSCommands.UpdateRotationState();
    }

    private static void RSRUpdateTeachingModeSettings(IFramework framework)
    {
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
    }

    private static void RSRUpdateMisc(IFramework framework)
    {
        if (Service.Config.TeachingMode || DataCenter.IsActivated())
        {
            try
            {
                MiscUpdater.UpdateMisc();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"MiscUpdater.UpdateEntry Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("MiscUpdater.UpdateEntry Exception");
                    }
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
        Svc.Framework.Update -= RSRTeachingModeUpdate;
        Svc.Framework.Update -= RSRInvalidUpdate;
        Svc.Framework.Update -= RSRUpdateMoving;
        Svc.Framework.Update -= RSRUpdateAction;
        Svc.Framework.Update -= RSRUpdateMacro;
        Svc.Framework.Update -= RSRUpdateTarget;
        Svc.Framework.Update -= RSRUpdateState;
        Svc.Framework.Update -= RSRUpdateActionSequencer;
        Svc.Framework.Update -= RSRUpdateNextAction;
        Svc.Framework.Update -= RSRUpdateHighlight;
        Svc.Framework.Update -= RSRUpdateCombatInfo;
        Svc.Framework.Update -= RSRUpdateDisplayWindow;
        Svc.Framework.Update -= RSRUpdateSystemWarnings;
        Svc.Framework.Update -= RSRUpdateVfxDataQueue;
        Svc.Framework.Update -= RSRUpdateLocalRotationWatcher;
        Svc.Framework.Update -= RSRUpdateRotation;
        Svc.Framework.Update -= RSRUpdateRotationState;
        Svc.Framework.Update -= RSRUpdateTeachingModeSettings;
        Svc.Framework.Update -= RSRUpdateMisc;
        MiscUpdater.Dispose();
        ActionSequencerUpdater.SaveFiles();
        ActionUpdater.ClearNextAction();
    }
}