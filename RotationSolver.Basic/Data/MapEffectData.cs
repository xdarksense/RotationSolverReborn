namespace RotationSolver.Basic.Data;

/// <summary>
/// Represents the data for a map effect.
/// </summary>
internal readonly struct MapEffectData
{
    /// <summary>
    /// The position of the map effect.
    /// </summary>
    public readonly uint Position;

    /// <summary>
    /// The first parameter of the map effect.
    /// </summary>
    public readonly ushort Param1;

    /// <summary>
    /// The second parameter of the map effect.
    /// </summary>
    public readonly ushort Param2;

    /// <summary>
    /// The time when the map effect was created.
    /// </summary>
    public readonly DateTime Time;

    /// <summary>
    /// Gets the duration since the map effect was created.
    /// </summary>
    public readonly TimeSpan TimeDuration => DateTime.UtcNow - Time;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapEffectData"/> struct.
    /// </summary>
    /// <param name="position">The position of the map effect.</param>
    /// <param name="param1">The first parameter of the map effect.</param>
    /// <param name="param2">The second parameter of the map effect.</param>
    public MapEffectData(uint position, ushort param1, ushort param2)
    {
        Time = DateTime.UtcNow;
        Position = position;
        Param1 = param1;
        Param2 = param2;
    }

    /// <summary>
    /// Returns a string representation of the map effect data.
    /// </summary>
    /// <returns>A string representation of the map effect data.</returns>
    public override string ToString() => $"MapEffect: Pos: {Position}, P1: {Param1}, P2: {Param2}";
}