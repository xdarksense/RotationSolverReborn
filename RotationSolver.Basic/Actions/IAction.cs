namespace RotationSolver.Basic.Actions;

/// <summary>
/// Represents an action that can be performed.
/// </summary>
public interface IAction : ITexture, IEnoughLevel
{
    /// <summary>
    /// Gets the ID of this action.
    /// </summary>
    uint ID { get; }

    /// <summary>
    /// Gets the adjusted ID of this action.
    /// </summary>
    uint AdjustedID { get; }

    /// <summary>
    /// Gets the animation lock time of this action.
    /// </summary>
    float AnimationLockTime { get; }

    /// <summary>
    /// Gets the key used for sorting this action.
    /// </summary>
    uint SortKey { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this action is in the cooldown window.
    /// </summary>
    /// <markdown file="Actions" name="Show on CD window">
    /// Toggles whether this action is in the <see cref="RotationSolver.UI.CooldownWindow">cooldown</see> window.
    /// </markdown>
    bool IsInCooldown { get; set; }

    /// <summary>
    /// Gets the cooldown information for this action.
    /// </summary>
    ICooldown Cooldown { get; }

    /// <summary>
    /// Uses the action.
    /// </summary>
    /// <returns>True if the action was successfully used; otherwise, false.</returns>
    bool Use();
}