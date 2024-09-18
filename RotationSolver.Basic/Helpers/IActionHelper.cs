namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Helper about actions.
/// </summary>
public static class IActionHelper
{
    internal static ActionID[] MovingActions { get; } =
    {
        ActionID.EnAvantPvE,
        //ActionID.PlungePvE,
        ActionID.RoughDividePvE,
        ActionID.ThunderclapPvE,
        ActionID.ShukuchiPvE,
        ActionID.IntervenePvE,
        ActionID.CorpsacorpsPvE,
        ActionID.HellsIngressPvE,
        ActionID.HissatsuGyotenPvE,
        ActionID.IcarusPvE,
        ActionID.OnslaughtPvE,
        //ActionID.SpineshatterDivePvE,
        ActionID.DragonfireDivePvE,
    };

    /// <summary>
    /// Determines if the last GCD action matches any of the provided actions.
    /// </summary>
    /// <param name="isAdjust">Whether to use the adjusted ID.</param>
    /// <param name="actions">The actions to check against.</param>
    /// <returns>True if the last GCD action matches any of the provided actions, otherwise false.</returns>
    internal static bool IsLastGCD(bool isAdjust, params IAction[] actions)
    {
        if (actions == null) return false;
        return IsLastGCD(GetIDFromActions(isAdjust, actions));
    }

    /// <summary>
    /// Determines if the last GCD action matches any of the provided action IDs.
    /// </summary>
    /// <param name="ids">The action IDs to check against.</param>
    /// <returns>True if the last GCD action matches any of the provided action IDs, otherwise false.</returns>
    internal static bool IsLastGCD(params ActionID[] ids)
    {
        return IsActionID(DataCenter.LastGCD, ids);
    }

    /// <summary>
    /// Determines if the last ability matches any of the provided actions.
    /// </summary>
    /// <param name="isAdjust">Whether to use the adjusted ID.</param>
    /// <param name="actions">The actions to check against.</param>
    /// <returns>True if the last ability matches any of the provided actions, otherwise false.</returns>
    internal static bool IsLastAbility(bool isAdjust, params IAction[] actions)
    {
        if (actions == null) return false;
        return IsLastAbility(GetIDFromActions(isAdjust, actions));
    }

    /// <summary>
    /// Determines if the last ability matches any of the provided action IDs.
    /// </summary>
    /// <param name="ids">The action IDs to check against.</param>
    /// <returns>True if the last ability matches any of the provided action IDs, otherwise false.</returns>
    internal static bool IsLastAbility(params ActionID[] ids)
    {
        return IsActionID(DataCenter.LastAbility, ids);
    }

    /// <summary>
    /// Determines if the last action matches any of the provided actions.
    /// </summary>
    /// <param name="isAdjust">Whether to use the adjusted ID.</param>
    /// <param name="actions">The actions to check against.</param>
    /// <returns>True if the last action matches any of the provided actions, otherwise false.</returns>
    internal static bool IsLastAction(bool isAdjust, params IAction[] actions)
    {
        if (actions == null) return false;
        return IsLastAction(GetIDFromActions(isAdjust, actions));
    }

    /// <summary>
    /// Determines if the last action matches any of the provided action IDs.
    /// </summary>
    /// <param name="ids">The action IDs to check against.</param>
    /// <returns>True if the last action matches any of the provided action IDs, otherwise false.</returns>
    internal static bool IsLastAction(params ActionID[] ids)
    {
        return IsActionID(DataCenter.LastAction, ids);
    }

    /// <summary>
    /// Determines if the action is the same as any of the provided actions.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <param name="isAdjust">Whether to use the adjusted ID.</param>
    /// <param name="actions">The actions to check against.</param>
    /// <returns>True if the action is the same as any of the provided actions, otherwise false.</returns>
    public static bool IsTheSameTo(this IAction action, bool isAdjust, params IAction[] actions)
    {
        if (actions == null) return false;
        return action.IsTheSameTo(GetIDFromActions(isAdjust, actions));
    }

    /// <summary>
    /// Determines if the action is the same as any of the provided action IDs.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <param name="actions">The action IDs to check against.</param>
    /// <returns>True if the action is the same as any of the provided action IDs, otherwise false.</returns>
    public static bool IsTheSameTo(this IAction action, params ActionID[] actions)
    {
        if (action == null || actions == null) return false;
        return IsActionID((ActionID)action.AdjustedID, actions);
    }

    /// <summary>
    /// Determines if the action ID matches any of the provided action IDs.
    /// </summary>
    /// <param name="id">The action ID to check.</param>
    /// <param name="ids">The action IDs to check against.</param>
    /// <returns>True if the action ID matches any of the provided action IDs, otherwise false.</returns>
    private static bool IsActionID(ActionID id, params ActionID[] ids)
    {
        if (ids == null) return false;
        return ids.Contains(id);
    }

    /// <summary>
    /// Gets the action IDs from the provided actions.
    /// </summary>
    /// <param name="isAdjust">Whether to use the adjusted ID.</param>
    /// <param name="actions">The actions to get the IDs from.</param>
    /// <returns>An array of action IDs.</returns>
    private static ActionID[] GetIDFromActions(bool isAdjust, params IAction[] actions)
    {
        if (actions == null) return Array.Empty<ActionID>();
        return actions.Select(a => isAdjust ? (ActionID)a.AdjustedID : (ActionID)a.ID).ToArray();
    }
}