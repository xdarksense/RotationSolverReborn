namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
    private static readonly ObjectListDelay<IBattleChara>
        _raisePartyTargets = new(() => Service.Config.RaiseDelay),
        _raiseAllTargets = new(() => Service.Config.RaiseDelay);

    private static DateTime _lastUpdateTimeToKill = DateTime.MinValue;
    private static readonly TimeSpan TimeToKillUpdateInterval = TimeSpan.FromSeconds(0.5);

    internal static void UpdateTarget()
    {
        UpdateTimeToKill();
    }

    private static void UpdateTimeToKill()
    {
        var now = DateTime.Now;
        if (now - _lastUpdateTimeToKill < TimeToKillUpdateInterval) return;
        _lastUpdateTimeToKill = now;

        if (DataCenter.RecordedHP.Count >= DataCenter.HP_RECORD_TIME)
        {
            DataCenter.RecordedHP.Dequeue();
        }

        var currentHPs = DataCenter.AllTargets
            .Where(b => b.CurrentHp != 0)
            .ToDictionary(b => b.GameObjectId, b => b.GetHealthRatio());

        DataCenter.RecordedHP.Enqueue((now, new SortedList<ulong, float>(currentHPs)));
    }
}