namespace RotationSolver.Basic.Data;

/// <summary>
/// Specifies who to raise.
/// </summary>
public enum RaiseType : byte
{
    /// <summary>
    /// Raise only party members.
    /// </summary>
    [Description("Raise only party members.")]
    PartyOnly,

    /// <summary>
    /// Raise party members and alliance supports.
    /// </summary>
    [Description("Raise party members and alliance supports.")]
    PartyAndAllianceSupports,

    /// <summary>
    /// Raise party members and alliance healers.
    /// </summary>
    [Description("Raise party members and alliance healers.")]
    PartyAndAllianceHealers,

    /// <summary>
    /// Raise all.
    /// </summary>
    [Description("Raise All.")]
    All,
}