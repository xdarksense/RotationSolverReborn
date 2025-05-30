using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static class Watcher
{
    public static void Enable()
    {
        ActionEffect.ActionEffectEvent += ActionFromEnemy;
        ActionEffect.ActionEffectEvent += ActionFromSelf;
    }

    public static void Disable()
    {
        ActionEffect.ActionEffectEvent -= ActionFromEnemy;
        ActionEffect.ActionEffectEvent -= ActionFromSelf;
    }

    public static string ShowStrSelf { get; private set; } = string.Empty;
    public static string ShowStrEnemy { get; private set; } = string.Empty;

    private static void ActionFromEnemy(ActionEffectSet set)
    {
        try
        {
            // Validate source is an enemy battle character
            if (set.Source is not IBattleChara battle ||
                battle is IPlayerCharacter ||
                battle.SubKind == 9) // Friend!
            {
                return;
            }

            if (Svc.Objects.SearchById(battle.GameObjectId) is not IBattleChara obj ||
                obj is IPlayerCharacter)
            {
                return;
            }

            IPlayerCharacter playerObject = Player.Object;
            if (playerObject == null)
            {
                return;
            }
            ulong playerId = playerObject.GameObjectId;

            // Calculate damage ratio to player
            float damageRatio = 0;
            foreach (var effect in set.TargetEffects)
            {
                if (effect.TargetID != playerId) continue;
                for (int i = 0; i < 8; i++)
                {
                    var entry = effect[i];
                    if (entry.type == ActionEffectType.Damage)
                    {
                        damageRatio += (float)entry.value / playerObject.MaxHp;
                    }
                }
            }
            DataCenter.AddDamageRec(damageRatio);
            ShowStrEnemy = $"Damage Ratio: {damageRatio}\n{set}";

            // Knockback detection (only need to check player's effect)
            foreach (var effect in set.TargetEffects)
            {
                if (effect.TargetID != playerId) continue;
                if (effect.GetSpecificTypeEffect(ActionEffectType.Knockback, out var entry))
                {
                    var knock = Svc.Data.GetExcelSheet<Knockback>()?.GetRow(entry.value);
                    if (knock != null)
                    {
                        DataCenter.KnockbackStart = DateTime.Now;
                        if (knock.HasValue)
                        {
                            DataCenter.KnockbackFinished = DateTime.Now + TimeSpan.FromSeconds(knock.Value.Distance / (float)knock.Value.Speed);
                        }
                        if (set.Action.HasValue &&
                            !OtherConfiguration.HostileCastingKnockback.Contains(set.Action.Value.RowId) &&
                            Service.Config.RecordKnockbackies)
                        {
                            _ = OtherConfiguration.HostileCastingKnockback.Add(set.Action.Value.RowId);
                            _ = OtherConfiguration.Save();
                        }
                    }
                    break; // Only need first knockback
                }
            }

            // Area effect detection for party
            if (set.Header.ActionType == ActionType.Action &&
                DataCenter.PartyMembers.Count >= 4 &&
                set.Action?.Cast100ms > 0)
            {
                var type = set.Action?.GetActionCate();
                if (type is ActionCate.Spell or ActionCate.Weaponskill or ActionCate.Ability)
                {
                    var partyMembers = DataCenter.PartyMembers;
                    int partyMemberCount = partyMembers.Count;
                    int damageEffectCount = 0;

                    // Build a HashSet for fast lookup if party is large
                    HashSet<ulong>? partyIds = null;
                    if (partyMemberCount > 4)
                    {
                        partyIds = [.. partyMembers.Select(m => m.GameObjectId)];
                    }

                    foreach (var effect in set.TargetEffects)
                    {
                        bool isPartyMember = partyIds != null
                            ? partyIds.Contains(effect.TargetID)
                            : partyMembers.Any(m => m.GameObjectId == effect.TargetID);

                        if (!isPartyMember) continue;

                        if (effect.GetSpecificTypeEffect(ActionEffectType.Damage, out var damageEffect) &&
                            (damageEffect.value > 0 || (damageEffect.param0 & 6) == 6))
                        {
                            damageEffectCount++;
                        }
                    }

                    if (damageEffectCount == partyMemberCount && Service.Config.RecordCastingArea)
                    {
                        _ = OtherConfiguration.HostileCastingArea.Add(set.Action!.Value.RowId);
                        _ = OtherConfiguration.SaveHostileCastingArea();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in ActionFromEnemy: {ex}");
        }
    }


    private static void ActionFromSelf(ActionEffectSet set)
    {
        try
        {
            var playerObject = Player.Object;
            if (set.Source == null || playerObject == null) return;
            if (set.Source.GameObjectId != playerObject.GameObjectId) return;
            if (set.Header.ActionType is not ActionType.Action and not ActionType.Item) return;
            if (set.Action is not { } action) return;
            if (action.ActionCategory.Value.RowId == (uint)ActionCate.Autoattack) return;
            if (set.TargetEffects.Length == 0) return;

            var tar = set.Target;

            // Record
            DataCenter.AddActionRec(action);
            ShowStrSelf = set.ToString();

            DataCenter.HealHP = set.GetSpecificTypeEffect(ActionEffectType.Heal);
            DataCenter.ApplyStatus = set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectTarget);

            var effects = set.GetSpecificTypeEffect(ActionEffectType.ApplyStatusEffectSource);
            if (effects is { Count: > 0 })
            {
                foreach (var effect in effects)
                {
                    DataCenter.ApplyStatus[effect.Key] = effect.Value;
                }
            }

            uint mpGain = 0;
            foreach (var effect in set.GetSpecificTypeEffect(ActionEffectType.MpGain))
            {
                if (effect.Key == playerObject.GameObjectId)
                {
                    mpGain += effect.Value;
                }
            }
            DataCenter.MPGain = mpGain;

            DataCenter.EffectTime = DateTime.Now;
            DataCenter.EffectEndTime = DateTime.Now.AddSeconds(set.Header.AnimationLockTime + 1);

            var attackedTargets = DataCenter.AttackedTargets;
            int attackedTargetsCount = DataCenter.AttackedTargetsCount;

            // Build a HashSet for fast lookup
            var attackedIds = new HashSet<ulong>(attackedTargets.Select(t => t.id));

            foreach (var effect in set.TargetEffects)
            {
                if (!effect.GetSpecificTypeEffect(ActionEffectType.Damage, out _)) continue;
                if (attackedIds.Contains(effect.TargetID)) continue;

                // Maintain queue size and uniqueness
                while (attackedTargets.Count >= attackedTargetsCount)
                {
                    var (id, time) = attackedTargets.Peek();
                    if (id == effect.TargetID) break;
                    attackedIds.Remove(attackedTargets.Dequeue().id);
                }

                attackedTargets.Enqueue((effect.TargetID, DateTime.Now));
                attackedIds.Add(effect.TargetID);
            }

            // Macro
            var regexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            foreach (var item in Service.Config.Events)
            {
                if (!Regex.IsMatch(action.Name.ExtractText(), item.Name, regexOptions)) continue;
                if (item.AddMacro(tar)) break;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error in ActionFromSelf: {ex}");
        }
    }

}