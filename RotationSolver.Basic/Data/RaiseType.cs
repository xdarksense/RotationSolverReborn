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
    /// Raise party and alliance members.
    /// </summary>
    [Description("Raise all.")]
    PartyAndAlliance,

    /// <summary>
    /// Raise party members and alliance healers.
    /// </summary>
    [Description("Raise party members and non-party healers.")]
    PartyAndAllianceHealers,
}