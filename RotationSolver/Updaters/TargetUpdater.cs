using Dalamud.Game.ClientState.Objects.Enums;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
    private static readonly ObjectListDelay<IBattleChara>
        _raisePartyTargets = new(() => Service.Config.RaiseDelay),
        _raiseAllTargets = new(() => Service.Config.RaiseDelay);

    private static DateTime _lastUpdateTimeToKill = DateTime.MinValue;
    private static readonly TimeSpan TimeToKillUpdateInterval = TimeSpan.FromSeconds(0.5);

    internal static void UpdateTargets()
    {
        DataCenter.AllTargets = GetAllTargets();
        DataCenter.FriendlyNPCMembers = GetFriendlyNPCs();
        DataCenter.AllianceMembers = GetAllianceMembers();
        DataCenter.PartyMembers = GetPartyMembers();
        DataCenter.DeathTarget = GetDeathTarget();
        DataCenter.DispelTarget = GetDispelTarget();
        DataCenter.AllHostileTargets = GetAllHostileTargets();
        DataCenter.ProvokeTarget = DataCenter.AllHostileTargets.FirstOrDefault(ObjectHelper.CanProvoke);
        DataCenter.InterruptTarget = DataCenter.AllHostileTargets.FirstOrDefault(ObjectHelper.CanInterrupt);
        UpdateTimeToKill();
    }

    private static List<IBattleChara> GetAllTargets()
    {
        var allTargets = new List<IBattleChara>();
        foreach (var obj in Svc.Objects.OfType<IBattleChara>())
        {
            if (!obj.IsDummy() || !Service.Config.DisableTargetDummys)
            {
                allTargets.Add(obj);
            }
        }
        return allTargets;
    }

    private static unsafe List<IBattleChara> GetPartyMembers()
    {
        var partyMembers = new List<IBattleChara>();
        try
        {
            if (DataCenter.AllianceMembers != null)
            {
                foreach (var member in DataCenter.AllianceMembers)
                {
                    if (ObjectHelper.IsParty(member) && member.Character() != null &&
                        member.Character()->CharacterData.OnlineStatus != 15 &&
                        member.Character()->CharacterData.OnlineStatus != 5 && member.IsTargetable)
                    {
                        partyMembers.Add(member);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in GetPartyMembers: {ex.Message}");
        }
        return partyMembers;
    }

    private static unsafe List<IBattleChara> GetAllianceMembers()
    {
        var allianceMembers = new List<IBattleChara>();
        try
        {
            if (DataCenter.AllTargets != null)
            {
                foreach (var target in DataCenter.AllTargets)
                {
                    if (ObjectHelper.IsAlliance(target) && target.Character() != null &&
                        target.Character()->CharacterData.OnlineStatus != 15 &&
                        target.Character()->CharacterData.OnlineStatus != 5 && target.IsTargetable)
                    {
                        allianceMembers.Add(target);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in GetAllianceMembers: {ex.Message}");
        }
        return allianceMembers;
    }

    private static List<IBattleChara> GetFriendlyNPCs()
    {
        var friendlyNpcs = new List<IBattleChara>();
        if (!Service.Config.FriendlyBattleNpcHeal && !Service.Config.FriendlyPartyNpcHealRaise2)
        {
            return friendlyNpcs;
        }

        try
        {
            if (Svc.Objects != null)
            {
                foreach (var obj in Svc.Objects)
                {
                    if (obj != null && obj.ObjectKind == ObjectKind.BattleNpc)
                    {
                        try
                        {
                            if (obj.GetNameplateKind() == NameplateKind.FriendlyBattleNPC ||
                                obj.GetBattleNPCSubKind() == BattleNpcSubKind.NpcPartyMember)
                            {
                                if (obj is IBattleChara battleChara)
                                {
                                    friendlyNpcs.Add(battleChara);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Svc.Log.Error($"Error filtering object in GetFriendlyNPCs: {ex.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in GetFriendlyNPCs: {ex.Message}");
        }
        return friendlyNpcs;
    }

    private static List<IBattleChara> GetAllHostileTargets()
    {
        var hostileTargets = new List<IBattleChara>();
        var strongOfShieldPositional = EnemyPositional.Front;

        try
        {
            foreach (var target in DataCenter.AllTargets)
            {
                if (target == null) continue;
                if (!target.IsEnemy() || !target.IsTargetable) continue;
                if (target.StatusList?.Any(StatusHelper.IsInvincible) == true && (DataCenter.IsPvP && !Service.Config.IgnorePvPInvincibility || !DataCenter.IsPvP)) continue;
                if (target.HasStatus(true, StatusID.StrongOfShield) && strongOfShieldPositional != target.FindEnemyPositional()) continue;

                hostileTargets.Add(target);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in GetAllHostileTargets: {ex.Message}");
        }
        return hostileTargets;
    }

    private static IBattleChara? GetDeathTarget()
    {
        var rotation = DataCenter.RightNowRotation;
        if (Player.Job == Job.WHM || Player.Job == Job.SCH || Player.Job == Job.AST || Player.Job == Job.SGE ||
            Player.Job == Job.SMN || Player.Job == Job.RDM)
        {
            try
            {
                var deathAll = DataCenter.AllianceMembers?.GetDeath() ?? new List<IBattleChara>();
                var deathParty = DataCenter.PartyMembers?.GetDeath() ?? new List<IBattleChara>();
                var deathNPC = DataCenter.FriendlyNPCMembers?.GetDeath() ?? new List<IBattleChara>();

                if (deathParty.Any())
                {
                    var deathT = deathParty.GetJobCategory(JobRole.Tank).ToList();
                    var deathH = deathParty.GetJobCategory(JobRole.Healer).ToList();

                    if (deathT.Count > 1) return deathT.FirstOrDefault();
                    if (deathH.Any()) return deathH.FirstOrDefault();
                    if (deathT.Any()) return deathT.FirstOrDefault();

                    return deathParty.FirstOrDefault();
                }

                if (deathAll.Any())
                {
                    if (Service.Config.RaiseType == RaiseType.PartyAndAllianceHealers)
                    {
                        var deathAllH = deathAll.GetJobCategory(JobRole.Healer).ToList();
                        if (deathAllH.Any()) return deathAllH.FirstOrDefault();
                    }

                    if (Service.Config.RaiseType == RaiseType.PartyAndAlliance)
                    {
                        var deathAllH = deathAll.GetJobCategory(JobRole.Healer).ToList();
                        var deathAllT = deathAll.GetJobCategory(JobRole.Tank).ToList();

                        if (deathAllH.Any()) return deathAllH.FirstOrDefault();
                        if (deathAllT.Any()) return deathAllT.FirstOrDefault();

                        return deathAll.FirstOrDefault();
                    }
                }

                if (deathNPC.Any() && Service.Config.FriendlyPartyNpcHealRaise2)
                {
                    var deathNPCT = deathNPC.GetJobCategory(JobRole.Tank).ToList();
                    var deathNPCH = deathNPC.GetJobCategory(JobRole.Healer).ToList();

                    if (deathNPCT.Count > 1) return deathNPCT.FirstOrDefault();
                    if (deathNPCH.Any()) return deathNPCH.FirstOrDefault();
                    if (deathNPCT.Any()) return deathNPCT.FirstOrDefault();

                    return deathNPC.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Error in GetDeathTarget: {ex.Message}");
            }

            return null;
        }

        return null;
    }

    private static IBattleChara? GetDispelTarget()
    {
        var rotation = DataCenter.RightNowRotation;
        if (Player.Job == Job.WHM || Player.Job == Job.SCH || Player.Job == Job.AST || Player.Job == Job.SGE ||
            Player.Job == Job.BRD)
        {
            var weakenPeople = new List<IBattleChara>();
            var weakenNPC = new List<IBattleChara>();
            var dyingPeople = new List<IBattleChara>();

            if (DataCenter.PartyMembers != null)
            {
                foreach (var member in DataCenter.PartyMembers)
                {
                    if (member is IBattleChara b && b.StatusList != null &&
                        b.StatusList.Any(status => status != null && status.CanDispel()))
                    {
                        weakenPeople.Add(b);
                    }
                }
            }

            if (DataCenter.FriendlyNPCMembers != null)
            {
                foreach (var npc in DataCenter.FriendlyNPCMembers)
                {
                    if (npc is IBattleChara b && b.StatusList != null &&
                        b.StatusList.Any(status => status != null && status.CanDispel()))
                    {
                        weakenNPC.Add(b);
                    }
                }
            }

            foreach (var person in weakenPeople)
            {
                if (person is IBattleChara b && b.StatusList != null &&
                    b.StatusList.Any(status => status != null && status.IsDangerous()))
                {
                    dyingPeople.Add(b);
                }
            }

            return dyingPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                   ?? weakenPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                   ?? weakenNPC.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault();
        }
        else
        {
            return null;
        }
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
