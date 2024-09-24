using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Get the information from object.
/// </summary>
public static class ObjectHelper
{
    static readonly EventHandlerType[] _eventType =
    {
        EventHandlerType.TreasureHuntDirector,
        EventHandlerType.Quest,
    };

    internal static Lumina.Excel.GeneratedSheets.BNpcBase? GetObjectNPC(this IGameObject obj)
    {
        return obj == null ? null : Service.GetSheet<Lumina.Excel.GeneratedSheets.BNpcBase>().GetRow(obj.DataId);
    }
    
    internal static bool CanProvoke(this IGameObject target)
    {
        if (target == null) return false;

        // Assuming DataCenter.FateId is an ID, not an IGameObject
        if (target.DataId == DataCenter.FateId) return false;

        //Removed the listed names.
        if (OtherConfiguration.NoProvokeNames.TryGetValue(Svc.ClientState.TerritoryType, out var ns1))
        {
            var names = ns1.Where(n => !string.IsNullOrEmpty(n) && new Regex(n).Match(target.Name.ToString()).Success);
            if (names.Any()) return false;
        }

        //Target can move or too big and has a target
        if ((target.GetObjectNPC()?.Unknown12 == 0 || target.HitboxRadius >= 5)
            && (target.TargetObject?.IsValid() ?? false))
        {
            //the target is not a tank role
            if (Svc.Objects.SearchById(target.TargetObjectId) is IBattleChara battle
                && !battle.IsJobCategory(JobRole.Tank)
                && (Vector3.Distance(target.Position, Player.Object.Position) > 5))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool HasPositional(this IGameObject obj)
    {
        return obj != null && !(obj.GetObjectNPC()?.Unknown10 ?? false);
    }

    internal static unsafe bool IsOthersPlayers(this IGameObject obj)
    {
        //SpecialType but no NamePlateIcon
        return _eventType.Contains(obj.GetEventType()) && obj.GetNamePlateIcon() == 0;
    }

    internal static bool IsAttackable(this IBattleChara battleChara)
    {
        // Dead.
        if (Service.Config.FilterOneHpInvincible && battleChara.CurrentHp <= 1) return false;

        if (battleChara.StatusList.Any(StatusHelper.IsInvincible)) return false;

        if (Svc.ClientState == null) return false;

        // In No Hostiles Names
        if (OtherConfiguration.NoHostileNames != null &&
            OtherConfiguration.NoHostileNames.TryGetValue(Svc.ClientState.TerritoryType, out var ns1))
        {
            var names = ns1.Where(n => !string.IsNullOrEmpty(n) && new Regex(n).Match(battleChara.Name.TextValue).Success);
            if (names.Any()) return false;
        }

        // Fate
        if (DataCenter.TerritoryContentType != TerritoryContentType.Eureka)
        {
            var tarFateId = battleChara.FateId();
            if (tarFateId != 0 && tarFateId != DataCenter.FateId) return false;
        }

        if (Service.Config.AddEnemyListToHostile)
        {
            if (battleChara.IsInEnemiesList()) return true;
            // Only attack
            if (Service.Config.OnlyAttackInEnemyList) return false;
        }

        // Tar on me
        if (battleChara.TargetObject == Player.Object
            || battleChara.TargetObject?.OwnerId == Player.Object.GameObjectId) return true;

        if (battleChara.IsOthersPlayers()) return false;

        if (battleChara.IsTopPriorityHostile()) return true;

        if (Service.CountDownTime > 0 || DataCenter.IsPvP) return true;

        return DataCenter.RightNowTargetToHostileType switch
        {
            TargetHostileType.AllTargetsCanAttack => true,
            TargetHostileType.TargetsHaveTarget => battleChara.TargetObject is IBattleChara,
            TargetHostileType.AllTargetsWhenSolo => DataCenter.PartyMembers.Length < 2 || battleChara.TargetObject is IBattleChara,
            TargetHostileType.AllTargetsWhenSoloInDuty => (DataCenter.PartyMembers.Length < 2 && Svc.Condition[ConditionFlag.BoundByDuty])
                || battleChara.TargetObject is IBattleChara,
            _ => true,
        };
    }

    internal static unsafe bool IsInEnemiesList(this IBattleChara IBattleChara)
    {
        var addons = Service.GetAddons<AddonEnemyList>();

        if (!addons.Any()) return false;
        var addon = addons.FirstOrDefault();
        var enemy = (AddonEnemyList*)addon;

        var numArray = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUIModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.NumberArrays[19];
        if (numArray == null) return false;

        const int baseIndex = 8;
        const int step = 6;

        for (var i = 0; i < enemy->EnemyCount; i++)
        {
            var id = (uint)numArray->IntArray[baseIndex + i * step];

            if (IBattleChara.GameObjectId == id) return true;
        }
        return false;
    }

    internal static unsafe bool IsEnemy(this IGameObject obj)
    => obj != null
    && ActionManager.CanUseActionOnTarget((uint)ActionID.BlizzardPvE, obj.Struct());

    internal static unsafe bool IsAlliance(this IGameObject obj)
        => obj.GameObjectId is not 0
        && (!(DataCenter.IsPvP) && obj is IPlayerCharacter
        || ActionManager.CanUseActionOnTarget((uint)ActionID.CurePvE, obj.Struct()));

    private static readonly object _lock = new object();

    internal static bool IsParty(this IGameObject gameObject)
    {
        if (gameObject == null) return false;

        // Use a lock to ensure thread safety
        lock (_lock)
        {
            // Accessing Player.Object and Svc.Party within the lock to ensure thread safety
            if (gameObject.GameObjectId == Player.Object?.GameObjectId) return true;
            if (Svc.Party.Any(p => p.GameObject?.GameObjectId == gameObject.GameObjectId)) return true;
            if (Service.Config.FriendlyPartyNpcHealRaise && gameObject.GetBattleNPCSubKind() == BattleNpcSubKind.NpcPartyMember) return true;

            // Check if ChocoboPartyMember is enabled
            if (Service.Config.ChocoboPartyMember)
            {
                // Add Player Chocobo Companion to Party List
                unsafe
                {
                    try
                    {
                        BattleChara* companionChocobo = DataCenter.GetCompanion();
                        if (companionChocobo != null)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Error(ex, "Error accessing companionChocobo");
                    }
                }
            }

            if (Service.Config.FriendlyBattleNpcHeal && gameObject.GetNameplateKind() == NameplateKind.FriendlyBattleNPC) return true;

        }
        return false;
    }

    internal static bool IsTargetOnSelf(this IBattleChara IBattleChara)
    {
        return IBattleChara.TargetObject?.TargetObject == IBattleChara;
    }

    internal static bool IsDeathToRaise(this IGameObject obj)
    {
        if (obj == null || !obj.IsDead || !obj.IsTargetable) return false;
        if (obj is IBattleChara b && b.CurrentHp != 0) return false;
        if (obj.HasStatus(false, StatusID.Raise)) return false;
        if (!Service.Config.RaiseBrinkOfDeath && obj.HasStatus(false, StatusID.BrinkOfDeath)) return false;
        if (DataCenter.AllianceMembers.Any(c => c.CastTargetObjectId == obj.GameObjectId)) return false;

        return true;
    }

    internal static bool IsAlive(this IGameObject obj)
    {
        return obj is not IBattleChara b || b.CurrentHp > 1 && obj.IsTargetable;
    }

    /// <summary>
    /// Get the object kind.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe ObjectKind GetObjectKind(this IGameObject obj) => (ObjectKind)obj.Struct()->ObjectKind;

    /// <summary>
    /// Determines whether the specified game object is a top priority hostile target.
    /// </summary>
    /// <param name="obj">The game object to check.</param>
    /// <returns>
    /// <c>true</c> if the game object is a top priority hostile target; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsTopPriorityHostile(this IGameObject obj)
    {
        if (obj == null) return false;

        var fateId = DataCenter.FateId;

        // Fetch prioritized target names
        if (OtherConfiguration.PrioTargetNames.TryGetValue(Svc.ClientState.TerritoryType, out var prioTargetNames))
        {
            // If the target's name matches any prioritized names, it is attackable
            if (obj is IBattleChara bpnc && prioTargetNames.Any(n => !string.IsNullOrEmpty(n) && new Regex(n).Match(bpnc.Name.TextValue).Success))
            {
                return true;
            }
        }

        if (obj is IBattleChara b && b.StatusList?.Any(StatusHelper.IsPriority) == true) return true;

        if (Service.Config.ChooseAttackMark && MarkingHelper.AttackSignTargets.FirstOrDefault(id => id != 0) == (long)obj.GameObjectId) return true;

        // Fate
        if (Service.Config.TargetFatePriority && fateId != 0 && obj.FateId() == fateId) return true;

        var icon = obj.GetNamePlateIcon();

        // Hunting log and weapon
        if (Service.Config.TargetHuntingRelicLevePriority && icon is 60092 or 60096 or 71244) return true;
        //60092 Hunt Target
        //60096 Relic Weapon
        //71244 Leve Target

        // Quest
        if (Service.Config.TargetQuestPriority && (icon is 71204 or 71144 or 71224 or 71344 || obj.GetEventType() is EventHandlerType.Quest)) return true;
        //71204 Main Quest
        //71144 Major Quest
        //71224 Other Quest
        //71344 Major Quest

        if (obj is IBattleChara npc && DataCenter.PrioritizedNameIds.Contains(npc.NameId)) return true;

        // Check if the object is a BattleNpcPart
        if (Service.Config.PrioEnemyParts && obj.GetBattleNPCSubKind() == BattleNpcSubKind.BattleNpcPart) return true;

        return false;
    }

    internal static unsafe uint GetNamePlateIcon(this IGameObject obj) => obj.Struct()->NamePlateIconId;

    internal static unsafe EventHandlerType GetEventType(this IGameObject obj) => obj.Struct()->EventId.ContentId;

    internal static unsafe BattleNpcSubKind GetBattleNPCSubKind(this IGameObject obj) => (BattleNpcSubKind)obj.Struct()->SubKind;

    internal static unsafe uint FateId(this IGameObject obj) => obj.Struct()->FateId;

    static readonly Dictionary<uint, bool> _effectRangeCheck = new();

    /// <summary>
    /// Determines whether the specified game object can be interrupted.
    /// </summary>
    /// <param name="o">The game object to check.</param>
    /// <returns>
    /// <c>true</c> if the game object can be interrupted; otherwise, <c>false</c>.
    /// </returns>
    internal static bool CanInterrupt(this IGameObject o)
    {
        if (o is not IBattleChara b) return false;

        var baseCheck = b.IsCasting && b.IsCastInterruptible && b.TotalCastTime >= 2;
        if (!baseCheck) return false;
        if (!Service.Config.InterruptibleMoreCheck) return false;

        var id = b.CastActionId;
        if (_effectRangeCheck.TryGetValue(id, out var check)) return check;

        var act = Service.GetSheet<Lumina.Excel.GeneratedSheets.Action>().GetRow(b.CastActionId);
        if (act == null) return _effectRangeCheck[id] = false;
        if (act.CastType is 3 or 4 || (act.EffectRange is > 0 and < 8)) return _effectRangeCheck[id] = false;

        return _effectRangeCheck[id] = true;
    }

    internal static bool IsDummy(this IBattleChara obj) => obj?.NameId == 541;

    /// <summary>
    /// Is target a boss depends on the ttk.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsBossFromTTK(this IBattleChara obj)
    {
        if (obj == null) return false;

        if (obj.IsDummy()) return true;

        //Fate
        return obj.GetTimeToKill(true) >= Service.Config.BossTimeToKill;
    }

    /// <summary>
    /// Is target a boss depends on the icon.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsBossFromIcon(this IBattleChara obj)
    {
        if (obj == null) return false;

        if (obj.IsDummy()) return true;

        //Icon
        var npc = obj.GetObjectNPC();
        return npc?.Rank is 1 or 2 or 6;
    }

    /// <summary>
    /// Is object dying.
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsDying(this IBattleChara b)
    {
        if (b == null || b.IsDummy()) return false;
        return b.GetTimeToKill() <= Service.Config.DyingTimeToKill || b.GetHealthRatio() < Service.Config.IsDyingConfig;
    }

    /// <summary>
    /// Determines whether the specified battle character is currently in combat.
    /// </summary>
    /// <param name="obj">The battle character to check.</param>
    /// <returns>
    /// <c>true</c> if the battle character is in combat; otherwise, <c>false</c>.
    /// </returns>
    internal static unsafe bool InCombat(this IBattleChara obj)
    {
        if (obj == null || obj.Struct() == null) return false;
        return obj.Struct()->Character.InCombat;
    }

    private static readonly TimeSpan CheckSpan = TimeSpan.FromSeconds(2.5);

    /// <summary>
    /// Calculates the estimated time to kill the specified battle character.
    /// </summary>
    /// <param name="b">The battle character to calculate the time to kill for.</param>
    /// <param name="wholeTime">If set to <c>true</c>, calculates the total time to kill; otherwise, calculates the remaining time to kill.</param>
    /// <returns>
    /// The estimated time to kill the battle character in seconds, or <see cref="float.NaN"/> if the calculation cannot be performed.
    /// </returns>
    internal static float GetTimeToKill(this IBattleChara b, bool wholeTime = false)
    {
        if (b == null) return float.NaN;
        if (b.IsDummy()) return 999.99f;

        var objectId = b.GameObjectId;

        DateTime startTime = DateTime.MinValue;
        float thatTimeRatio = 0;

        // Create a copy of the RecordedHP collection to avoid modification during enumeration
        var recordedHPCopy = DataCenter.RecordedHP.ToList();

        foreach (var (time, hpRatios) in recordedHPCopy)
        {
            if (hpRatios.TryGetValue(objectId, out var ratio) && ratio != 1)
            {
                startTime = time;
                thatTimeRatio = ratio;
                break;
            }
        }

        var timespan = DateTime.UtcNow - startTime;
        if (startTime == DateTime.MinValue || timespan < CheckSpan) return float.NaN;

        var ratioNow = b.GetHealthRatio();
        if (float.IsNaN(ratioNow)) return float.NaN;

        var ratioReduce = thatTimeRatio - ratioNow;
        if (ratioReduce <= 0) return float.NaN;

        return (float)timespan.TotalSeconds / ratioReduce * (wholeTime ? 1 : ratioNow);
    }

    /// <summary>
    /// Determines if the specified battle character has been attacked within the last second.
    /// </summary>
    /// <param name="b">The battle character to check.</param>
    /// <returns>
    /// <c>true</c> if the battle character has been attacked within the last second; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsAttacked(this IBattleChara b)
    {
        if (b == null) return false;
        var now = DateTime.Now;
        foreach (var (id, time) in DataCenter.AttackedTargets)
        {
            if (id == b.GameObjectId)
            {
                return now - time <= TimeSpan.FromSeconds(1);
            }
        }
        return false;
    }

    /// <summary>
    /// Determines if the player can see the specified game object.
    /// </summary>
    /// <param name="b">The game object to check visibility for.</param>
    /// <returns>
    /// <c>true</c> if the player can see the specified game object; otherwise, <c>false</c>.
    /// </returns>
    internal static unsafe bool CanSee(this IGameObject b)
    {
        var player = Player.Object;
        if (player == null || b == null) return false;

        const uint specificEnemyId = 3830; // Bioculture Node in Aetherial Chemical Research Facility
        if (b.GameObjectId == specificEnemyId)
        {
            return true;
        }

        var point = player.Position + Vector3.UnitY * Player.GameObject->Height;
        var tarPt = b.Position + Vector3.UnitY * b.Struct()->Height;
        var direction = tarPt - point;

        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit;
        var ray = new Ray(point, direction);

        return !FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastMaterialFilter(&hit, &point, &direction, direction.Length(), 1, unknown);
    }

    /// <summary>
    /// Get the <paramref name="g"/>'s current HP percentage.
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public static float GetHealthRatio(this IGameObject g)
    {
        if (g is not IBattleChara b) return 0;
        if (DataCenter.RefinedHP.TryGetValue(b.GameObjectId, out var hp)) return hp;
        return (float)b.CurrentHp / b.MaxHp;
    }

    /// <summary>
    /// Determines the positional relationship of the player relative to the enemy.
    /// </summary>
    /// <param name="enemy">The enemy game object.</param>
    /// <returns>
    /// An <see cref="EnemyPositional"/> value indicating whether the player is in front, at the rear, or on the flank of the enemy.
    /// </returns>
    public static EnemyPositional FindEnemyPositional(this IGameObject enemy)
    {
        if (enemy == null || Player.Object == null) return EnemyPositional.None;

        Vector3 pPosition = enemy.Position;
        Vector2 faceVec = enemy.GetFaceVector();

        Vector3 dir = Player.Object.Position - pPosition;
        Vector2 dirVec = new Vector2(dir.Z, dir.X);

        double angle = faceVec.AngleTo(dirVec);

        const double frontAngle = Math.PI / 4;
        const double rearAngle = Math.PI * 3 / 4;

        if (angle < frontAngle) return EnemyPositional.Front;
        else if (angle > rearAngle) return EnemyPositional.Rear;
        return EnemyPositional.Flank;
    }

    /// <summary>
    /// Gets the facing direction vector of the game object.
    /// </summary>
    /// <param name="obj">The game object.</param>
    /// <returns>
    /// A <see cref="Vector2"/> representing the facing direction of the game object.
    /// </returns>
    internal static Vector2 GetFaceVector(this IGameObject obj)
    {
        if (obj == null) return Vector2.Zero;

        float rotation = obj.Rotation;
        return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
    }

    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="vec1">The first vector.</param>
    /// <param name="vec2">The second vector.</param>
    /// <returns>
    /// The angle in radians between the two vectors.
    /// </returns>
    internal static double AngleTo(this Vector2 vec1, Vector2 vec2)
    {
        double lengthProduct = vec1.Length() * vec2.Length();
        if (lengthProduct == 0) return 0;

        double dotProduct = Vector2.Dot(vec1, vec2);
        return Math.Acos(dotProduct / lengthProduct);
    }

    /// <summary>
    /// The distance from <paramref name="obj"/> to the player
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float DistanceToPlayer(this IGameObject? obj)
    {
        if (obj == null || Player.Object == null) return float.MaxValue;

        var player = Player.Object;
        var distance = Vector3.Distance(player.Position, obj.Position) - (player.HitboxRadius + obj.HitboxRadius);
        return distance;
    }

}
