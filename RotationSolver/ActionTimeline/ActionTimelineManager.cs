using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel.Sheets;
using System.Collections.Concurrent;
using Action = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace RotationSolver.ActionTimeline;

/// <summary>
/// Manages action timeline data for timeline visualization
/// </summary>
public class ActionTimelineManager : IDisposable
{
    internal const byte GCDCooldownGroup = 58;

    private static ActionTimelineManager? _instance;
    public static ActionTimelineManager Instance => _instance ??= new ActionTimelineManager();

    private readonly Queue<TimelineItem> _items = new(2048);
    private TimelineItem? _lastItem;
    private DateTime _lastTime = DateTime.MinValue;
    
    private delegate void OnActorControlDelegate(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg4, uint arg5, ulong targetId, byte a10);
    
    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(OnActorControl))]
    private readonly Hook<OnActorControlDelegate>? _onActorControlHook = null;

    private delegate void OnCastDelegate(uint sourceId, IntPtr sourceCharacter);
    
    [Signature("40 56 41 56 48 81 EC ?? ?? ?? ?? 48 8B F2", DetourName = nameof(OnCast))]
    private readonly Hook<OnCastDelegate>? _onCastHook = null;

    public DateTime EndTime { get; private set; } = DateTime.Now;

    private ActionTimelineManager()
    {
        try
        {
            Svc.Hook.InitializeFromAttributes(this);
            _onActorControlHook?.Enable();
            _onCastHook?.Enable();
            ActionEffect.ActionEffectEvent += ActionFromSelf;
        }
        catch (Exception e)
        {
            Svc.Log.Error("Error initiating ActionTimeline hooks: " + e.Message);
        }
    }

    public void Dispose()
    {
        _items.Clear();
        ActionEffect.ActionEffectEvent -= ActionFromSelf;
        _onActorControlHook?.Disable();
        _onActorControlHook?.Dispose();
        _onCastHook?.Disable();
        _onCastHook?.Dispose();
    }

    public unsafe float GCD
    {
        get
        {
            var cdGrp = ActionManager.Instance()->GetRecastGroupDetail(GCDCooldownGroup - 1);
            return cdGrp->Total;
        }
    }

    private static TimelineItemType GetActionType(uint actionId, ActionType type)
    {
        switch (type)
        {
            case ActionType.Action:
                var action = Svc.Data.GetExcelSheet<Action>()?.GetRow(actionId);
                if (action == null) break;

                if (actionId == 3) return TimelineItemType.OGCD; // Sprint

                var isRealGcd = action.Value.CooldownGroup == GCDCooldownGroup || action.Value.AdditionalCooldownGroup == GCDCooldownGroup;
                return action.Value.ActionCategory.Value.RowId == 1 // AutoAttack
                    ? TimelineItemType.AutoAttack
                    : !isRealGcd && action.Value.ActionCategory.Value.RowId == 4 ? TimelineItemType.OGCD // Ability
                    : TimelineItemType.GCD;

            case ActionType.Item:
                var item = Svc.Data.GetExcelSheet<Item>()?.GetRow(actionId);
                return item?.CastTimeSeconds > 0 ? TimelineItemType.GCD : TimelineItemType.OGCD;
        }

        return TimelineItemType.GCD;
    }

    private void AddItem(TimelineItem item)
    {
        if (item == null) return;
        if (_items.Count >= 2048)
        {
            _items.Dequeue();
        }
        _items.Enqueue(item);
        if (item.Type != TimelineItemType.AutoAttack)
        {
            _lastItem = item;
            _lastTime = DateTime.Now;
            UpdateEndTime(item.EndTime);
        }
    }

    private void UpdateEndTime(DateTime endTime)
    {
        if (endTime > EndTime) EndTime = endTime;
    }

    public List<TimelineItem> GetItems(DateTime time, out DateTime lastEndTime)
    {
        var result = new List<TimelineItem>();
        lastEndTime = DateTime.Now;
        foreach (var item in _items)
        {
            if (item.EndTime > time)
            {
                result.Add(item);
            }
            else if (item.Type == TimelineItemType.GCD)
            {
                lastEndTime = item.EndTime;
            }
        }
        return result;
    }

    private void ActionFromSelf(ActionEffectSet set)
    {
        if (!Player.Available) return;
        if (set.Source?.GameObjectId != Player.Object?.GameObjectId) return;

        var now = DateTime.Now;
        var type = GetActionType(set.Header.ActionID, set.Header.ActionType);

        if (_lastItem != null && _lastItem.CastingTime > 0 && type == TimelineItemType.GCD
            && _lastItem.State == TimelineItemState.Casting)
        {
            _lastItem.AnimationLockTime = set.Header.AnimationLockTime;
            _lastItem.Name = set.Name;
            _lastItem.Icon = set.IconId;
            _lastItem.State = TimelineItemState.Finished;
        }
        else
        {
            AddItem(new TimelineItem()
            {
                StartTime = now,
                AnimationLockTime = type == TimelineItemType.AutoAttack ? 0 : set.Header.AnimationLockTime,
                GCDTime = type == TimelineItemType.GCD ? GCD : 0,
                Type = type,
                Name = set.Name,
                Icon = set.IconId,
                State = TimelineItemState.Finished,
            });
        }

        var effectItem = _lastItem;
        if (effectItem?.Type is TimelineItemType.AutoAttack) return;

        UpdateEndTime(effectItem?.EndTime ?? now);
    }

    private void CancelCasting()
    {
        if (_lastItem == null || _lastItem.CastingTime == 0) return;

        _lastItem.State = TimelineItemState.Canceled;
        var maxTime = (float)(DateTime.Now - _lastItem.StartTime).TotalSeconds;
        _lastItem.GCDTime = 0;
        _lastItem.CastingTime = MathF.Min(maxTime, _lastItem.CastingTime);
    }

    private void OnActorControl(uint entityId, uint type, uint buffID, uint direct, uint actionId, uint sourceId, uint arg4, uint arg5, ulong targetId, byte a10)
    {
        _onActorControlHook?.Original(entityId, type, buffID, direct, actionId, sourceId, arg4, arg5, targetId, a10);

        try
        {
            if (Player.Object == null || entityId != Player.Object.GameObjectId) return;

            // CancelAbility ActorControlCategory value
            if (type == 15)
            {
                CancelCasting();
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in OnActorControl: {ex.Message}");
        }
    }

    private void OnCast(uint sourceId, IntPtr sourceCharacter)
    {
        _onCastHook?.Original(sourceId, sourceCharacter);
        // Additional cast handling could go here
    }
}

/// <summary>
/// Represents an item in the action timeline
/// </summary>
public class TimelineItem
{
    public DateTime StartTime { get; set; }
    public string Name { get; set; } = "";
    public uint Icon { get; set; }
    public float CastingTime { get; set; }
    public float AnimationLockTime { get; set; }
    public float GCDTime { get; set; }
    public TimelineItemType Type { get; set; }
    public TimelineItemState State { get; set; }

    public DateTime EndTime => StartTime.AddSeconds(Math.Max(CastingTime + AnimationLockTime, GCDTime));
}

/// <summary>
/// Type of timeline item
/// </summary>
public enum TimelineItemType
{
    GCD,
    OGCD,
    AutoAttack
}

/// <summary>
/// State of timeline item
/// </summary>
public enum TimelineItemState
{
    Casting,
    Finished,
    Canceled
}
