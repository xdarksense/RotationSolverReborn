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

            if (Svc.Condition[ConditionFlag.Occupied]
               || Svc.Condition[ConditionFlag.LoggingOut]
               || Svc.Condition[ConditionFlag.Occupied30]
               || Svc.Condition[ConditionFlag.Occupied33]
               || Svc.Condition[ConditionFlag.Occupied38]
               || Svc.Condition[ConditionFlag.Occupied39]
               || Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
               || Svc.Condition[ConditionFlag.OccupiedInEvent]
               || Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
               || Svc.Condition[ConditionFlag.OccupiedSummoningBell]
               || Svc.Condition[ConditionFlag.WatchingCutscene]
               || Svc.Condition[ConditionFlag.WatchingCutscene78]
               || Svc.Condition[ConditionFlag.BetweenAreas]
               || Svc.Condition[ConditionFlag.BetweenAreas51]
               || Svc.Condition[ConditionFlag.InThatPosition]
               //|| Svc.Condition[ConditionFlag.TradeOpen]
               || Svc.Condition[ConditionFlag.Crafting]
               || Svc.Condition[ConditionFlag.ExecutingCraftingAction]
               || Svc.Condition[ConditionFlag.PreparingToCraft]
               || Svc.Condition[ConditionFlag.Unconscious]
               || Svc.Condition[ConditionFlag.MeldingMateria]
               || Svc.Condition[ConditionFlag.Gathering]
               || Svc.Condition[ConditionFlag.OperatingSiegeMachine]
               || Svc.Condition[ConditionFlag.CarryingItem]
               || Svc.Condition[ConditionFlag.CarryingObject]
               || Svc.Condition[ConditionFlag.BeingMoved]
               || Svc.Condition[ConditionFlag.Mounted]
               || Svc.Condition[ConditionFlag.Mounted2]
               || Svc.Condition[ConditionFlag.Mounting]
               || Svc.Condition[ConditionFlag.Mounting71]
               || Svc.Condition[ConditionFlag.ParticipatingInCustomMatch]
               || Svc.Condition[ConditionFlag.PlayingLordOfVerminion]
               || Svc.Condition[ConditionFlag.ChocoboRacing]
               || Svc.Condition[ConditionFlag.PlayingMiniGame]
               || Svc.Condition[ConditionFlag.Performing]
               || Svc.Condition[ConditionFlag.Fishing]
               || Svc.Condition[ConditionFlag.Transformed]
               || Svc.Condition[ConditionFlag.UsingHousingFunctions]
               || Svc.Condition[ConditionFlag.Jumping61]
               || Svc.Condition[ConditionFlag.SufferingStatusAffliction2]
               || Svc.Condition[ConditionFlag.RolePlaying]
               || Svc.Condition[ConditionFlag.InFlight]
               || Svc.Condition[ConditionFlag.Diving]
               || Svc.Condition[ConditionFlag.Swimming])
                {
                    return true;
                }

            return true;
        }
    }

    private static Exception? _threadException;

    public static void Enable()
    {
        ActionSequencerUpdater.Enable(Svc.PluginInterface.ConfigDirectory.FullName + "\\Conditions");
        Svc.Framework.Update += RSRUpdate;
    }

    private unsafe static void RSRUpdate(IFramework framework)
    {
        HotbarHighlightManager.HotbarIDs.Clear();

        // Transistion safe commands
        if (!IsValid)
        {
            try
            {
                RSCommands.UpdateRotationState();
                ActionUpdater.ClearNextAction();
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
                        BasicWarningHelper.AddSystemWarning("RSRUpdate TSC Exception");
                }
            }
        }

        if (DataCenter.IsActivated())
        {
            try
            {
                var canDoAction = ActionUpdater.CanDoAction();
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
                        BasicWarningHelper.AddSystemWarning("RSRUpdate DC Exception");
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

            // Clear old VFX data
            if (DataCenter.VfxDataQueue.Count > 0)
                DataCenter.VfxDataQueue.RemoveAll(vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));

            // Check local rotation files
            if (Service.Config.AutoReloadRotations)
            {
                RotationUpdater.LocalRotationWatcher();
            }

            // Change loaded rotation based on job
            RotationUpdater.UpdateRotation();

            // Change RS state
            RSCommands.UpdateRotationState();

            HotbarHighlightManager.UpdateSettings();
            MiscUpdater.UpdateMisc();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                PluginLog.Error($"Secondary RSRUpdate Exception: {ex.Message}");
                if (Service.Config.InDebug)
                    BasicWarningHelper.AddSystemWarning("Secondary RSRUpdate Exception");
            }
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