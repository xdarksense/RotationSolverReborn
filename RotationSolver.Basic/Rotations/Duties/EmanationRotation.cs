namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// The variant action.
/// </summary>
[DutyTerritory(263, 264)] // TODO: Verify the variant territory IDs.
public abstract class EmanationRotation : DutyRotation
{
}

partial class DutyRotation
{
    /// <summary>
    /// Modifies the settings for Vril PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyVrilPvE(ref ActionSetting setting)
    {
        setting.StatusFromSelf = true;
        setting.TargetStatusProvide = new[] { StatusID.Vril };
    }

    /// <summary>
    /// Modifies the settings for Vril PvE with ID 9345.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyVrilPvE_9345(ref ActionSetting setting)
    {
        setting.StatusFromSelf = true;
        setting.TargetStatusProvide = new[] { StatusID.Vril };
    }
}