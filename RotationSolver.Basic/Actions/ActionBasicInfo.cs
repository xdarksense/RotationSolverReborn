using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace RotationSolver.Basic.Actions;

/// <summary>
/// The action info for the <see cref="Lumina.Excel.Sheets.Action"/>.
/// </summary>
public readonly struct ActionBasicInfo
{
    /// <summary>
    /// Actions that do not require casting.
    /// </summary>
    internal static readonly uint[] ActionsNoNeedCasting =
    [
        5,
        (uint)ActionID.PowerfulShotPvP,
        (uint)ActionID.BlastChargePvP,
    ];

    private readonly IBaseAction _action;

    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    public readonly string Name => _action.Action.Name.ExtractText();

    /// <summary>
    /// Gets the unique identifier of the action.
    /// </summary>
    public readonly uint ID => _action.Action.RowId;

    /// <summary>
    /// Gets the range of the action.
    /// </summary>
    public readonly sbyte Range => _action.Action.Range;

    /// <summary>
    /// Gets the effect range of the action.
    /// </summary>
    public readonly byte EffectRange => _action.Action.EffectRange;

    /// <summary>
    /// Gets the icon ID of the action.
    /// </summary>
    public readonly uint IconID => ID == (uint)ActionID.SprintPvE ? 104u : _action.Action.Icon;

    /// <summary>
    /// Gets the adjusted ID of the action, considering any modifications.
    /// </summary>
    public readonly uint AdjustedID => (uint)Service.GetAdjustedActionId((ActionID)ID);

    /// <summary>
    /// Gets the attack type of the action.
    /// </summary>
    public readonly AttackType AttackType => (AttackType)(_action.Action.AttackType.RowId != 0 ? _action.Action.AttackType.RowId : byte.MaxValue);

    /// <summary>
    /// Gets the aspect of the action.
    /// </summary>
    public Aspect Aspect { get; }

    /// <summary>
    /// Gets the level required to use the action.
    /// </summary>
    public readonly byte Level => _action.Action.ClassJobLevel;

    /// <summary>
    /// Determines whether the player's level is sufficient to use the action.
    /// </summary>
    public readonly bool EnoughLevel => Player.Level >= Level;

    /// <summary>
    /// Determines whether the action is a PvP action.
    /// </summary>
    public readonly bool IsPvP => _action.Action.IsPvP;

    /// <summary>
    /// Gets the cast type of the action.
    /// </summary>
    public readonly byte CastType => _action.Action.CastType;

    /// <summary>
    /// Gets the casting time of the action.
    /// </summary>
    public readonly unsafe float CastTime => ((ActionID)AdjustedID).GetCastTime();

    /// <summary>
    /// Gets the MP required to use the action.
    /// </summary>
    public readonly unsafe uint MPNeed
    {
        get
        {
            if (IsPvP && ID != 29711)
            {
                return 0;
            }

            uint? mpOver = _action.Setting.MPOverride?.Invoke();
            if (mpOver.HasValue)
            {
                return mpOver.Value;
            }

            uint mp = (uint)ActionManager.GetActionCost(ActionType.Action, AdjustedID, 0, 0, 0, 0);
            return mp < 100 ? 0 : mp;
        }
    }

    /// <summary>
    /// Determines whether the action is on the player's hotbar or slot.
    /// </summary>
    public readonly bool IsOnSlot => _action.Action.ClassJob.RowId == (uint)Job.BLU
                ? DataCenter.BluSlots.Contains(ID)
                : IsDutyAction ? DataCenter.DutyActions.Contains(ID) : IsPvP == DataCenter.IsPvP;

    /// <summary>
    /// Determines whether the action is a limit break action.
    /// </summary>
    public bool IsLimitBreak { get; }

    /// <summary>
    /// Determines whether the action is a general global cooldown (GCD) action.
    /// </summary>
    public bool IsGeneralGCD { get; }

    /// <summary>
    /// Determines whether the action is a real global cooldown (GCD) action.
    /// </summary>
    public bool IsRealGCD { get; }

    /// <summary>
    /// Determines whether the action is a duty action.
    /// </summary>
    public bool IsDutyAction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionBasicInfo"/> struct.
    /// </summary>
    /// <param name="action">The base action.</param>
    /// <param name="isDutyAction">Indicates whether the action is a duty action.</param>
    public ActionBasicInfo(IBaseAction action, bool isDutyAction)
    {
        _action = action;
        IsGeneralGCD = _action.Action.IsGeneralGCD();
        IsRealGCD = _action.Action.IsRealGCD();
        IsLimitBreak = (ActionCate?)_action.Action.ActionCategory.Value.RowId
            is ActionCate.LimitBreak or ActionCate.LimitBreak_15;
        IsDutyAction = isDutyAction;
        Aspect = (Aspect)_action.Action.Aspect;
    }

    /// <summary>
    /// Performs a basic check to determine whether the action can be used.
    /// </summary>
    /// <param name="skipStatusProvideCheck">Whether to skip the status provide check.</param>
    /// <param name="skipComboCheck">Whether to skip the combo check.</param>
    /// <param name="skipCastingCheck">Whether to skip the casting check.</param>
    /// <returns>True if the action passes the basic check; otherwise, false.</returns>
    internal readonly bool BasicCheck(bool skipStatusProvideCheck, bool skipComboCheck, bool skipCastingCheck)
    {
        if (Player.Object.StatusList == null)
        {
            return false;
        }

        if (!IsActionEnabled() || !IsOnSlot)
        {
            return false;
        }

        if (IsLimitBreak)
        {
            return true;
        }

        if (IsActionDisabled() || !EnoughLevel || !HasEnoughMP() || !SpellUnlocked)
        {
            return false;
        }

        if (IsStatusNeeded() || IsStatusProvided(skipStatusProvideCheck))
        {
            return false;
        }

        if (IsLimitBreakLevelLow() || !IsComboValid(skipComboCheck) || !IsRoleActionValid())
        {
            return false;
        }

        return !NeedsCasting(skipCastingCheck) && (!IsGeneralGCD || !IsStatusProvidedDuringGCD()) && IsActionCheckValid() && IsRotationCheckValid();
    }

    /// <summary>
    /// Determines whether the spell is unlocked for the player.
    /// </summary>
    public unsafe bool SpellUnlocked => _action.Action.UnlockLink.RowId <= 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(_action.Action.UnlockLink.RowId);

    private bool IsActionEnabled()
    {
        return _action.Config?.IsEnabled ?? false;
    }

    private bool IsActionDisabled()
    {
        return !IBaseAction.ForceEnable && (DataCenter.DisabledActionSequencer?.Contains(ID) ?? false);
    }

    private bool HasEnoughMP()
    {
        return DataCenter.CurrentMp >= MPNeed;
    }

    private bool IsStatusNeeded()
    {
        return Player.Object.StatusList != null && _action.Setting.StatusNeed != null && Player.Object.WillStatusEndGCD(_action.Config.StatusGcdCount, 0, _action.Setting.StatusFromSelf, _action.Setting.StatusNeed);
    }

    private bool IsStatusProvided(bool skipStatusProvideCheck)
    {
        return Player.Object.StatusList != null && !skipStatusProvideCheck && _action.Setting.StatusProvide != null && !Player.Object.WillStatusEndGCD(_action.Config.StatusGcdCount, 0, _action.Setting.StatusFromSelf, _action.Setting.StatusProvide);
    }

    private bool IsLimitBreakLevelLow()
    {
        return _action.Action.ActionCategory.RowId == 15 && CustomRotation.LimitBreakLevel <= 1;
    }

    private bool IsComboValid(bool skipComboCheck)
    {
        return skipComboCheck || !IsGeneralGCD || CheckForCombo();
    }

    private bool IsRoleActionValid()
    {
        return !_action.Action.IsRoleAction || (_action.Action.ClassJobCategory.Value.DoesJobMatchCategory(DataCenter.Job) == true);
    }

    private bool IsRotationCheckValid()
    {
        return IBaseAction.ForceEnable || (_action.Setting.RotationCheck?.Invoke() ?? true);
    }

    private bool NeedsCasting(bool skipCastingCheck)
    {
        return CastTime > 0 && !Player.Object.HasStatus(true, new[] { StatusID.Swiftcast, StatusID.Triplecast, StatusID.Dualcast }) && !ActionsNoNeedCasting.Contains(ID) &&
               (DataCenter.SpecialType == SpecialCommandType.NoCasting || (DateTime.Now > DataCenter.KnockbackStart && DateTime.Now < DataCenter.KnockbackFinished) ||
                (DataCenter.NoPoslock && DataCenter.IsMoving && !skipCastingCheck));
    }

    private bool IsStatusProvidedDuringGCD()
    {
        return _action.Setting.StatusProvide?.Length > 0 && _action.Setting.IsFriendly && IActionHelper.IsLastGCD(true, _action) && DataCenter.TimeSinceLastAction.TotalSeconds < 3;
    }

    private bool IsActionCheckValid()
    {
        return _action.Setting.ActionCheck?.Invoke() ?? true;
    }

    private readonly bool CheckForCombo()
    {
        if (!_action.Config.ShouldCheckCombo)
        {
            return true;
        }

        if (_action.Setting.ComboIdsNot != null)
        {
            if (_action.Setting.ComboIdsNot.Contains(DataCenter.LastComboAction))
            {
                return false;
            }
        }

        ActionID[] comboActions = _action.Action.ActionCombo.RowId != 0
                                ? [(ActionID)_action.Action.ActionCombo.RowId]
                                : [];

        if (_action.Setting.ComboIds != null)
        {
            comboActions = [.. comboActions, .. _action.Setting.ComboIds];
        }

        if (comboActions.Length > 0)
        {
            if (comboActions.Contains(DataCenter.LastComboAction))
            {
                if (DataCenter.ComboTime < DataCenter.DefaultGCDRemain)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
}


