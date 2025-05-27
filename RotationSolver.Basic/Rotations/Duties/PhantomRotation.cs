namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// Represents a rotation for phantom duties in the game.
/// </summary>
[DutyTerritory(1252)] // TODO: Verify IDs.
public partial class PhantomRotation : DutyRotation
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
        setting.ActionCheck = () => BardLevel >= 1;
        setting.StatusProvide = [StatusID.OffensiveAria];
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
        setting.ActionCheck = () => BardLevel >= 2;
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
        setting.ActionCheck = () => BardLevel >= 3;
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
        setting.ActionCheck = () => BardLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomAimPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomAimPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RangerLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for OccultFeatherfootPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultFeatherfootPvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Move;
        setting.ActionCheck = () => RangerLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for OccultFalconPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultFalconPvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Interrupt;
        setting.ActionCheck = () => RangerLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for OccultUnicornPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultUnicornPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RangerLevel >= 6;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomGuardPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomGuardPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 1;
    }

    /// <summary>
    /// Modifies the settings for PrayPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPrayPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 2;
        setting.StatusProvide = [StatusID.EnduringFortitude];
    }

    /// <summary>
    /// Modifies the settings for OccultHealPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultHealPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 3;
    }

    /// <summary>
    /// Modifies the settings forPledgePvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPledgePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 6;
    }
}