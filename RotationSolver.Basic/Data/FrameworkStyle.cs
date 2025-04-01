namespace RotationSolver.Basic.Data;

/// <summary>
/// The way the framework updates.
/// </summary>
public enum FrameworkStyle : byte
{
    /// <summary>
    /// On the game thread.
    /// </summary>
    [Description("On the game thread")]
    MainThread,

    //[Description("Running outside of game thread")]
    //WorkTask,

    /// <summary>
    /// Running on Game Tick.
    /// </summary>
    [Description("Running on Game Tick")]
    RunOnTick,
}