namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// Represents a rotation for variant duties in the game.
/// </summary>
[DutyTerritory(1069, 1075, 1076, 1137, 1176)] // TODO: Verify the variant territory IDs.
public abstract class VariantRotation : DutyRotation
{
}

public partial class DutyRotation
{
    /// <summary>
    /// Modifies the settings for Variant Cure PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyVariantCurePvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = new[] { StatusID.VariantCureSet };
    }

    /// <summary>
    /// Modifies the settings for Variant Cure PvE with ID 33862.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyVariantCurePvE_33862(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = new[] { StatusID.VariantCureSet };
    }
}