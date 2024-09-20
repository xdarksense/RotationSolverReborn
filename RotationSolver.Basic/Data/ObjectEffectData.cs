using ECommons.DalamudServices;

namespace RotationSolver.Basic.Data;

/// <summary>
/// Represents the data for an object effect.
/// </summary>
internal readonly struct ObjectEffectData
{
    /// <summary>
    /// The ID of the object.
    /// </summary>
    public readonly ulong ObjectId;

    /// <summary>
    /// The first parameter of the object effect.
    /// </summary>
    public readonly ushort Param1;

    /// <summary>
    /// The second parameter of the object effect.
    /// </summary>
    public readonly ushort Param2;

    /// <summary>
    /// The time when the object effect was created.
    /// </summary>
    public readonly DateTime Time;

    /// <summary>
    /// Gets the duration since the object effect was created.
    /// </summary>
    public readonly TimeSpan TimeDuration => DateTime.UtcNow - Time;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectEffectData"/> struct.
    /// </summary>
    /// <param name="objectId">The ID of the object.</param>
    /// <param name="param1">The first parameter of the object effect.</param>
    /// <param name="param2">The second parameter of the object effect.</param>
    public ObjectEffectData(ulong objectId, ushort param1, ushort param2)
    {
        Time = DateTime.UtcNow;
        ObjectId = objectId;
        Param1 = param1;
        Param2 = param2;
    }

    /// <summary>
    /// Returns a string representation of the object effect data.
    /// </summary>
    /// <returns>A string representation of the object effect data.</returns>
    public override string ToString() => $"Object Effect: {Svc.Objects.SearchById(ObjectId)?.Name ?? "Object"}, P1: {Param1}, P2: {Param2}";
}