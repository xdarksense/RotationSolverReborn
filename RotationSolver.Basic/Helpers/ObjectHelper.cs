using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using ExCSS;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Get the information from object.
/// </summary>
public static class ObjectHelper
{
    private static readonly EventHandlerContent[] _eventType =
    {
        EventHandlerContent.TreasureHuntDirector,
        EventHandlerContent.Quest,
    };

    internal static Lumina.Excel.Sheets.BNpcBase? GetObjectNPC(this IGameObject obj)
    {
        return obj == null ? null : Service.GetSheet<Lumina.Excel.Sheets.BNpcBase>().GetRow(obj.DataId);
    }

    internal static bool CanProvoke(this IGameObject target)
    {
        if (target == null)
        {
            return false;
        }

        if (DataCenter.PlayerFateId != 0 && target.FateId() == DataCenter.PlayerFateId)
        {
            return false;
        }

        // Removed the listed names.
        if (OtherConfiguration.NoProvokeNames.TryGetValue(Svc.ClientState.TerritoryType, out string[]? ns1))
        {
            foreach (string n in ns1)
            {
                if (!string.IsNullOrEmpty(n) && new Regex(n).Match(target.Name?.GetText() ?? string.Empty).Success)
                {
                    return false;
                }
            }
        }

        // Target can move or too big and has a target
        if ((target.GetObjectNPC()?.Unknown0 == 0 || target.HitboxRadius >= 5) // Unknown12 used to be the flag checked for the mobs ability to move, honestly just guessing on this one
            && (target.TargetObject?.IsValid() ?? false))
        {
            // The target is not a tank role
            if (Svc.Objects.SearchById(target.TargetObjectId) is IBattleChara targetObject && !targetObject.IsJobCategory(JobRole.Tank)
                && (Vector3.Distance(target.Position, Player.Object?.Position ?? Vector3.Zero) > 5))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool HasPositional(this IGameObject obj)
    {
        return obj != null && (!(obj.GetObjectNPC()?.IsOmnidirectional ?? false));
    }

    internal static unsafe bool IsOthersPlayersMob(this IGameObject obj)
    {
        //SpecialType but no NamePlateIcon
        return _eventType.Contains(obj.GetEventType()) && obj.GetNamePlateIcon() == 0;
    }

    internal static bool IsAttackable(this IBattleChara battleChara)
    {
        if (Svc.ClientState == null)
        {
            return false;
        }

        if (battleChara.StatusList == null)
        {
            return false;
        }

        if (battleChara.IsAllianceMember())
        {
            return false;
        }

        if (!battleChara.IsEnemy())
        {
            return false;
        }

        if (battleChara.IsSpecialExecptionImmune())
        {
            return false; // For specific named mobs that are immune to everything.
        }

        if (battleChara.IsSpecialImmune())
        {
            return false;
        }

        // Dead.
        if (Service.Config.FilterOneHpInvincible && battleChara.CurrentHp <= 1)
        {
            return false;
        }

        foreach (Dalamud.Game.ClientState.Statuses.Status status in battleChara.StatusList)
        {
            if (StatusHelper.IsInvincible(status) && ((DataCenter.IsPvP && !Service.Config.IgnorePvPInvincibility) || !DataCenter.IsPvP))
            {
                return false;
            }
        }

        // In No Hostiles Names
        if (OtherConfiguration.NoHostileNames != null &&
            OtherConfiguration.NoHostileNames.TryGetValue(Svc.ClientState.TerritoryType, out string[]? ns1))
        {
            foreach (string n in ns1)
            {
                if (!string.IsNullOrEmpty(n) && new Regex(n).Match(battleChara.Name.TextValue).Success)
                {
                    return false;
                }
            }
        }

        // Fate
        if (Service.Config.IgnoreNonFateInFate && DataCenter.Territory?.ContentType != TerritoryContentType.Eureka)
        {
            if (battleChara.FateId() != 0 && battleChara.FateId() != DataCenter.PlayerFateId)
            {
                return false;
            }
        }

        if (Service.Config.BozjaCEmobtargeting
            && DataCenter.IsInBozjanFieldOp
            && !DataCenter.IsInDelubrumNormal
            && !DataCenter.IsInDelubrumSavage)
        {
            bool isInCE = DataCenter.IsInBozjanFieldOpCE;

            // Prevent targeting mobs in Bozja CE if you are not in CE
            if (battleChara.IsBozjanCEFateMob() && !isInCE)
            {
                return false;
            }

            // Prevent targeting mobs out of Bozja CE if you are in CE
            if (!battleChara.IsBozjanCEFateMob() && isInCE)
            {
                return false;
            }
        }

        if (Service.Config.TargetQuestThings && battleChara.IsOthersPlayersMob())
        {
            return false;
        }

        if (battleChara.IsTopPriorityNamedHostile())
        {
            return true;
        }

        if (battleChara.IsTopPriorityHostile())
        {
            return true;
        }

        if (Service.CountDownTime > 0 || DataCenter.IsPvP)
        {
            return true;
        }

        // Tar on me
        return battleChara.TargetObject == Player.Object
            || battleChara.TargetObject?.OwnerId == Player.Object.GameObjectId || DataCenter.CurrentTargetToHostileType switch
            {
                TargetHostileType.AllTargetsCanAttack => true,
                TargetHostileType.TargetsHaveTarget => battleChara.TargetObject is IBattleChara,
                TargetHostileType.AllTargetsWhenSolo => DataCenter.PartyMembers.Count == 1 || battleChara.TargetObject is IBattleChara,
                TargetHostileType.AllTargetsWhenSoloInDuty => (DataCenter.PartyMembers.Count == 1 && (Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56]))
                                    || battleChara.TargetObject is IBattleChara,
                _ => true,
            };
    }

    internal static bool IsBozjanCEFateMob(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.IsEnemy() == false)
        {
            return false;
        }

        if (!DataCenter.IsInBozjanFieldOp)
        {
            return false;
        }

        // Get the EventId of the mob
        return obj.GetEventType() == EventHandlerContent.PublicContentDirector;
    }

