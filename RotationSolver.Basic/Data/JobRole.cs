using ECommons.ExcelServices;
using Lumina.Excel.GeneratedSheets;

namespace RotationSolver.Basic.Data;

/// <summary>
/// The role of jobs.
/// </summary>
public enum JobRole : byte
{
    /// <summary>
    /// No specific role.
    /// </summary>
    None = 0,

    /// <summary>
    /// Tank role.
    /// </summary>
    Tank = 1,

    /// <summary>
    /// Melee role.
    /// </summary>
    Melee = 2,

    // Uncomment and document if needed in the future.
    ///// <summary>
    ///// Ranged role.
    ///// </summary>
    //Ranged = 3,

    /// <summary>
    /// Healer role.
    /// </summary>
    Healer = 4,

    /// <summary>
    /// Ranged physical role.
    /// </summary>
    RangedPhysical = 5,

    /// <summary>
    /// Ranged magical role.
    /// </summary>
    RangedMagical = 6,

    /// <summary>
    /// Disciple of the Land role.
    /// </summary>
    DiscipleOfTheLand = 7,

    /// <summary>
    /// Disciple of the Hand role.
    /// </summary>
    DiscipleOfTheHand = 8,
}

/// <summary>
/// Extension methods for the JobRole enum.
/// </summary>
public static class JobRoleExtension
{
    /// <summary>
    /// Gets the job role from a class.
    /// </summary>
    /// <param name="job">The class job.</param>
    /// <returns>The job role.</returns>
    public static JobRole GetJobRole(this ClassJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        var role = (JobRole)job.Role;

        if (role is (JobRole)3 or JobRole.None)
        {
            role = job.ClassJobCategory.Row switch
            {
                30 => JobRole.RangedPhysical,
                31 => JobRole.RangedMagical,
                // Uncomment and document if needed in the future.
                32 => JobRole.DiscipleOfTheLand,
                33 => JobRole.DiscipleOfTheHand,
                _ => JobRole.None,
            };
        }
        return role;
    }

    /// <summary>
    /// Gets the jobs from a role.
    /// </summary>
    /// <param name="role">The job role.</param>
    /// <returns>An array of jobs corresponding to the role.</returns>
    public static Job[] ToJobs(this JobRole role)
    {
        return role switch
        {
            JobRole.Tank => new[] { Job.WAR, Job.PLD, Job.DRK, Job.GNB },
            JobRole.Healer => new[] { Job.WHM, Job.SCH, Job.AST, Job.SGE },
            JobRole.Melee => new[] { Job.MNK, Job.DRG, Job.NIN, Job.SAM, Job.RPR, Job.VPR },
            JobRole.RangedPhysical => new[] { Job.BRD, Job.MCH, Job.DNC },
            JobRole.RangedMagical => new[] { Job.BLM, Job.SMN, Job.RDM, Job.BLU, Job.PCT },
            _ => Array.Empty<Job>(),
        };
    }
}