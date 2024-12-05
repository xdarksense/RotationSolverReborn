using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

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
        DataCenter.AllTargets = Svc.Objects.OfType<IBattleChara>().GetObjectInRadius(30)
            .Where(o => !o.IsDummy() || !Service.Config.DisableTargetDummys).ToList();
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

    private static unsafe List<IBattleChara> GetPartyMembers()
    {
        return DataCenter.AllianceMembers.Where(ObjectHelper.IsParty)
            .Where(b => b.Character()->CharacterData.OnlineStatus != 15 && b.IsTargetable).ToList();
    }

    private static unsafe List<IBattleChara> GetAllianceMembers()
    {
        return DataCenter.AllTargets.Where(ObjectHelper.IsAlliance)
            .Where(b => b.Character()->CharacterData.OnlineStatus != 15 && b.IsTargetable).ToList();
    }

    private static List<IBattleChara> GetFriendlyNPCs()
    {
        // Check if the configuration setting is true
        if (!Service.Config.FriendlyBattleNpcHeal && !Service.Config.FriendlyPartyNpcHealRaise2)
        {
            return [];
        }

        try
        {
            // Ensure Svc.Objects is not null
            if (Svc.Objects == null)
            {
                return [];
            }

            // Filter and cast objects safely
            var friendlyNpcs = Svc.Objects
                .Where(obj => obj != null && obj.ObjectKind == ObjectKind.BattleNpc)
                .Where(obj =>
                {
                    try
                    {
                        return obj.GetNameplateKind() == NameplateKind.FriendlyBattleNPC ||
                               obj.GetBattleNPCSubKind() == BattleNpcSubKind.NpcPartyMember;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception for debugging purposes
                        Svc.Log.Error($"Error filtering object in get_FriendlyNPCMembers: {ex.Message}");
                        return false;
                    }
                })
                .OfType<IBattleChara>()
                .ToList();

            return friendlyNpcs;
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
            Svc.Log.Error($"Error in get_FriendlyNPCMembers: {ex.Message}");
            return [];
        }
    }

    private static List<IBattleChara> GetAllHostileTargets()
    {
        var strongOfShieldPositional = EnemyPositional.Front;

        return DataCenter.AllTargets.Where(b =>
        {
            // Check if the target is an enemy and targetable.
            if (!b.IsEnemy() || !b.IsTargetable) return false;

            // Check if the target is invincible.
            if (b.StatusList.Any(StatusHelper.IsInvincible)) return false;

            // Special exception for the Strong of Shield status on Hansel and Gretel.
            if (b.HasStatus(true, StatusID.StrongOfShield) && strongOfShieldPositional != b.FindEnemyPositional()) return false;

            // If all checks pass, the target is considered hostile.
            return true;
        }).ToList();
    }

    private static IBattleChara? GetDeathTarget()
    {
        // Added so it only tracks deathtarget if you are on a raise job
        var rotation = DataCenter.RightNowRotation;
        if (Player.Job == Job.WHM || Player.Job == Job.SCH || Player.Job == Job.AST || Player.Job == Job.SGE ||
            Player.Job == Job.SMN || Player.Job == Job.RDM)
        {
            var deathAll = DataCenter.AllianceMembers.GetDeath();

            var deathParty = DataCenter.PartyMembers.GetDeath();
            var deathNPC = DataCenter.FriendlyNPCMembers.GetDeath();

            // Check death in party members
            if (deathParty.Any())
            {
                var deathT = deathParty.GetJobCategory(JobRole.Tank).ToList();
                var deathH = deathParty.GetJobCategory(JobRole.Healer).ToList();

                if (deathT.Count > 1) return deathT.FirstOrDefault();
                if (deathH.Any()) return deathH.FirstOrDefault();
                if (deathT.Any()) return deathT.FirstOrDefault();

                return deathParty.FirstOrDefault();
            }

            // Check death in alliance members
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

            // Check death in friendly NPC members
            if (deathNPC.Any() && Service.Config.FriendlyPartyNpcHealRaise2)
            {
                var deathNPCT = deathNPC.GetJobCategory(JobRole.Tank).ToList();
                var deathNPCH = deathNPC.GetJobCategory(JobRole.Healer).ToList();

                if (deathNPCT.Count > 1) return deathNPCT.FirstOrDefault();
                if (deathNPCH.Any()) return deathNPCH.FirstOrDefault();
                if (deathNPCT.Any()) return deathNPCT.FirstOrDefault();

                return deathNPC.FirstOrDefault();
            }

            return null;
        }

        return null;
    }

    private static IBattleChara? GetDispelTarget()
    {
        var weakenPeople = DataCenter.PartyMembers?
            .Where(o => o is IBattleChara b && b.StatusList != null &&
                        b.StatusList.Any(status => status != null && status.CanDispel())) ?? [];
        var weakenNPC = DataCenter.FriendlyNPCMembers?
            .Where(o => o is IBattleChara b && b.StatusList != null &&
                        b.StatusList.Any(status => status != null && status.CanDispel())) ?? [];
        var dyingPeople = weakenPeople
            .Where(o => o is IBattleChara b && b.StatusList != null &&
                        b.StatusList.Any(status => status != null && status.IsDangerous()));

        return dyingPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                                  ?? weakenPeople.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault()
                                  ?? weakenNPC.OrderBy(ObjectHelper.DistanceToPlayer).FirstOrDefault();
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