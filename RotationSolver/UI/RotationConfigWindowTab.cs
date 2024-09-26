using ECommons.DalamudServices;
using System.ComponentModel;

namespace RotationSolver.UI;

/// <summary>
/// Attribute to mark tabs that should be skipped.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class TabSkipAttribute : Attribute
{
}

/// <summary>
/// Attribute to specify an icon for a tab.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class TabIconAttribute : Attribute
{
    public uint Icon { get; init; }
}

/// <summary>
/// Enum representing different tabs in the rotation config window.
/// </summary>
internal enum RotationConfigWindowTab : byte
{
    [TabSkip] About,
    [TabSkip] Rotation,

    [Description("The abilities and custom conditions for your current Job.")]
    [TabIcon(Icon = 4)] Actions,

    [Description("All rotations that RSR has loaded")]
    [TabIcon(Icon = 47)] Rotations,

    [Description("Reactive action and statuses lists")]
    [TabIcon(Icon = 21)] List,

    [Description("Basic settings")]
    [TabIcon(Icon = 14)] Basic,

    [Description("User interface settings")]
    [TabIcon(Icon = 42)] UI,

    [Description("General Action usage and control")]
    [TabIcon(Icon = 29)] Auto,

    [Description("Targeting settings")]
    [TabIcon(Icon = 16)] Target,

    [Description("Features that are not core to RSR but are helpful")]
    [TabIcon(Icon = 51)] Extra,

    [Description("Debug stuff for developers and rotation writers. Please do not leave this enabled.")]
    [TabIcon(Icon = 5)] Debug,

    [Description("Specific Autoduty related settings and information")]
    [TabIcon(Icon = 4)] AutoDuty,
}

/// <summary>
/// Struct representing an incompatible plugin.
/// </summary>
public struct IncompatiblePlugin
{
    public string Name { get; init; }
    public string Icon { get; init; }
    public string Url { get; init; }
    public string Features { get; init; }

    /// <summary>
    /// Checks if the plugin is installed.
    /// </summary>
    [JsonIgnore]
    public readonly bool IsInstalled
    {
        get
        {
            var name = this.Name;
            var installedPlugins = Svc.PluginInterface.InstalledPlugins;
            return installedPlugins.Any(x =>
                (x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.InternalName.Equals(name, StringComparison.OrdinalIgnoreCase)) && x.IsLoaded);
        }
    }

    public CompatibleType Type { get; init; }
}

/// <summary>
/// Enum representing different types of compatibility issues.
/// </summary>
[Flags]
public enum CompatibleType : byte
{
    Skill_Usage = 1 << 0,
    Skill_Selection = 1 << 1,
    Crash = 1 << 2,
    Broken = 1 << 3,
}