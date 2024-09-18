namespace RotationSolver.Basic.Data;

/// <summary>
/// The way to place the beneficial area action.
/// </summary>
public enum BeneficialAreaStrategy : byte
{
    /// <summary>
    /// Should use predefined location.
    /// </summary>
    [Description("On predefined location")]
    OnLocations,

    /// <summary>
    /// Should use only predefined location.
    /// </summary>
    [Description("Only on predefined location")]
    OnlyOnLocations,

    /// <summary>
    /// Should use target.
    /// </summary>
    [Description("On target")]
    OnTarget,

    /// <summary>
    /// Should use the calculated location.
    /// </summary>
    [Description("On the calculated location")]
    OnCalculated,
}