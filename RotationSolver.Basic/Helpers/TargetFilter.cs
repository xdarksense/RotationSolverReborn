using ECommons.ExcelServices;
using Lumina.Excel.GeneratedSheets;
using System.Data;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The filter for target.
/// </summary>
public static class TargetFilter
{
    #region Find one target
    /// <summary>
    /// Get the dead ones in the list.
    /// </summary>
    /// <param name="charas">The list of characters.</param>
    /// <returns>The dead characters.</returns>
    public static IEnumerable<IBattleChara> GetDeath(this IEnumerable<IBattleChara> charas) => charas.Where(item =>
    {
        if (item == null || !item.IsDead || item.CurrentHp != 0 || !item.IsTargetable) return false;
        if (item.HasStatus(false, StatusID.Raise)) return false;
        if (!Service.Config.RaiseBrinkOfDeath && item.HasStatus(false, StatusID.BrinkOfDeath)) return false;
        if (DataCenter.AllianceMembers.Any(c => c.CastTargetObjectId == item.GameObjectId)) return false;
        return true;
    });

    /// <summary>
    /// Get the specific roles members.
    /// </summary>
    /// <param name="objects">The list of objects.</param>
    /// <param name="roles">The roles to filter by.</param>
    /// <returns>The objects that match the roles.</returns>
    public static IEnumerable<IBattleChara> GetJobCategory(this IEnumerable<IBattleChara> objects, params JobRole[] roles)
    {
        var validJobs = roles.SelectMany(role => Service.GetSheet<ClassJob>()
            .Where(job => role == job.GetJobRole())
            .Select(job => (byte)job.RowId))
            .ToHashSet();

        return objects.Where(obj => obj.IsJobs(validJobs));
    }

    /// <summary>
    /// Is the target the role.
    /// </summary>
    /// <param name="obj">The game object.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the object is of the specified role, otherwise false.</returns>
    public static bool IsJobCategory(this IGameObject obj, JobRole role)
    {
        var validJobs = new HashSet<byte>(Service.GetSheet<ClassJob>()
            .Where(job => role == job.GetJobRole())
            .Select(job => (byte)job.RowId));

        return obj.IsJobs(validJobs);
    }

    /// <summary>
    /// Is the target in the jobs.
    /// </summary>
    /// <param name="obj">The game object.</param>
    /// <param name="validJobs">The valid jobs.</param>
    /// <returns>True if the object is in the valid jobs, otherwise false.</returns>
    public static bool IsJobs(this IGameObject obj, params Job[] validJobs)
    {
        return obj.IsJobs(new HashSet<byte>(validJobs.Select(j => (byte)(uint)j)));
    }

    private static bool IsJobs(this IGameObject obj, HashSet<byte> validJobs)
    {
        if (obj is not IBattleChara b) return false;
        return validJobs.Contains((byte?)b.ClassJob.GameData?.RowId ?? 0);
    }
    #endregion

    /// <summary>
    /// Get the <paramref name="objects"/> in <paramref name="radius"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects.</typeparam>
    /// <param name="objects">The list of objects.</param>
    /// <param name="radius">The radius to filter by.</param>
    /// <returns>The objects within the radius.</returns>
    public static IEnumerable<T> GetObjectInRadius<T>(this IEnumerable<T> objects, float radius) where T : IGameObject
        => objects.Where(o => o.DistanceToPlayer() <= radius);
}
