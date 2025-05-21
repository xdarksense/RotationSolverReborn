using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
    private static readonly ObjectListDelay<IBattleChara>
        _raisePartyTargets = new(() => Service.Config.RaiseDelay),
        _raiseAllTargets = new(() => Service.Config.RaiseDelay);

    private static DateTime _lastUpdateTimeToKill = DateTime.MinValue;
    private static readonly TimeSpan TimeToKillUpdateInterval = TimeSpan.FromSeconds(0.1);

    internal static void UpdateTargets()
    {
        DataCenter.AllTargets = GetAllTargets();
        if (DataCenter.AllTargets != null)
        {
            DataCenter.PartyMembers = GetPartyMembers();
            DataCenter.AllianceMembers = GetAllianceMembers();
            DataCenter.AllHostileTargets = GetAllHostileTargets();
            DataCenter.DeathTarget = GetDeathTarget();
            DataCenter.DispelTarget = GetDispelTarget();
            DataCenter.ProvokeTarget = GetFirstHostileTarget(ObjectHelper.CanProvoke);
            DataCenter.InterruptTarget = GetFirstHostileTarget(ObjectHelper.CanInterrupt);
        }
        UpdateTimeToKill();
    }

    private static List<IBattleChara> GetAllTargets()
    {
        var allTargets = new List<IBattleChara>();
        bool skipDummyCheck = !Service.Config.DisableTargetDummys;
        foreach (IBattleChara battleChara in Svc.Objects.OfType<IBattleChara>())
        {
            if (skipDummyCheck || !battleChara.IsDummy())
            {
                allTargets.Add(battleChara);
            }
        }
        return allTargets;
    }

    private static unsafe List<IBattleChara> GetPartyMembers()
    {
        var partyMembers = new List<IBattleChara>();
        try
        {
            foreach (var member in DataCenter.AllTargets)
            {
                if (member.StatusList == null || !member.IsParty()) continue;
                var character = member.Character();
                if (character == null) continue;
                var status = character->CharacterData.OnlineStatus;
                if (status != 15 && status != 5 && member.IsTargetable)
                {
                    partyMembers.Add(member);
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in GetPartyMembers: {ex.Message}");
        }
        return partyMembers;
    }

    private static unsafe List<IBattleChara> GetAllianceMembers()
    {
        var allianceMembers = new List<IBattleChara>();
        try
        {
            foreach (var target in DataCenter.AllTargets)
            {
                if (ObjectHelper.IsAllianceMember(target) && !target.IsParty() && target.Character() != null &&
                    target.Character()->CharacterData.OnlineStatus != 15 &&
                    target.Character()->CharacterData.OnlineStatus != 5 && target.IsTargetable)
                {
                    allianceMembers.Add(target);
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in GetAllianceMembers: {ex.Message}");
        }
        return allianceMembers;
    }

    private static List<IBattleChara> GetAllHostileTargets()
    {
        var hostileTargets = new List<IBattleChara>();
        try
        {
            foreach (var target in DataCenter.AllTargets)
            {
                if (target.StatusList == null || !target.IsEnemy() || !target.IsTargetable) continue;

                bool hasInvincible = false;
                var statusList = target.StatusList;
                for (int i = 0; i < statusList.Length; i++)
                {
                    var status = statusList[i];
                    if (status != null && StatusHelper.IsInvincible(status))
                    {
                        hasInvincible = true;
                        break;
                    }
                }
                if (hasInvincible &&
                    (DataCenter.IsPvP && !Service.Config.IgnorePvPInvincibility || !DataCenter.IsPvP)) continue;

                hostileTargets.Add(target);
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in GetAllHostileTargets: {ex.Message}");
        }
        return hostileTargets;
    }

    private static IBattleChara? GetFirstHostileTarget(Func<IBattleChara, bool> predicate)
    {
        foreach (var target in DataCenter.AllHostileTargets)
        {
            try
            {
                if (predicate(target))
                {
                    return target;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error in GetFirstHostileTarget: {ex.Message}");
            }
        }
        return null;
    }

    private static IBattleChara? GetDeathTarget()
    {
        if (Player.Job == Job.WHM || Player.Job == Job.SCH || Player.Job == Job.AST || Player.Job == Job.SGE ||
            Player.Job == Job.SMN || Player.Job == Job.RDM)
        {
            try
            {
                var deathParty = new List<IBattleChara>();
                if (DataCenter.PartyMembers != null)
                {
                    foreach (var target in DataCenter.PartyMembers.GetDeath())
                    {
                        if (!target.IsEnemy())
                        {
                            deathParty.Add(target);
                        }
                    }
                }
                var deathAll = new List<IBattleChara>();
                foreach (var target in DataCenter.AllTargets.GetDeath())
                {
                    if (!target.IsEnemy())
                    {
                        deathAll.Add(target);
                    }
                }
                var deathAllianceMembers = new List<IBattleChara>();
                if (DataCenter.AllianceMembers != null)
                {
                    foreach (var target in DataCenter.AllianceMembers.GetDeath())
                    {
                        if (!target.IsEnemy())
                        {
                            deathAllianceMembers.Add(target);
                        }
                    }
                }
                var deathAllianceHealers = new List<IBattleChara>(deathParty);
                var deathAllianceSupports = new List<IBattleChara>(deathParty);

                if (DataCenter.AllianceMembers != null)
                {
                    foreach (var member in DataCenter.AllianceMembers)
                    {
                        if (member.IsJobCategory(JobRole.Healer))
                        {
                            deathAllianceHealers.Add(member);
                        }
                        if (member.IsJobCategory(JobRole.Healer) || member.IsJobCategory(JobRole.Tank))
                        {
                            deathAllianceSupports.Add(member);
                        }
                    }
                }

                var raisePartyAndAllianceSupports = new List<IBattleChara>(deathParty);
                raisePartyAndAllianceSupports.AddRange(deathAllianceSupports);

                var raisePartyAndAllianceHealers = new List<IBattleChara>(deathParty);
                raisePartyAndAllianceHealers.AddRange(deathAllianceHealers);

                var raisetype = Service.Config.RaiseType;

                var validRaiseTargets = new List<IBattleChara>();

                if (raisetype == RaiseType.PartyOnly)
                {
                    validRaiseTargets.AddRange(deathParty);
                }
                else if (raisetype == RaiseType.PartyAndAllianceSupports)
                {
                    validRaiseTargets.AddRange(raisePartyAndAllianceSupports);
                }
                else if (raisetype == RaiseType.PartyAndAllianceHealers)
                {
                    validRaiseTargets.AddRange(raisePartyAndAllianceHealers);
                }
                else if (raisetype == RaiseType.All)
                {
                    validRaiseTargets.AddRange(deathAll);
                }

                foreach (RaiseType type in Enum.GetValues(typeof(RaiseType)))
                {
                    var deathTarget = GetPriorityDeathTarget(validRaiseTargets, type);
                    if (deathTarget != null) return deathTarget;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error in GetDeathTarget: {ex.Message}");
            }
        }
        return null;
    }

    private static IBattleChara? GetPriorityDeathTarget(List<IBattleChara> validRaiseTargets, RaiseType raiseType = RaiseType.PartyOnly)
    {
        if (validRaiseTargets.Count == 0) return null;

        var deathTanks = new List<IBattleChara>();
        var deathHealers = new List<IBattleChara>();
        var deathOffHealers = new List<IBattleChara>();
        var deathOthers = new List<IBattleChara>();

        foreach (var chara in validRaiseTargets)
        {
            if (chara.IsJobCategory(JobRole.Tank))
            {
                deathTanks.Add(chara);
            }
            else if (chara.IsJobCategory(JobRole.Healer))
            {
                deathHealers.Add(chara);
            }
            else if (Service.Config.OffRaiserRaise && chara.IsJobs(Job.SMN, Job.RDM))
            {
                deathOffHealers.Add(chara);
            }
            else
            {
                deathOthers.Add(chara);
            }
        }

        if (raiseType == RaiseType.PartyAndAllianceHealers && deathHealers.Count > 0)
        {
            return deathHealers[0];
        }

        if (Service.Config.H2)
        {
            deathOffHealers.Reverse();
            deathOthers.Reverse();
        }

        if (deathTanks.Count > 1) return deathTanks[0];
        if (deathHealers.Count > 0) return deathHealers[0];
        if (deathTanks.Count > 0) return deathTanks[0];
        if (Service.Config.OffRaiserRaise && deathOffHealers.Count > 0) return deathOffHealers[0];

        return deathOthers.Count > 0 ? deathOthers[0] : null;
    }

    private static IBattleChara? GetDispelTarget()
    {
        if (Player.Job == Job.WHM || Player.Job == Job.SCH || Player.Job == Job.AST || Player.Job == Job.SGE ||
            Player.Job == Job.BRD)
        {
            var weakenPeople = new List<IBattleChara>();
            var dyingPeople = new List<IBattleChara>();

            AddDispelTargets(DataCenter.PartyMembers, weakenPeople);

            foreach (var person in weakenPeople)
            {
                bool hasDangerous = false;
                if (person.StatusList != null)
                {
                    for (int i = 0; i < person.StatusList.Length; i++)
                    {
                        var status = person.StatusList[i];
                        if (status != null && status.IsDangerous())
                        {
                            hasDangerous = true;
                            break;
                        }
                    }
                }
                if (hasDangerous)
                {
                    dyingPeople.Add(person);
                }
            }

            return GetClosestTarget(dyingPeople) ?? GetClosestTarget(weakenPeople);
        }
        return null;
    }

    private static void AddDispelTargets(List<IBattleChara>? members, List<IBattleChara> targetList)
    {
        if (members == null) return;

        foreach (var member in members)
        {
            try
            {
                if (member.StatusList != null)
                {
                    for (int i = 0; i < member.StatusList.Length; i++)
                    {
                        var status = member.StatusList[i];
                        if (status != null && status.CanDispel())
                        {
                            targetList.Add(member);
                            break; // Add only once per member if any status can be dispelled
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                PluginLog.Error($"NullReferenceException in AddDispelTargets for member {member?.ToString()}: {ex.Message}");
            }
        }
    }

    private static IBattleChara? GetClosestTarget(List<IBattleChara> targets)
    {
        IBattleChara? closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var target in targets)
        {
            var distance = ObjectHelper.DistanceToPlayer(target);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
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

        var currentHPs = new SortedList<ulong, float>();
        foreach (var target in DataCenter.AllTargets)
        {
            if (target.CurrentHp != 0)
            {
                currentHPs[target.GameObjectId] = target.GetHealthRatio();
            }
        }

        DataCenter.RecordedHP.Enqueue((now, currentHPs));
    }
}