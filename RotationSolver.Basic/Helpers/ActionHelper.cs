using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationSolver.Basic.Helpers;

internal static class ActionHelper
{
    internal const byte GCDCooldownGroup = 58;

    internal static ActionCate GetActionCate(this Action action)
    {
        return (ActionCate)(action.ActionCategory.Value?.RowId ?? 0);
    }

    internal static bool IsGeneralGCD(this Action action)
    {
        return action.CooldownGroup == GCDCooldownGroup;
    }

    internal static bool IsRealGCD(this Action action)
    {
        return action.IsGeneralGCD() || action.AdditionalCooldownGroup == GCDCooldownGroup;
    }

    internal static byte GetCoolDownGroup(this Action action)
    {
        var group = action.IsGeneralGCD() ? action.AdditionalCooldownGroup : action.CooldownGroup;
        return group == 0 ? GCDCooldownGroup : group;
    }

    internal static bool IsInJob(this Action action)
    {
        var cate = action.ClassJobCategory.Value;
        if (cate != null)
        {
            var inJob = (bool?)cate.GetType().GetProperty(DataCenter.Job.ToString())?.GetValue(cate);
            return inJob.GetValueOrDefault(true);
        }
        return true;
    }

    internal static bool CanUseGCD
    {
        get
        {
            var maxAhead = DataCenter.ActionAhead;
            return DataCenter.DefaultGCDRemain <= maxAhead;
        }
    }
}