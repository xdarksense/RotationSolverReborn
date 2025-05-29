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
                    PluginLog.Error($"RSRTeachingModeUpdate Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRTeachingModeUpdate Exception");
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
                    PluginLog.Error($"RSRInvalidUpdate Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRInvalidUpdate Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateMoving(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                        PluginLog.Error($"RSRUpdateMoving Exception: {ex.Message}");
                        if (Service.Config.InDebug)
                        {
                            _ = BasicWarningHelper.AddSystemWarning("RSRUpdateMoving Exception");
                        }
                    }
                }
            }
        }
    }

    private static void RSRUpdateAction(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateAction Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateAction Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateMacro(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateMacro Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateMacro Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateTarget(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateTarget Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateTarget Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateState(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateState Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateState Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateActionSequencer(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateActionSequencer Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateActionSequencer Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateNextAction(IFramework framework)
    {
        if (DataCenter.IsActivated())
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
                    PluginLog.Error($"RSRUpdateNextAction Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateNextAction Exception");
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
                    PluginLog.Error($"RSRUpdateHighlight Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateHighlight Exception");
                    }
                }
            }
        }
    }



    private static void RSRUpdateCombatInfo(IFramework framework)
    {
        // Update various combat tracking parameters
        try
        {
            ActionUpdater.UpdateCombatInfo();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"RSRUpdateCombatInfo Exception: {ex.Message}");
                if (Service.Config.InDebug)
                {
                    _ = BasicWarningHelper.AddSystemWarning("RSRUpdateCombatInfo Exception");
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
            try
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
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdateSystemWarnings Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateSystemWarnings Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateVfxDataQueue(IFramework framework)
    {
        // Clear old VFX data
        if (DataCenter.VfxDataQueue.Count > 0)
        {
            try
            {
                _ = DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdateVfxDataQueue Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateVfxDataQueue Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateLocalRotationWatcher(IFramework framework)
    {
        // Check local rotation files
        if (Service.Config.AutoReloadRotations && IsValid)
        {
            try
            {
                RotationUpdater.LocalRotationWatcher();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdateLocalRotationWatcher Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateLocalRotationWatcher Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateRotation(IFramework framework)
    {
        // Change loaded rotation based on job
        if (IsValid)
        {
            try
            {
                RotationUpdater.UpdateRotation();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdateRotation Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateRotation Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateRotationState(IFramework framework)
    {
        // Change RS state
        if (IsValid)
        {
            try
            {
                RSCommands.UpdateRotationState();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    PluginLog.Error($"RSRUpdateRotationState Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateRotationState Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateTeachingModeSettings(IFramework framework)
    {
        if (Service.Config.TeachingMode && IsValid)
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
                    PluginLog.Error($"RSRUpdateTeachingModeSettings Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateTeachingModeSettings Exception");
                    }
                }
            }
        }
    }

    private static void RSRUpdateMisc(IFramework framework)
    {

        if (IsValid)
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
                    PluginLog.Error($"RSRUpdateMisc Exception: {ex.Message}");
                    if (Service.Config.InDebug)
                    {
                        _ = BasicWarningHelper.AddSystemWarning("RSRUpdateMisc Exception");
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