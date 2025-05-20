using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using RotationSolver.Commands;
using RotationSolver.Data;
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
    private static DateTime _lastUpdatedWork = DateTime.Now;
    private static DateTime _warningsLastDisplayed = DateTime.MinValue;

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
            return;
        }

        HandleSystemWarnings();

        if (DataCenter.IsActivated())
        {
            try
            {
                ActionUpdater.UpdateActionInfo();
                var canDoAction = ActionUpdater.CanDoAction();
                MovingUpdater.UpdateCanMove(canDoAction);

                if (canDoAction)
                {
                    RSCommands.DoAction();
                }

                MacroUpdater.UpdateMacro();
                CloseWindow();
            }
            catch (Exception ex)
            {
                if (_threadException != ex)
                {
                    _threadException = ex;
                    Svc.Log.Error(ex, "Main Thread Exception");
                    if (Service.Config.InDebug)
                        BasicWarningHelper.AddSystemWarning("Main Thread Exception");
                }
            }

            if (Service.Config.TeachingMode && ActionUpdater.NextAction is not null)
            {
                try
                {
                    UpdateHighlight();
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        Svc.Log.Error(ex, "UpdateHighlight Exception");
                        if (Service.Config.InDebug)
                            BasicWarningHelper.AddSystemWarning("UpdateHighlight Exception");
                    }
                }
            }

            if (Service.Config.AutoOpenChest)
            {
                try
                {
                    OpenChest();
                }
                catch (Exception ex)
                {
                    if (_threadException != ex)
                    {
                        _threadException = ex;
                        Svc.Log.Error(ex, "OpenChest Exception");
                        if (Service.Config.InDebug)
                            BasicWarningHelper.AddSystemWarning("OpenChest Exception");
                    }
                }
            }
        }

        try
        {
            PreviewUpdater.UpdatePreview();
        }
        catch (Exception ex)
        {
            if (_threadException != ex)
            {
                _threadException = ex;
                Svc.Log.Error(ex, "UpdatePreview Exception");
                if (Service.Config.InDebug)
                BasicWarningHelper.AddSystemWarning("UpdatePreview Exception");
            }
        }

        HandleWorkUpdate();
    }

    private static void HandleSystemWarnings()
    {
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
    }

    private static void HandleWorkUpdate()
    {
        try
        {
            Svc.Framework.RunOnTick(UpdateWork);
        }
        catch (Exception tEx)
        {
            Svc.Log.Error(tEx, "Worker Task Exception");
            if (Service.Config.InDebug)
            WarningHelper.AddSystemWarning("Inner Worker Exception");
        }
    }

    private static void UpdateWork()
    {
        if (!IsValid)
        {
            ActionUpdater.NextAction = ActionUpdater.NextGCDAction = null;
            return;
        }

        try
        {
            if (DataCenter.VfxDataQueue.Count > 0)
                RemoveExpiredVfxData();

            bool autoReloadRotations = Service.Config.AutoReloadRotations;
            if (autoReloadRotations)
            {
                RotationUpdater.LocalRotationWatcher();
            }

            RotationUpdater.UpdateRotation();

            bool isActivated = DataCenter.IsActivated();
            if (isActivated)
            {
                TargetUpdater.UpdateTargets();
                StateUpdater.UpdateState();
                ActionSequencerUpdater.UpdateActionSequencerAction();
                ActionUpdater.UpdateNextAction();
            }

            RSCommands.UpdateRotationState();
            HotbarHighlightManager.UpdateSettings();
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Inner Worker Exception");
            if (Service.Config.InDebug)
            WarningHelper.AddSystemWarning("Inner Worker Exception");
        }
    }

    private static void RemoveExpiredVfxData()
    {
        DataCenter.VfxDataQueue.RemoveAll(
                vfx => vfx.TimeDuration > TimeSpan.FromSeconds(6));
    }

    private static void UpdateHighlight()
    {
        if (!Service.Config.TeachingMode || ActionUpdater.NextAction is not IAction nextAction) return;

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

    private static void ShowWarning()
    {
        bool foundAvarice = false;
        foreach (var p in Svc.PluginInterface.InstalledPlugins)
        {
            if (p.InternalName == "Avarice")
            {
                foundAvarice = true;
                break;
            }
        }
        if (!foundAvarice)
        {
            WarningHelper.AddSystemWarning(UiString.AvariceWarning.GetDescription());
        }

        bool foundTextToTalk = false;
        foreach (var p in Svc.PluginInterface.InstalledPlugins)
        {
            if (p.InternalName == "TextToTalk")
            {
                foundTextToTalk = true;
                break;
            }
        }
        if (!foundTextToTalk)
        {
            WarningHelper.AddSystemWarning(UiString.TextToTalkWarning.GetDescription());
        }
    }

    static DateTime _closeWindowTime = DateTime.Now;
    private unsafe static void CloseWindow()
    {
        if (_closeWindowTime < DateTime.Now) return;

        var needGreedWindow = Svc.GameGui.GetAddonByName("NeedGreed", 1);
        if (needGreedWindow == IntPtr.Zero) return;

        var notification = (AtkUnitBase*)Svc.GameGui.GetAddonByName("_Notification", 1);
        if (notification == null) return;

        // Use stackalloc to avoid heap allocation
        var atkValues = stackalloc AtkValue[2];
        atkValues[0].Type = atkValues[1].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int;
        atkValues[0].Int = 0;
        atkValues[1].Int = 2;
        try
        {
            notification->FireCallback(2, atkValues);
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, "Failed to close the window!");
        }
    }

    static DateTime _nextOpenTime = DateTime.Now;
    static ulong _lastChest = 0;
    private unsafe static void OpenChest()
    {
        if (Player.Object == null) return;
        if (DataCenter.InCombat) return;
        var player = Player.Object;

        object? treasure = null;
        foreach (var o in Svc.Objects)
        {
            if (o == null) continue;
            var dis = Vector3.Distance(player.Position, o.Position) - player.HitboxRadius - o.HitboxRadius;
            if (dis > 0.5f) continue;

            var address = (GameObject*)(void*)o.Address;
            if (address->ObjectKind != ObjectKind.Treasure) continue;

            bool opened = false;
            foreach (var item in Loot.Instance()->Items)
            {
                if (item.ChestObjectId == o.GameObjectId)
                {
                    opened = true;
                    break;
                }
            }
            if (opened) continue;

            treasure = o;
            break;
        }

        if (treasure == null) return;
        if (DateTime.Now < _nextOpenTime) return;

        var treasureGameObject = treasure as IGameObject;
        if (treasureGameObject == null) return;

        if (treasureGameObject.GameObjectId == _lastChest && DateTime.Now - _nextOpenTime < TimeSpan.FromSeconds(10)) return;

        _nextOpenTime = DateTime.Now.AddSeconds(new Random().NextDouble() + 0.2);
        _lastChest = treasureGameObject.GameObjectId;

        try
        {
            Svc.Targets.Target = treasureGameObject;
            TargetSystem.Instance()->InteractWithObject((GameObject*)(void*)treasureGameObject.Address);
            Notify.Plain($"Try to open the chest {treasureGameObject.Name}");
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Failed to open the chest!");
        }

        if (!Service.Config.AutoCloseChestWindow) return;
        _closeWindowTime = DateTime.Now.AddSeconds(0.5);
    }

    public static void Dispose()
    {
        Svc.Framework.Update -= RSRUpdate;
        PreviewUpdater.Dispose();
        ActionSequencerUpdater.SaveFiles();
        ActionUpdater.ClearNextAction();
    }
}