using ECommons.ExcelServices;
using Lumina.Excel.Sheets;

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
    public static IEnumerable<IBattleChara> GetDeath(this IEnumerable<IBattleChara> charas)
    {
        if (charas == null)
        {
            return Enumerable.Empty<IBattleChara>();
        }

        List<IBattleChara> result = [];
        foreach (IBattleChara item in charas)
        {
            if (item == null || !item.IsDead || item.CurrentHp != 0 || !item.IsTargetable)
            {
                continue;
            }

            if (item.HasStatus(false, StatusID.Raise))
            {
                continue;
            }

            if (!Service.Config.RaiseBrinkOfDeath && item.HasStatus(false, StatusID.BrinkOfDeath))
            {
                continue;
            }

            result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Get the specific roles members.
    /// </summary>
    /// <param name="objects">The list of objects.</param>
    /// <param name="roles">The roles to filter by.</param>
    /// <returns>The objects that match the roles.</returns>
    public static IEnumerable<IBattleChara> GetJobCategory(this IEnumerable<IBattleChara> objects, params JobRole[] roles)
    {
        if (objects == null || roles == null || roles.Length == 0)
        {
            return Enumerable.Empty<IBattleChara>();
        }

        HashSet<byte> validJobs = [];
        Lumina.Excel.ExcelSheet<ClassJob> classJobs = Service.GetSheet<ClassJob>();
        if (classJobs == null)
        {
            return Enumerable.Empty<IBattleChara>();
        }

        foreach (JobRole role in roles)
        {
            foreach (ClassJob job in classJobs)
            {
                if (role == job.GetJobRole())
                {
                    _ = validJobs.Add((byte)job.RowId);
                }
            }
        }

        List<IBattleChara> result = [];
        foreach (IBattleChara obj in objects)
        {
            if (obj != null && obj.IsJobs(validJobs))
            {
                result.Add(obj);
            }
        }

        return result;
    }

    /// <summary>
    /// Is the target the role.
    /// </summary>
    /// <param name="obj">The game object.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the object is of the specified role, otherwise false.</returns>
    public static bool IsJobCategory(this IGameObject obj, JobRole role)
    {
        if (obj == null)
        {
            return false;
        }

        HashSet<byte> validJobs = [];
        Lumina.Excel.ExcelSheet<ClassJob> classJobs = Service.GetSheet<ClassJob>();
        if (classJobs == null)
        {
            return false;
        }

        foreach (ClassJob job in classJobs)
        {
            if (role == job.GetJobRole())
            {
                _ = validJobs.Add((byte)job.RowId);
            }
        }

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
        if (obj == null || validJobs == null || validJobs.Length == 0)
        {
            return false;
        }

        HashSet<byte> validJobSet = [];
        foreach (Job job in validJobs)
        {
            _ = validJobSet.Add((byte)(uint)job);
        }

        return obj.IsJobs(validJobSet);
    }

    private static bool IsJobs(this IGameObject obj, HashSet<byte> validJobs)
    {
        return obj is IBattleChara b && validJobs != null && validJobs.Contains((byte)b.ClassJob.Value.RowId);
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
    {
        if (objects == null)
        {
            return Enumerable.Empty<T>();
        }

        List<T> result = [];
        foreach (T obj in objects)
        {
            if (obj.DistanceToPlayer() <= radius)
            {
                result.Add(obj);
            }
        }
        return result;
    }
}