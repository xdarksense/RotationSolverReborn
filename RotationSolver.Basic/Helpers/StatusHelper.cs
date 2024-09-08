using Dalamud.Game.ClientState.Statuses;
using ECommons.Automation;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The helper for the status.
/// </summary>
public static class StatusHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] RangePhysicalDefense { get; } =
    [
        StatusID.Troubadour,
        StatusID.Tactician_1951,
        StatusID.Tactician_2177,
        StatusID.ShieldSamba,
    ];

    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] PhysicalResistance { get; } =
    [
        StatusID.IceSpikes_1720,
    ];

    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] PhysicalRangedResistance { get; } =
    [
        StatusID.RangedResistance,
    ];

    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] MagicResistance { get; } =
    [
        StatusID.MagicResistance,
        StatusID.RepellingSpray_556,
        StatusID.MagitekField_2166,
    ];


    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] AreaHots { get; } =
    [
        StatusID.AspectedHelios,
        StatusID.MedicaIi,
        StatusID.TrueMedicaIi,
        StatusID.PhysisIi,
        StatusID.Physis,
        StatusID.SacredSoil_1944,
        StatusID.WhisperingDawn,
        StatusID.AngelsWhisper,
        StatusID.Seraphism_3885,
        StatusID.Asylum_1911,
        StatusID.DivineAura,
        StatusID.MedicaIii_3986,
        StatusID.MedicaIii
    ];

    /// <summary>
    /// 
    /// </summary>
    public static StatusID[] SingleHots { get; } =
    [
        StatusID.AspectedBenefic,
        StatusID.Regen,
        StatusID.Regen_897,
        StatusID.Regen_1330,
        StatusID.TheEwer_3891,
    ];

    internal static StatusID[] TankStanceStatus { get; } =
    [
        StatusID.Grit,
        StatusID.RoyalGuard_1833,
        StatusID.IronWill,
        StatusID.Defiance,
    ];

    internal static StatusID[] NoNeedHealingStatus { get; } =
    [
        StatusID.Holmgang_409,
        StatusID.LivingDead,
        //StatusID.WalkingDead,
        StatusID.Superbolide,
    ];

    internal static StatusID[] SwiftcastStatus { get; } =
    [
        StatusID.Swiftcast,
        StatusID.Triplecast,
        StatusID.Dualcast,
    ];

    internal static StatusID[] AstCardStatus { get; } =
    [
        StatusID.TheBalance_3887,
        StatusID.TheSpear_3889,

        StatusID.Weakness,
        StatusID.BrinkOfDeath,
    ];

    internal static StatusID[] RampartStatus { get; } =
    [
        StatusID.Superbolide,
        StatusID.HallowedGround,
        StatusID.Rampart,
        StatusID.Bulwark,
        StatusID.Bloodwhetting,
        StatusID.Vengeance,
        StatusID.Sentinel,
        StatusID.ShadowWall,
        StatusID.Nebula,
        StatusID.GreatNebula,
        .. NoNeedHealingStatus,
    ];

    internal static StatusID[] NoPositionalStatus { get; } =
    [
        StatusID.TrueNorth,
        StatusID.RightEye,
    ];

    /// <summary>
    /// Check whether the target needs to be healing.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool NeedHealing(this IGameObject p) => p.WillStatusEndGCD(2, 0, false, NoNeedHealingStatus);

    /// <summary>
    /// Will any of <paramref name="statusIDs"/> end after <paramref name="gcdCount"/> GCDs plus <paramref name="offset"/> seconds?
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="gcdCount"></param>
    /// <param name="offset"></param>
    /// <param name="isFromSelf"></param>
    /// <param name="statusIDs"></param>
    /// <returns></returns>
    public static bool WillStatusEndGCD(this IGameObject obj, uint gcdCount = 0, float offset = 0, bool isFromSelf = true, params StatusID[] statusIDs)
        => WillStatusEnd(obj, DataCenter.GCDTime(gcdCount, offset), isFromSelf, statusIDs);

    /// <summary>
    /// Will any of <paramref name="statusIDs"/> end after <paramref name="time"/> seconds?
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="time"></param>
    /// <param name="isFromSelf"></param>
    /// <param name="statusIDs"></param>
    /// <returns></returns>
    public static bool WillStatusEnd(this IGameObject obj, float time, bool isFromSelf = true, params StatusID[] statusIDs)
    {
        if (DataCenter.HasApplyStatus(obj.GameObjectId, statusIDs)) return false;
        var remain = obj.StatusTime(isFromSelf, statusIDs);
        if (remain < 0 && obj.HasStatus(isFromSelf, statusIDs)) return false;
        return remain <= time;
    }

    /// <summary>
    /// Get the remaining time of the status.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isFromSelf"></param>
    /// <param name="statusIDs"></param>
    /// <returns></returns>
    public static float StatusTime(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        try
        {
            if (obj == null || DataCenter.HasApplyStatus(obj.GameObjectId, statusIDs)) return float.MaxValue;
            var times = obj.StatusTimes(isFromSelf, statusIDs);
            if (times == null || !times.Any()) return 0;
            return Math.Max(0, times.Min() - DataCenter.DefaultGCDRemain);
        }
        catch
        {
            return 0;
        }
    }

    internal static IEnumerable<float> StatusTimes(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        return obj.GetStatus(isFromSelf, statusIDs).Select(status => status.RemainingTime == 0 ? float.MaxValue : status.RemainingTime);
    }

    /// <summary>
    /// Get the stack count of the status.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isFromSelf"></param>
    /// <param name="statusIDs"></param>
    /// <returns></returns>
    public static byte StatusStack(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        if (obj == null || DataCenter.HasApplyStatus(obj.GameObjectId, statusIDs)) return byte.MaxValue;
        var stacks = obj.StatusStacks(isFromSelf, statusIDs);
        if (stacks == null || !stacks.Any()) return 0;
        return stacks.Min();
    }

    private static IEnumerable<byte> StatusStacks(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        return obj.GetStatus(isFromSelf, statusIDs).Select(status => status.StackCount == 0 ? byte.MaxValue : status.StackCount);
    }

    /// <summary>
    /// Check if the object has any of the specified statuses.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isFromSelf"></param>
    /// <param name="statusIDs"></param>
    /// <returns></returns>
    public static bool HasStatus(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        if (obj == null || DataCenter.HasApplyStatus(obj.GameObjectId, statusIDs)) return true;
        return obj.GetStatus(isFromSelf, statusIDs).Any();
    }

    /// <summary>
    /// Remove the specified status.
    /// </summary>
    /// <param name="status"></param>
    public static void StatusOff(StatusID status)
    {
        if (Player.Object == null || !Player.Object.HasStatus(false, status)) return;
        Chat.Instance.SendMessage($"/statusoff {GetStatusName(status)}");
    }

    internal static string GetStatusName(StatusID id)
    {
        return Service.GetSheet<Lumina.Excel.GeneratedSheets.Status>().GetRow((uint)id)!.Name.ToString();
    }

    private static IEnumerable<Status> GetStatus(this IGameObject obj, bool isFromSelf, params StatusID[] statusIDs)
    {
        // Convert statusIDs to a HashSet for faster lookups
        var newEffects = new HashSet<uint>(statusIDs.Select(a => (uint)a));
        var allStatuses = obj.GetAllStatus(isFromSelf);
        return allStatuses.Where(status => newEffects.Contains(status.StatusId));
    }

    private static IEnumerable<Status> GetAllStatus(this IGameObject obj, bool isFromSelf)
    {
        if (obj is not IBattleChara b) return Enumerable.Empty<Status>();

        var playerId = Player.Object?.GameObjectId ?? 0;
        // Ensure b.StatusList is not null
        return b.StatusList?.Where(status => !isFromSelf
                                              || status.SourceId == playerId
                                              || status.SourceObject?.OwnerId == playerId)
                             ?? Enumerable.Empty<Status>();
    }

    /// <summary>
    /// Check if the status is invincible.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsInvincible(this Status status)
    {
        if (status.GameData.Icon == 15024) return true;
        return OtherConfiguration.InvincibleStatus.Any(id => (uint)id == status.StatusId);
    }

    /// <summary>
    /// Check if the status is a priority.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsPriority(this Status status)
    {
        return OtherConfiguration.PriorityStatus.Any(id => (uint)id == status.StatusId);
    }

    /// <summary>
    /// Check if the status needs to be dispelled immediately.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool IsDangerous(this Status status)
    {
        if (!status.CanDispel()) return false;
        if (status.StackCount > 2) return true;
        if (status.RemainingTime > 20) return true;
        return OtherConfiguration.DangerousStatus.Any(id => id == status.StatusId);
    }

    /// <summary>
    /// Check if the status can be dispelled.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool CanDispel(this Status status)
    {
        return status.GameData.CanDispel && status.RemainingTime > 1 + DataCenter.DefaultGCDRemain;
    }
}
