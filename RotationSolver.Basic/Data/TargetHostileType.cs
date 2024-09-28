namespace RotationSolver.Basic.Data;

/// <summary>
/// Hostile target.
/// </summary>
public enum TargetHostileType : byte
{
    /// <summary>
    /// All targets that are in range for any abilities (Tanks/Autoduty).
    /// </summary>
    [Description("All targets that are in range for any abilities (Tanks/Autoduty)")]
    AllTargetsCanAttack,

    /// <summary>
    /// Previously engaged targets (Non-Tanks).
    /// </summary>
    [Description("Previously engaged targets (Non-Tanks)")]
    TargetsHaveTarget,

    /// <summary>
    /// All targets when solo in duty, or previously engaged.
    /// </summary>
    [Description("All targets when solo in duty, or previously engaged.")]
    AllTargetsWhenSoloInDuty,

    /// <summary>
    /// All targets when solo, or previously engaged.
    /// </summary>
    [Description("All targets when solo, or previously engaged.")]
    AllTargetsWhenSolo,

    /// <summary>
    /// Targets in your enemy list.
    /// </summary>
    [Description("Only attack targets in your parties enemy list")]
    TargetIsInEnemiesList,

    /// <summary>
    /// All targets when solo, or only attack targets in your parties enemy list.
    /// </summary>
    [Description("All targets when solo, or only attack targets in your parties enemy list")]
    AllTargetsWhenSoloTargetIsInEnemiesList,
}