namespace RotationSolver.Basic.Data;

/// <summary>
/// The Type of description.
/// </summary>
public enum DescType : byte
{
    /// <summary>
    /// 
    /// </summary>
    None,

    /// <summary>
    /// 
    /// </summary>
    [Description("Burst Actions")]
    BurstActions,

    /// <summary>
    /// Area Heal GCDs
    /// </summary>
    [Description("Heal Area GCD")]
    HealAreaGCD,

    /// <summary>
    /// Area Heal oGCDs
    /// </summary>
    [Description("Heal Area Ability")]
    HealAreaAbility,

    /// <summary>
    /// Single Target Heal GCDs
    /// </summary>
    [Description("Heal Single GCD")]
    HealSingleGCD,

    /// <summary>
    /// Single Target Heal oGCDs
    /// </summary>
    [Description("Heal Single Ability")]
    HealSingleAbility,

    /// <summary>
    /// Area Defensive GCDs (Sheilds, mitigation, etc)
    /// </summary>
    [Description("Defense Area GCD")]
    DefenseAreaGCD,

    /// <summary>
    /// Area Defensive oGCDs (Sheilds, mitigation, etc)
    /// </summary>
    [Description("Defense Area Ability")]
    DefenseAreaAbility,

    /// <summary>
    /// Single Target Defensive GCDs (Sheilds, mitigation, etc)
    /// </summary>
    [Description("Defense Single GCD")]
    DefenseSingleGCD,

    /// <summary>
    /// Single Target Defensive oGCDs (Sheilds, mitigation, etc)
    /// </summary>
    [Description("Defense Single Ability")]
    DefenseSingleAbility,

    /// <summary>
    /// 
    /// </summary>
    [Description("Move Forward GCD")]
    MoveForwardGCD,

    /// <summary>
    /// 
    /// </summary>
    [Description("Move Forward Ability")]
    MoveForwardAbility,

    /// <summary>
    /// 
    /// </summary>
    [Description("Move Back Ability")]
    MoveBackAbility,

    /// <summary>
    /// 
    /// </summary>
    [Description("Speed Ability")]
    SpeedAbility,
}
