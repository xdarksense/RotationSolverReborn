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
        List<IBattleChara> allTargets = [];
        bool skipDummyCheck = !Service.Config.DisableTargetDummys;
        foreach (var obj in Svc.Objects)
        {
            if (obj is IBattleChara battleChara)
            {
                if ((skipDummyCheck || !battleChara.IsDummy()) && battleChara.StatusList != null)
                {
                    allTargets.Add(battleChara);
                }
            }
        }
        return allTargets;
    }

    private static unsafe List<IBattleChara> GetPartyMembers()
    {
        List<IBattleChara> partyMembers = [];
        try
        {
            foreach (IBattleChara member in DataCenter.AllTargets)
            {
                if (!member.IsParty())
                {
                    continue;
                }

                FFXIVClientStructs.FFXIV.Client.Game.Character.Character* character = member.Character();
                if (character == null)
                {
                    continue;
                }

                byte status = character->CharacterData.OnlineStatus;
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
        List<IBattleChara> allianceMembers = [];
        RaiseType raisetype = Service.Config.RaiseType;
        if (raisetype != RaiseType.PartyOnly)
        {
            try
            {
                foreach (IBattleChara target in DataCenter.AllTargets)
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
        }
        return allianceMembers;
    }

    private static List<IBattleChara> GetAllHostileTargets()
    {
        List<IBattleChara> hostileTargets = [];
        try
        {
            foreach (IBattleChara target in DataCenter.AllTargets)
            {
                if (!target.IsEnemy() || !target.IsTargetable)
                {
                    continue;
                }

                bool hasInvincible = false;
                Dalamud.Game.ClientState.Statuses.StatusList statusList = target.StatusList;
                for (int i = 0; i < statusList.Length; i++)
                {
                    Dalamud.Game.ClientState.Statuses.Status? status = statusList[i];
                    if (status != null && StatusHelper.IsInvincible(status))
                    {
                        hasInvincible = true;
                        break;
                    }
                }
                if (hasInvincible &&
                    ((DataCenter.IsPvP && !Service.Config.IgnorePvPInvincibility) || !DataCenter.IsPvP))
                {
                    continue;
                }

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
        foreach (IBattleChara target in DataCenter.AllHostileTargets)
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
        if (Player.Job is Job.WHM or Job.SCH or Job.AST or Job.SGE or
            Job.SMN or Job.RDM)
        {
            try
            {
                List<IBattleChara> deathParty = [];
                if (DataCenter.PartyMembers != null)
                {
                    foreach (IBattleChara target in DataCenter.PartyMembers.GetDeath())
                    {
                        if (!target.IsEnemy() && !target.IsTargetMoving())
                        {
                            deathParty.Add(target);
                        }
                    }
                }
                List<IBattleChara> deathAll = [];
                foreach (IBattleChara target in DataCenter.AllTargets.GetDeath())
                {
                    if (!target.IsEnemy() && !target.IsTargetMoving())
                    {
                        deathAll.Add(target);
                    }
                }
                List<IBattleChara> deathAllianceMembers = [];
                if (DataCenter.AllianceMembers != null)
                {
                    foreach (IBattleChara target in DataCenter.AllianceMembers.GetDeath())
                    {
                        if (!target.IsEnemy() && !target.IsTargetMoving())
                        {
                            deathAllianceMembers.Add(target);
                        }
                    }
                }
                List<IBattleChara> deathAllianceHealers = [.. deathParty];
                List<IBattleChara> deathAllianceSupports = [.. deathParty];

                if (DataCenter.AllianceMembers != null)
                {
                    foreach (IBattleChara member in DataCenter.AllianceMembers)
                    {
                        if (member.IsJobCategory(JobRole.Healer) && !member.IsTargetMoving())
                        {
                            deathAllianceHealers.Add(member);
                        }
                        if ((member.IsJobCategory(JobRole.Healer) || member.IsJobCategory(JobRole.Tank)) && !member.IsTargetMoving())
                        {
                            deathAllianceSupports.Add(member);
                        }
                    }
                }

                List<IBattleChara> raisePartyAndAllianceSupports = [.. deathParty, .. deathAllianceSupports];

                List<IBattleChara> raisePartyAndAllianceHealers = [.. deathParty, .. deathAllianceHealers];

                RaiseType raisetype = Service.Config.RaiseType;

                List<IBattleChara> validRaiseTargets = [];

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

                foreach (RaiseType type in Enum.GetValues<RaiseType>())
                {
                    IBattleChara? deathTarget = GetPriorityDeathTarget(validRaiseTargets, type);
                    if (deathTarget != null)
                    {
                        return deathTarget;
                    }
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
        if (validRaiseTargets.Count == 0)
        {
            return null;
        }

        List<IBattleChara> deathTanks = [];
        List<IBattleChara> deathHealers = [];
        List<IBattleChara> deathOffHealers = [];
        List<IBattleChara> deathOthers = [];

        foreach (IBattleChara chara in validRaiseTargets)
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

        if (deathTanks.Count > 1)
        {
            return deathTanks[0];
        }

        return deathHealers.Count > 0
            ? deathHealers[0]
            : deathTanks.Count > 0
            ? deathTanks[0]
            : Service.Config.OffRaiserRaise && deathOffHealers.Count > 0
            ? deathOffHealers[0]
            : deathOthers.Count > 0 ? deathOthers[0] : null;
    }

    private static IBattleChara? GetDispelTarget()
    {
        if (Player.Job is Job.WHM or Job.SCH or Job.AST or Job.SGE or
            Job.BRD)
        {
            List<IBattleChara> weakenPeople = [];
            List<IBattleChara> dyingPeople = [];

            AddDispelTargets(DataCenter.PartyMembers, weakenPeople);

            foreach (IBattleChara person in weakenPeople)
            {
                bool hasDangerous = false;
                if (person.StatusList != null)
                {
                    for (int i = 0; i < person.StatusList.Length; i++)
                    {
                        Dalamud.Game.ClientState.Statuses.Status? status = person.StatusList[i];
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
        if (members == null)
        {
            return;
        }

        foreach (IBattleChara member in members)
        {
            try
            {
                if (member.StatusList != null)
                {
                    for (int i = 0; i < member.StatusList.Length; i++)
                    {
                        Dalamud.Game.ClientState.Statuses.Status? status = member.StatusList[i];
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

        foreach (IBattleChara target in targets)
        {
            float distance = ObjectHelper.DistanceToPlayer(target);
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
        DateTime now = DateTime.Now;
        if (now - _lastUpdateTimeToKill < TimeToKillUpdateInterval)
        {
            return;
        }

        _lastUpdateTimeToKill = now;

        if (DataCenter.RecordedHP.Count >= DataCenter.HP_RECORD_TIME)
        {
            _ = DataCenter.RecordedHP.Dequeue();
        }

        SortedList<ulong, float> currentHPs = [];
        foreach (IBattleChara target in DataCenter.AllTargets)
        {
            if (target.CurrentHp != 0)
            {
                currentHPs[target.GameObjectId] = target.GetHealthRatio();
            }
        }

        DataCenter.RecordedHP.Enqueue((now, currentHPs));
    }
}