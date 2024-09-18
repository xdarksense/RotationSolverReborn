using System.Diagnostics.CodeAnalysis;

namespace RotationSolver.Basic.Configuration.RotationConfig;

/// <summary>
/// Compares two <see cref="IRotationConfig"/> objects for equality based on their names.
/// </summary>
internal class RotationConfigComparer : IEqualityComparer<IRotationConfig>
{
    /// <summary>
    /// Determines whether the specified <see cref="IRotationConfig"/> objects are equal.
    /// </summary>
    /// <param name="x">The first <see cref="IRotationConfig"/> to compare.</param>
    /// <param name="y">The second <see cref="IRotationConfig"/> to compare.</param>
    /// <returns><c>true</c> if the specified <see cref="IRotationConfig"/> objects are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(IRotationConfig? x, IRotationConfig? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.Name.Equals(y.Name);
    }

    /// <summary>
    /// Returns a hash code for the specified <see cref="IRotationConfig"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IRotationConfig"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified <see cref="IRotationConfig"/>.</returns>
    public int GetHashCode([DisallowNull] IRotationConfig obj)
    {
        if (obj.Name == null) throw new ArgumentNullException(nameof(obj.Name));
        return obj.Name.GetHashCode();
    }
}