    internal static bool IsSpecialExecptionImmune(this IBattleChara obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.NameId == 9441)
        {
            return true; // Special case for Bottom gate in CLL
        }

        return false;
    }

    private static string RemoveControlCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Use a StringBuilder for efficient string manipulation
        StringBuilder output = new(input.Length);
        foreach (char c in input)
        {
            // Exclude control characters and private use area characters
            if (!char.IsControl(c) && (c < '\uE000' || c > '\uF8FF'))
            {
                _ = output.Append(c);
            }
        }
        return output.ToString();
    }

    internal static unsafe bool IsEnemy(this IGameObject obj)
    {
        return obj != null
        && ActionManager.CanUseActionOnTarget((uint)ActionID.BlizzardPvE, obj.Struct());
    }

    internal static unsafe bool IsAllianceMember(this IGameObject obj)
    {
        return obj.GameObjectId is not 0
            && !DataCenter.IsPvP && DataCenter.IsInAllianceRaid && obj is IPlayerCharacter && (ActionManager.CanUseActionOnTarget((uint)ActionID.RaisePvE, obj.Struct()) || ActionManager.CanUseActionOnTarget((uint)ActionID.CurePvE, obj.Struct()));
    }

    internal static bool IsParty(this IGameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        if (gameObject.GameObjectId == Player.Object.GameObjectId)
        {
            return true;
        }

        foreach (Dalamud.Game.ClientState.Party.IPartyMember p in Svc.Party)
        {
            if (p.GameObject?.GameObjectId == gameObject.GameObjectId)
            {
                return true;
            }
        }

        if (Service.Config.FriendlyPartyNpcHealRaise3 && gameObject.IsNpcPartyMember())
        {
            return true;
        }

        return Service.Config.ChocoboPartyMember && gameObject.IsPlayerCharacterChocobo() || (Service.Config.FriendlyBattleNpcHeal && gameObject.IsFriendlyBattleNPC()) || (Service.Config.FocusTargetIsParty && gameObject.IsFocusTarget() && gameObject.IsAllianceMember());
    }

    internal static bool IsNpcPartyMember(this IGameObject gameObj)
    {
        return gameObj.GetBattleNPCSubKind() == BattleNpcSubKind.NpcPartyMember;
    }

    internal static bool IsFriendlyBattleNPC(this IGameObject gameObj)
    {
        return gameObj.GetNameplateKind() == NameplateKind.FriendlyBattleNPC;
    }

    internal static bool IsPlayerCharacterChocobo(this IGameObject gameObj)
    {
        return gameObj.GetBattleNPCSubKind() == BattleNpcSubKind.Chocobo;
    }

    internal static bool IsFocusTarget(this IGameObject gameObj)
    {
        return Svc.Targets.FocusTarget != null && Svc.Targets.FocusTarget.GameObjectId == gameObj.GameObjectId;
    }

    internal static bool IsTargetOnSelf(this IBattleChara IBattleChara)
    {
        return IBattleChara.TargetObject?.TargetObject == IBattleChara;
    }

    internal static bool IsDeathToRaise(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (!obj.IsDead || !obj.IsTargetable)
        {
            return false;
        }

        if (obj is IBattleChara b && b.CurrentHp != 0)
        {
            return false;
        }

        if (obj.HasStatus(false, StatusID.Raise))
        {
            return false;
        }

        if (!Service.Config.RaiseBrinkOfDeath && obj.HasStatus(false, StatusID.BrinkOfDeath))
        {
            return false;
        }

        foreach (IBattleChara c in DataCenter.PartyMembers)
        {
            if (c.CastTargetObjectId == obj.GameObjectId)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsAlive(this IGameObject obj)
    {
        return obj is not IBattleChara b || (b.CurrentHp > 0 && obj.IsTargetable);
    }

    /// <summary>
    /// Get the object kind.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static unsafe ObjectKind GetObjectKind(this IGameObject obj)
    {
        FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* s = obj.Struct();
        return s == null ? default : (ObjectKind)s->ObjectKind;
    }

    /// <summary>
    /// Determines whether the specified game object is a top priority hostile target based on its name being listed.
    /// </summary>
    /// <param name="obj">The game object to check.</param>
    /// <returns>
    /// <c>true</c> if the game object is a top priority named hostile, target; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsTopPriorityNamedHostile(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is IBattleChara npc)
        {
            foreach (uint id in DataCenter.PrioritizedNameIds)
            {
                if (npc.NameId == id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified game object is a top priority hostile target.
    /// </summary>
    /// <param name="obj">The game object to check.</param>
    /// <returns>
    /// <c>true</c> if the game object is a top priority hostile target; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsTopPriorityHostile(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.IsAllianceMember() || obj.IsParty())
        {
            return false;
        }

        if (obj is IBattleChara b)
        {
            // Check IBattleChara against the priority target list of OIDs
            if (PriorityTargetHelper.IsPriorityTarget(b.DataId))
            {
                return true;
            }

            if (Player.Job == Job.MCH && obj.HasStatus(true, StatusID.Wildfire))
            {
                return true;
            }

            // Ensure StatusList is not null before iterating
            if (b.StatusList != null)
            {
                foreach (Dalamud.Game.ClientState.Statuses.Status status in b.StatusList)
                {
                    if (StatusHelper.IsPriority(status))
                    {
                        return true;
                    }
                }
            }
        }

        if (Service.Config.ChooseAttackMark)
        {
            long[] targets = MarkingHelper.GetAttackSignTargets();
            if (targets != null)
            {
                foreach (long id in targets)
                {
                    if (id != 0 && id == (long)obj.GameObjectId && obj.IsEnemy())
                    {
                        return true;
                    }
                }
            }
        }

        if (Service.Config.TargetFatePriority && DataCenter.PlayerFateId != 0 && obj.FateId() == DataCenter.PlayerFateId)
        {
            return true;
        }

        uint icon = obj.GetNamePlateIcon();

        // Hunting log and weapon
        if (Service.Config.TargetHuntingRelicLevePriority && (icon == 60092 || icon == 60096 || icon == 71244))
        {
            return true;
        }
        //60092 Hunt Target
        //60096 Relic Weapon
        //71244 Leve Target

        // Quest
        if (Service.Config.TargetQuestPriority && (icon == 71204 || icon == 71144 || icon == 71224 || icon == 71344 || obj.GetEventType() == EventHandlerContent.Quest))
        {
            return true;
        }
        //71204 Main Quest
        //71144 Major Quest
        //71224 Other Quest
        //71344 Major Quest

        // Check if the object is a BattleNpcPart
        return Service.Config.PrioEnemyParts && obj.GetBattleNPCSubKind() == BattleNpcSubKind.BattleNpcPart;
    }

    internal static unsafe uint GetNamePlateIcon(this IGameObject obj)
    {
        return obj.Struct()->NamePlateIconId;
    }

    internal static unsafe EventHandlerContent GetEventType(this IGameObject obj)
    {
        return obj.Struct()->EventId.ContentId;
    }

    internal static unsafe BattleNpcSubKind GetBattleNPCSubKind(this IGameObject obj)
    {
        return (BattleNpcSubKind)obj.Struct()->SubKind;
    }

    internal static unsafe uint FateId(this IGameObject obj)
    {
        return obj.Struct()->FateId;
    }

    private static readonly ConcurrentDictionary<uint, bool> _effectRangeCheck = [];

    /// <summary>
    /// Determines whether the specified game object can be interrupted.
    /// </summary>
    /// <param name="obj">The game object to check.</param>
    /// <returns>
    /// <c>true</c> if the game object can be interrupted; otherwise, <c>false</c>.
    /// </returns>
    internal static bool CanInterrupt(this IBattleChara obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not IBattleChara b)
        {
            return false;
        }

        // Ensure the IBattleChara object is valid before accessing its properties
        unsafe
        {
            if (b.Struct() == null)
            {
                return false;
            }
        }

        bool baseCheck = b.IsCasting && b.IsCastInterruptible && b.TotalCastTime >= 2;
        if (!baseCheck)
        {
            return false;
        }

        if (!Service.Config.InterruptibleMoreCheck)
        {
            return false;
        }

        if (_effectRangeCheck.TryGetValue(b.CastActionId, out bool check))
        {
            return check;
        }

        Lumina.Excel.Sheets.Action act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(b.CastActionId);
        return act.RowId == 0
            ? (_effectRangeCheck[b.CastActionId] = false)
            : act.CastType == 3 || act.CastType == 4 || (act.EffectRange > 0 && act.EffectRange < 8)
            ? (_effectRangeCheck[b.CastActionId] = false)
            : (_effectRangeCheck[b.CastActionId] = true);
    }

    internal static bool IsDummy(this IBattleChara obj)
    {
        return obj?.NameId == 541;
    }

    /// <summary>
    /// Checks if the target is immune due to any special boss mechanic (Wolf, Jeuno, COD, CinderDrift, Resistance, Omega, LimitlessBlue, HanselOrGretel).
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the target is immune due to any special mechanic; otherwise, false.</returns>
    public static bool IsSpecialImmune(this IBattleChara obj)
    {
        return obj.IsWolfImmune()
            || obj.IsJeunoBossImmune()
            || obj.IsCODBossImmune()
            || obj.IsCinderDriftImmune()
            || obj.IsResistanceImmune()
            || obj.IsOmegaImmune()
            || obj.IsLimitlessBlue()
            || obj.IsHanselorGretelShielded();
    }

    /// <summary>
    /// Is target Wolf add immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsWolfImmune(this IBattleChara obj)
    {
        // Numeric values used instead of name as Lumina does not provide name yet, and may update to change name
        StatusID WindPack = (StatusID)4389; // Numeric value for "Rsv43891100S74Cfc3B0E74Cfc3B0", unable to hit Wolf of Wind
        StatusID StonePack = (StatusID)4390; // Numeric value for "Rsv43901100S74Cfc3B0E74Cfc3B0", unable to hit Wolf of Wind

        bool WolfOfWind = obj.NameId == 13846;
        bool WolfOfStone = obj.NameId == 13847;

        if (WolfOfWind &&
                Player.Object.HasStatus(false, WindPack))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsWolfImmune: WindPack status found");
            }
            return true;
        }

        if (WolfOfStone &&
            Player.Object.HasStatus(false, StonePack))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsWolfImmune: StonePack status found");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target Jeuno Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsJeunoBossImmune(this IBattleChara obj)
    {
        if (obj.HasStatus(false, StatusID.EpicVillain) &&
                (Player.Object.HasStatus(false, StatusID.VauntedHero) || Player.Object.HasStatus(false, StatusID.FatedHero)))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsJeunoBossImmune: EpicVillain status found");
            }
            return true;
        }

        if (obj.HasStatus(false, StatusID.VauntedVillain) &&
            (Player.Object.HasStatus(false, StatusID.EpicHero) || Player.Object.HasStatus(false, StatusID.FatedHero)))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsJeunoBossImmune: VauntedVillain status found");
            }
            return true;
        }

        if (obj.HasStatus(false, StatusID.FatedVillain) &&
            (Player.Object.HasStatus(false, StatusID.EpicHero) || Player.Object.HasStatus(false, StatusID.VauntedHero)))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsJeunoBossImmune: FatedVillain status found");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target COD Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsCODBossImmune(this IBattleChara obj)
    {
        StatusID StygianStatus = StatusID.UnnamedStatus_4388;
        StatusID CloudOfDarknessStatus = StatusID.VeilOfDarkness;
        StatusID AntiCloudOfDarknessStatus = StatusID.OuterDarkness;
        StatusID AntiStygianStatus = StatusID.InnerDarkness;

        if (obj.HasStatus(false, CloudOfDarknessStatus) &&
                Player.Object.HasStatus(false, AntiCloudOfDarknessStatus))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsCODBossImmune: OuterDarkness status found, CloudOfDarkness immune");
            }
            return true;
        }

        if (obj.HasStatus(false, StygianStatus) &&
            Player.Object.HasStatus(false, AntiStygianStatus))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsCODBossImmune: InnerDarkness status found, Stygian immune");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target COD Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsCinderDriftImmune(this IBattleChara obj)
    {
        StatusID GriefAdd = StatusID.BlindToGrief;
        StatusID RageAdd = StatusID.BlindToRage;
        StatusID AntiGriefAdd = StatusID.PallOfGrief;
        StatusID AntiRageAdd = StatusID.PallOfRage;

        if (obj.HasStatus(false, GriefAdd) &&
                Player.Object.HasStatus(false, AntiGriefAdd))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsCinderDriftImmune: AntiGriefAdd status found, GriefAdd immune");
            }
            return true;
        }

        if (obj.HasStatus(false, RageAdd) &&
            Player.Object.HasStatus(false, AntiRageAdd))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsCinderDriftImmune: AntiRageAdd status found, RageAdd immune");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target COD Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsResistanceImmune(this IBattleChara obj)
    {
        StatusID VoidArkMagicResistance = StatusID.MagicResistance;
        StatusID VoidArkRangedResistance = StatusID.RangedResistance;
        StatusID LeviMagicResistance = StatusID.MantleOfTheWhorl;
        StatusID LeviRangedResistance = StatusID.VeilOfTheWhorl;
        JobRole role = Player.Object?.ClassJob.Value.GetJobRole() ?? JobRole.None;

        if (obj.HasStatus(false, VoidArkMagicResistance, LeviMagicResistance) &&
                (role == JobRole.RangedMagical || role == JobRole.Healer))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsResistanceImmune: MagicResistance status found");
            }
            return true;
        }

        if (obj.HasStatus(false, VoidArkRangedResistance, LeviRangedResistance) &&
            role == JobRole.RangedPhysical)
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsResistanceImmune: RangedResistance status found");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target COD Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsOmegaImmune(this IBattleChara obj)
    {
        StatusID AntiOmegaF = StatusID.PacketFilterF;
        StatusID AntiOmegaF_Extreme = StatusID.PacketFilterF_3500;
        StatusID AntiOmegaM = StatusID.PacketFilterM;
        StatusID AntiOmegaM_Extreme = StatusID.PacketFilterM_3499;

        StatusID OmegaF = StatusID.OmegaF;
        StatusID OmegaM = StatusID.OmegaM;
        StatusID OmegaM2 = StatusID.OmegaM_3454;

        if (obj.HasStatus(false, OmegaF) &&
                Player.Object.HasStatus(false, AntiOmegaF, AntiOmegaF_Extreme))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsOmegaImmune: PacketFilterF status found");
            }
            return true;
        }

        if (obj.HasStatus(false, OmegaM, OmegaM2) &&
            Player.Object.HasStatus(false, AntiOmegaM, AntiOmegaM_Extreme))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsOmegaImmune: PacketFilterM status found");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target COD Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsLimitlessBlue(this IBattleChara obj)
    {
        StatusID WillOfTheWater = StatusID.WillOfTheWater;
        StatusID WillOfTheWind = StatusID.WillOfTheWind;
        StatusID WhaleBack = StatusID.Whaleback;

        bool Green = obj.NameId == 3654;
        bool Blue = obj.NameId == 3655;
        bool BismarkShell = obj.NameId == 3656;
        bool BismarkCorona = obj.NameId == 3657;

        if ((BismarkShell || BismarkCorona) &&
                !Player.Object.HasStatus(false, WhaleBack))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsLimitlessBlue: Bismark found, WhaleBack status not found");
            }
            return true;
        }

        if (Blue &&
            Player.Object.HasStatus(false, WillOfTheWater))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsLimitlessBlue: WillOfTheWater status found");
            }
            return true;
        }

        if (Green &&
            Player.Object.HasStatus(false, WillOfTheWind))
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsLimitlessBlue: WillOfTheWind status found");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target Jeuno Boss immune.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsHanselorGretelShielded(this IBattleChara obj)
    {
        EnemyPositional strongOfShieldPositional = EnemyPositional.Front;
        StatusID strongOfShieldStatus = StatusID.StrongOfShield;

        if (obj.HasStatus(false, strongOfShieldStatus) &&
                strongOfShieldPositional != obj.FindEnemyPositional())
        {
            if (Service.Config.InDebug)
            {
                PluginLog.Information("IsHanselorGretelSheilded: StrongOfShield status found, ignoring status haver if player is out of position");
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is target a boss depends on the ttk.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsBossFromTTK(this IBattleChara obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.IsDummy())
        {
            return true;
        }

        //Fate
        return obj.GetTTK(true) >= Service.Config.BossTimeToKill;
    }

    /// <summary>
    /// Is target a boss depends on the icon.
    /// </summary>
    /// <param name="obj">the object.</param>
    /// <returns></returns>
    public static bool IsBossFromIcon(this IBattleChara obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.IsDummy())
        {
            return true;
        }

        //Icon
        Lumina.Excel.Sheets.BNpcBase? npc = obj.GetObjectNPC();
        return npc?.Rank is 1 or 2 or 6;
    }

    /// <summary>
    /// Returns object's calculated shield value.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static uint GetObjectShield(this IGameObject obj)
    {
        return obj is ICharacter character && character.MaxHp > 0 && character.ShieldPercentage > 0
            ? character.MaxHp * character.ShieldPercentage / 100
            : 0;
    }

    /// <summary>
    /// Is object dying.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsDying(this IBattleChara obj)
    {
        return obj != null && !obj.IsDummy() && (obj.GetTTK() <= Service.Config.DyingTimeToKill || obj.GetHealthRatio() < Service.Config.IsDyingConfig);
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
        return obj != null && obj.Struct() != null && obj.Struct()->Character.InCombat;
    }

    private static readonly Dictionary<ulong, Vector3> LastPositions = [];
    internal static bool IsTargetMoving(this IBattleChara target)
    {
        if (target == null)
        {
            return false;
        }

        ulong id = target.GameObjectId;
        Vector3 currentPos = target.Position;
        if (LastPositions.TryGetValue(id, out Vector3 lastPos))
        {
            // You can adjust the threshold as needed
            bool isMoving = Vector3.Distance(currentPos, lastPos) > 0.01f;
            LastPositions[id] = currentPos;
            return isMoving;
        }
        else
        {
            LastPositions[id] = currentPos;
            return false; // First check, assume not moving
        }
    }

    private static readonly TimeSpan CheckSpan = TimeSpan.FromSeconds(2.5);

    /// <summary>
    /// Calculates the estimated time to kill the specified battle character.
    /// </summary>
    /// <param name="obj">The battle character to calculate the time to kill for.</param>
    /// <param name="wholeTime">If set to <c>true</c>, calculates the total time to kill; otherwise, calculates the remaining time to kill.</param>
    /// <returns>
    /// The estimated time to kill the battle character in seconds, or <see cref="float.NaN"/> if the calculation cannot be performed.
    /// </returns>
    internal static float GetTTK(this IBattleChara obj, bool wholeTime = false)
    {
        if (obj == null)
        {
            return float.NaN;
        }

        if (obj.IsDummy())
        {
            return 999.99f;
        }

        DateTime startTime = DateTime.MinValue;
        float initialHpRatio = 0;

        // Use a snapshot of the RecordedHP collection to avoid modification during enumeration
        (DateTime time, SortedList<ulong, float> hpRatios)[] recordedHPCopy = DataCenter.RecordedHP.ToArray();

        // Calculate a moving average of HP ratios
        const int movingAverageWindow = 5;
        Queue<float> hpRatios = new();

        foreach ((DateTime time, SortedList<ulong, float> hpRatiosDict) in recordedHPCopy)
        {
            if (hpRatiosDict != null && hpRatiosDict.TryGetValue(obj.GameObjectId, out float ratio) && ratio != 1)
            {
                if (startTime == DateTime.MinValue)
                {
                    startTime = time;
                    initialHpRatio = ratio;
                }

                hpRatios.Enqueue(ratio);
                if (hpRatios.Count > movingAverageWindow)
                {
                    _ = hpRatios.Dequeue();
                }
            }
        }

        if (startTime == DateTime.MinValue || (DateTime.Now - startTime) < CheckSpan)
        {
            return float.NaN;
        }

        float currentHealthRatio = obj.GetHealthRatio();
        if (float.IsNaN(currentHealthRatio))
        {
            return float.NaN;
        }

        // Manual average calculation to avoid LINQ
        float sum = 0;
        int count = 0;
        foreach (float r in hpRatios)
        {
            sum += r;
            count++;
        }
        float avg = count > 0 ? sum / count : 0;

        float hpRatioDifference = initialHpRatio - avg;
        if (hpRatioDifference <= 0)
        {
            return float.NaN;
        }

        float elapsedTime = (float)(DateTime.Now - startTime).TotalSeconds;
        return elapsedTime / hpRatioDifference * (wholeTime ? 1 : currentHealthRatio);
    }

    private static readonly ConcurrentDictionary<ulong, DateTime> _aliveStartTimes = [];

    /// <summary>
    /// Gets how long the character has been alive in seconds since their last death.
    /// </summary>
    /// <param name="obj">The battle character to check.</param>
    /// <returns>
    /// The time in seconds since the character's last death or first appearance, or float.NaN if unable to determine.
    /// </returns>
    internal static float TimeAlive(this IBattleChara obj)
    {
        if (obj == null)
        {
            return float.NaN;
        }

        // If the character is dead, reset their alive time
        if (obj.IsDead || Svc.Condition[ConditionFlag.BetweenAreas])
        {
            _ = _aliveStartTimes.TryRemove(obj.GameObjectId, out _);
            return 0;
        }

        // If we haven't tracked this character yet, start tracking them
        if (!_aliveStartTimes.ContainsKey(obj.GameObjectId))
        {
            _aliveStartTimes[obj.GameObjectId] = DateTime.Now;
        }

        return (float)(DateTime.Now - _aliveStartTimes[obj.GameObjectId]).TotalSeconds > 30 ? 30 : (float)(DateTime.Now - _aliveStartTimes[obj.GameObjectId]).TotalSeconds;
    }

    private static readonly ConcurrentDictionary<ulong, DateTime> _deadStartTimes = [];

    /// <summary>
    /// Gets how long the character has been dead in seconds since their last death.
    /// </summary>
    /// <param name="obj">The battle character to check.</param>
    /// <returns>
    /// The time in seconds since the character's death, or float.NaN if unable to determine or character is alive.
    /// </returns>
    internal static float TimeDead(this IBattleChara obj)
    {
        if (obj == null)
        {
            return float.NaN;
        }

        // If the character is alive, reset their dead time
        if (!obj.IsDead)
        {
            _ = _deadStartTimes.TryRemove(obj.GameObjectId, out _);
            return 0;
        }

        // If we haven't tracked this character's death yet, start tracking them
        if (!_deadStartTimes.ContainsKey(obj.GameObjectId))
        {
            _deadStartTimes[obj.GameObjectId] = DateTime.Now;
        }

        return (float)(DateTime.Now - _deadStartTimes[obj.GameObjectId]).TotalSeconds > 30 ? 30 : (float)(DateTime.Now - _deadStartTimes[obj.GameObjectId]).TotalSeconds;
    }

    /// <summary>
    /// Determines if the specified battle character has been attacked within the last second.
    /// </summary>
    /// <param name="obj">The battle character to check.</param>
    /// <returns>
    /// <c>true</c> if the battle character has been attacked within the last second; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsAttacked(this IBattleChara obj)
    {
        if (obj == null)
        {
            return false;
        }

        DateTime now = DateTime.Now;
        foreach ((ulong id, DateTime time) in DataCenter.AttackedTargets)
        {
            if (id == obj.GameObjectId)
            {
                return now - time >= TimeSpan.FromSeconds(1);
            }
        }
        return false;
    }

    /// <summary>
    /// Determines if the player can see the specified game object.
    /// </summary>
    /// <param name="obj">The game object to check visibility for.</param>
    /// <returns>
    /// <c>true</c> if the player can see the specified game object; otherwise, <c>false</c>.
    /// </returns>
    internal static unsafe bool CanSee(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.Struct() == null)
        {
            return false;
        }

        const uint specificEnemyId = 3830; // Bioculture Node in Aetherial Chemical Research Facility
        if (obj.GameObjectId == specificEnemyId)
        {
            return true;
        }

        Vector3 point = Player.Object.Position + (Vector3.UnitY * Player.GameObject->Height);
        Vector3 tarPt = obj.Position + (Vector3.UnitY * obj.Struct()->Height);
        Vector3 direction = tarPt - point;

        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit;
        Ray ray = new(point, direction);

        return !FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastMaterialFilter(&hit, &point, &direction, direction.Length(), 1, unknown);
    }

    /// <summary>
    /// Get the <paramref name="obj"/>'s current HP percentage.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float GetHealthRatio(this IGameObject obj)
    {
        if (obj == null)
        {
            return 0;
        }

        if (obj is not IBattleChara b)
        {
            return 0;
        }

        if (DataCenter.RefinedHP.TryGetValue(b.GameObjectId, out float hp))
        {
            return hp;
        }

        if (b.MaxHp == 0)
        {
            return 0; // Avoid division by zero
        }

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
        if (enemy == null)
        {
            return EnemyPositional.None;
        }

        if (enemy is not IBattleChara)
        {
            return EnemyPositional.None;
        }

        Vector3 pPosition = enemy.Position;
        Vector3 faceVec = enemy.GetFaceVector();

        Vector3 dir = Player.Object.Position - pPosition;
        dir = Vector3.Normalize(dir);
        faceVec = Vector3.Normalize(faceVec);

        // Calculate the angle between the direction vector and the facing vector
        double dotProduct = Vector3.Dot(faceVec, dir);
        double angle = Math.Acos(dotProduct);

        const double frontAngle = Math.PI / 4;
        const double rearAngle = Math.PI * 3 / 4;

        if (angle < frontAngle)
        {
            return EnemyPositional.Front;
        }
        else if (angle > rearAngle)
        {
            return EnemyPositional.Rear;
        }

        return EnemyPositional.Flank;
    }

    /// <summary>
    /// Gets the facing direction vector of the game object.
    /// </summary>
    /// <param name="obj">The game object.</param>
    /// <returns>
    /// A <see cref="Vector3"/> representing the facing direction of the game object.
    /// </returns>
    internal static Vector3 GetFaceVector(this IGameObject obj)
    {
        if (obj == null)
        {
            return Vector3.Zero;
        }

        if (obj is not IBattleChara)
        {
            return Vector3.Zero;
        }

        float rotation = obj.Rotation;
        return new Vector3((float)Math.Sin(rotation), 0, (float)Math.Cos(rotation));
    }

    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="vec1">The first vector.</param>
    /// <param name="vec2">The second vector.</param>
    /// <returns>
    /// The angle in radians between the two vectors.
    /// </returns>
    internal static double AngleTo(this Vector3 vec1, Vector3 vec2)
    {
        double lengthProduct = vec1.Length() * vec2.Length();
        if (lengthProduct == 0)
        {
            return 0;
        }

        double dotProduct = Vector3.Dot(vec1, vec2);
        return Math.Acos(dotProduct / lengthProduct);
    }

    /// <summary>
    /// The distance from <paramref name="obj"/> to the player
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float DistanceToPlayer(this IGameObject? obj)
    {
        if (obj == null)
        {
            return float.MaxValue;
        }

        if (obj is not IBattleChara)
        {
            return float.MaxValue;
        }

        float distance = Vector3.Distance(Player.Object.Position, obj.Position) - (Player.Object.HitboxRadius + obj.HitboxRadius);
        return distance;
    }

}
