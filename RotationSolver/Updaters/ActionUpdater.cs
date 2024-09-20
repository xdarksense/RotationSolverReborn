using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using RotationSolver.Commands;

namespace RotationSolver.Updaters;

internal static class ActionUpdater
{
    internal static DateTime AutoCancelTime { get; set; } = DateTime.MinValue;

    static readonly RandomDelay _GCDDelay = new(() => Service.Config.WeaponDelay);

    internal static IAction? NextAction { get; set; }

    private static IBaseAction? _nextGCDAction;
    const float gcdHeight = 5;
    internal static IBaseAction? NextGCDAction
    {
        get => _nextGCDAction;
        set
        {
            if (_nextGCDAction != value)
            {
                _nextGCDAction = value;
            }
        }
    }

    internal static void ClearNextAction()
    {
        SetAction(0);
        NextAction = NextGCDAction = null;
    }

    internal static void UpdateNextAction()
    {
        IPlayerCharacter localPlayer = Player.Object;
        var customRotation = DataCenter.RightNowRotation;

        try
        {
            if (localPlayer != null && customRotation != null
                && customRotation.TryInvoke(out var newAction, out var gcdAction))
            {
                NextAction = newAction;
                NextGCDAction = gcdAction as IBaseAction;
                return;
            }
        }
        catch (Exception ex)
        {
#pragma warning disable 0436
            WarningHelper.AddSystemWarning($"Failed to update the next action in the rotation because: {ex.Message}");
            Svc.Log.Error(ex, "Failed to update next action.");
        }

        NextAction = NextGCDAction = null;
    }

    private static List<uint> actionOverrideList = new();

    private static void SetAction(uint id)
    {
        if (actionOverrideList.Count == 0)
        {
            actionOverrideList.Add(id);
        }
        else
        {
            actionOverrideList[0] = id;
        }
    }

    internal unsafe static void UpdateActionInfo()
    {
        SetAction(NextGCDAction?.AdjustedID ?? 0);
        //UpdateWeaponTime();
        UpdateCombatTime();
        UpdateSlots();
        UpdateMoving();
        UpdateMPTimer();
    }

    private unsafe static void UpdateSlots()
    {
        var actionManager = ActionManager.Instance();
        for (int i = 0; i < DataCenter.BluSlots.Length; i++)
        {
            DataCenter.BluSlots[i] = actionManager->GetActiveBlueMageActionInSlot(i);
        }
        for (ushort i = 0; i < DataCenter.DutyActions.Length; i++)
        {
            DataCenter.DutyActions[i] = ActionManager.GetDutyActionId(i);
        }
    }

    static DateTime _stopMovingTime = DateTime.MinValue;
    private unsafe static void UpdateMoving()
    {
        var last = DataCenter.IsMoving;
        DataCenter.IsMoving = AgentMap.Instance()->IsPlayerMoving > 0;
        if (last && !DataCenter.IsMoving)
        {
            _stopMovingTime = DateTime.Now;
        }
        else if (DataCenter.IsMoving)
        {
            _stopMovingTime = DateTime.MinValue;
        }

        DataCenter.StopMovingRaw = _stopMovingTime == DateTime.MinValue
            ? 0
            : (float)(DateTime.Now - _stopMovingTime).TotalSeconds;
    }

    static DateTime _startCombatTime = DateTime.MinValue;
    private static void UpdateCombatTime()
    {
        var lastInCombat = DataCenter.InCombat;
        DataCenter.InCombat = Svc.Condition[ConditionFlag.InCombat];

        if (!lastInCombat && DataCenter.InCombat)
        {
            _startCombatTime = DateTime.Now;
        }
        else if (lastInCombat && !DataCenter.InCombat)
        {
            _startCombatTime = DateTime.MinValue;

            if (Service.Config.AutoOffAfterCombat)
            {
                AutoCancelTime = DateTime.Now.AddSeconds(Service.Config.AutoOffAfterCombatTime);
            }
        }

        DataCenter.CombatTimeRaw = _startCombatTime == DateTime.MinValue
            ? 0
            : (float)(DateTime.Now - _startCombatTime).TotalSeconds;
    }

    static uint _lastMP = 0;
    static DateTime _lastMPUpdate = DateTime.Now;

    internal static float MPUpdateElapsed => (float)(DateTime.Now - _lastMPUpdate).TotalSeconds % 3;

    private static void UpdateMPTimer()
    {
        var player = Player.Object;
        if (player == null) return;

        // Ignore if player is Black Mage
        if (player.ClassJob.Id != (uint)ECommons.ExcelServices.Job.BLM) return;

        // Ignore if player is Lucid Dreaming
        if (player.HasStatus(true, StatusID.LucidDreaming)) return;

        if (_lastMP < player.CurrentMp)
        {
            _lastMPUpdate = DateTime.Now;
        }
        _lastMP = player.CurrentMp;
    }

    internal unsafe static bool CanDoAction()
    {
        if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
            || Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || Svc.Condition[ConditionFlag.Occupied33]
            || Svc.Condition[ConditionFlag.Occupied38]
            || Svc.Condition[ConditionFlag.Jumping61]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Mounted]
            //|| Svc.Condition[ConditionFlag.SufferingStatusAffliction] //Because of BLU30!
            || Svc.Condition[ConditionFlag.SufferingStatusAffliction2]
            || Svc.Condition[ConditionFlag.RolePlaying]
            || Svc.Condition[ConditionFlag.InFlight]
            || Svc.Condition[ConditionFlag.Diving]
            || Svc.Condition[ConditionFlag.Swimming]
            || Svc.Condition[ConditionFlag.Unconscious]
            || Svc.Condition[ConditionFlag.MeldingMateria]
            || (ActionManager.Instance()->ActionQueued && NextAction != null
                && ActionManager.Instance()->QueuedActionId != NextAction.AdjustedID)
            || Player.Object.CurrentHp == 0) return false;

        var nextAction = NextAction;
        if (nextAction == null) return false;

        //Skip when casting
        if (Player.Object.TotalCastTime - DataCenter.ActionAhead > 0) return false;

        //GCD
        return RSCommands.CanDoAnAction(ActionHelper.CanUseGCD);
    }
}