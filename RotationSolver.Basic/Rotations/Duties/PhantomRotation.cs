namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// Represents a rotation for phantom duties in the game.
/// </summary>
//[DutyTerritory(1252)] // TODO: Verify IDs.
public abstract class PhantomRotation : DutyRotation
{
}

public partial class DutyRotation
{
    /// <summary>
    /// Modifies the settings for Offensive Aria PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOffensiveAriaPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PhantomBard];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Romeos Ballad PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRomeosBalladPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PhantomBard];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Mighty March PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyMightyMarchPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PhantomBard];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Heros Rime PvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyHerosRimePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PhantomBard];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
}