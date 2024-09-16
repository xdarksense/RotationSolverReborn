namespace RotationSolver.Basic.Data;

/// <summary>
/// The type of targeting.
/// </summary>
public enum TargetingType
{
    /// <summary>
    /// Find the target whose hit box is biggest.
    /// </summary>
    [Description("Big")]
    Big,

    /// <summary>
    /// Find the target whose hit box is smallest.
    /// </summary>
    [Description("Small")]
    Small,

    /// <summary>
    /// Find the target whose hp is highest.
    /// </summary>
    [Description("High HP")]
    HighHP,

    /// <summary>
    /// Find the target whose hp is lowest.
    /// </summary>
    [Description("Low HP")]
    LowHP,

    /// <summary>
    /// Find the target whose max hp is highest.
    /// </summary>
    [Description("High Max HP")]
    HighMaxHP,

    /// <summary>
    /// Find the target whose max hp is lowest.
    /// </summary>
    [Description("Low Max HP")]
    LowMaxHP,

    /// <summary>
    /// Find the target that is nearest.
    /// </summary>
    [Description("Nearest")]
    Nearest,

    /// <summary>
    /// Find the target that is farthest.
    /// </summary>
    [Description("Farthest")]
    Farthest,
}
