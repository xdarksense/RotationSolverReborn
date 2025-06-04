using static RotationSolver.Basic.Rotations.Duties.DutyRotation;

namespace RotationSolver.Basic.Attributes;

/// <summary>
/// 
/// </summary>
/// <param name="type"></param>
[AttributeUsage(AttributeTargets.Property)]
public class RotationConfigAttribute(CombatType type) : Attribute
{
    /// <summary>
    /// The type of this config.
    /// </summary>
    public CombatType Type => type;

    /// <summary>
    /// The display name for this config.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Phantom Job for this config.
    /// </summary>
    public PhantomJob PhantomJob { get; set; } = PhantomJob.None;
}
