namespace RotationSolver.Basic.Configuration;

/// <summary>
/// Records usage statistics for the Rotation Solver.
/// </summary>
public class RotationSolverRecord
{
    /// <summary>
    /// Gets or sets the number of times the Rotation Solver has clicked for you.
    /// </summary>
    public uint ClickingCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the users that have already been greeted.
    /// </summary>
    public HashSet<string> SaidUsers { get; set; } = new HashSet<string>();
}