using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using RotationSolver.Basic.Configuration;
using RotationSolver.Commands;

namespace RotationSolver.Updaters;

internal static class PreviewUpdater
{
    internal static void UpdatePreview()
    {
        UpdateEntry();
        UpdateCancelCast();
    }

    //public static byte Job => Job;
    static IDtrBarEntry? _dtrEntry;

    private static void UpdateEntry()
    {
        var showStr = RSCommands.EntryString;
        var icon = Player.Job switch
        {
            Job.WAR => BitmapFontIcon.Warrior,
            Job.PLD => BitmapFontIcon.Paladin,
            Job.DRK => BitmapFontIcon.DarkKnight,
            Job.GNB => BitmapFontIcon.Gunbreaker,

            Job.AST => BitmapFontIcon.Astrologian,
            Job.WHM => BitmapFontIcon.WhiteMage,
            Job.SGE => BitmapFontIcon.Sage,
            Job.SCH => BitmapFontIcon.Scholar,

            Job.BLM => BitmapFontIcon.BlackMage,
            Job.SMN => BitmapFontIcon.Summoner,
            Job.RDM => BitmapFontIcon.RedMage,
            Job.PCT => BitmapFontIcon.Pictomancer,

            Job.MNK => BitmapFontIcon.Monk,
            Job.SAM => BitmapFontIcon.Samurai,
            Job.DRG => BitmapFontIcon.Dragoon,
            Job.RPR => BitmapFontIcon.Reaper,
            Job.NIN => BitmapFontIcon.Ninja,
            Job.VPR => BitmapFontIcon.Viper,

            Job.BRD => BitmapFontIcon.Bard,
            Job.MCH => BitmapFontIcon.Machinist,
            Job.DNC => BitmapFontIcon.Dancer,
            _ => BitmapFontIcon.ExclamationRectangle,
        };

        if (Service.Config.ShowInfoOnDtr && !string.IsNullOrEmpty(showStr))
        {
            try
            {
                _dtrEntry ??= Svc.DtrBar.Get("Rotation Solver Reborn");
            }
            catch
            {
#pragma warning disable 0436
                WarningHelper.AddSystemWarning($"Unable to add server bar entry");
                return;
            }

            if (!_dtrEntry.Shown) _dtrEntry.Shown = true;

            _dtrEntry.Text = new SeString(
                new IconPayload(icon),
                new TextPayload(showStr)
            );
            _dtrEntry.OnClick = RSCommands.IncrementState;
        }
        else if (_dtrEntry != null && _dtrEntry.Shown)
        {
            _dtrEntry.Shown = false;
        }
    }

    static RandomDelay _tarStopCastDelay = new(() => Service.Config.StopCastingDelay);

    private static unsafe void UpdateCancelCast()
    {
        if (!Player.Object.IsCasting) return;

        var tarDead = Service.Config.UseStopCasting
            && Svc.Objects.SearchById(Player.Object.CastTargetObjectId) is IBattleChara b
            && b.IsEnemy() && b.CurrentHp == 0;

        var statusTimes = Player.Object.StatusTimes(false, [.. OtherConfiguration.NoCastingStatus.Select(i => (StatusID)i)]);

        var stopDueStatus = statusTimes.Any() && statusTimes.Min() > Player.Object.TotalCastTime - Player.Object.CurrentCastTime && statusTimes.Min() < 5;

        if (_tarStopCastDelay.Delay(tarDead) || stopDueStatus)
        {
            UIState.Instance()->Hotbar.CancelCast();
        }
    }


    internal static unsafe void PulseActionBar(uint actionID)
    {
        LoopAllSlotBar((bar, hot, index) =>
        {
            return IsActionSlotRight(bar, hot, actionID);
        });
    }


    private unsafe static bool IsActionSlotRight(ActionBarSlot slot, RaptureHotbarModule.HotbarSlot? hot, uint actionID)
    {
        if (hot.HasValue)
        {
            if (hot.Value.OriginalApparentSlotType != RaptureHotbarModule.HotbarSlotType.CraftAction && hot.Value.OriginalApparentSlotType != RaptureHotbarModule.HotbarSlotType.Action) return false;
            if (hot.Value.ApparentSlotType != RaptureHotbarModule.HotbarSlotType.CraftAction && hot.Value.ApparentSlotType != RaptureHotbarModule.HotbarSlotType.Action) return false;
        }

        return Service.GetAdjustedActionId((uint)slot.ActionId) == actionID;
    }

    unsafe delegate bool ActionBarAction(ActionBarSlot bar, RaptureHotbarModule.HotbarSlot? hot, uint highLightID);
    unsafe delegate bool ActionBarPredicate(ActionBarSlot bar, RaptureHotbarModule.HotbarSlot* hot);
    private static unsafe void LoopAllSlotBar(ActionBarAction doingSomething)
    {
        var index = 0;
        var hotBarIndex = 0;
        foreach (var intPtr in Service.GetAddons<AddonActionBar>()
            .Union(Service.GetAddons<AddonActionBarX>())
            .Union(Service.GetAddons<AddonActionCross>())
            .Union(Service.GetAddons<AddonActionDoubleCrossBase>()))
        {
            if (intPtr == IntPtr.Zero) continue;
            var actionBar = (AddonActionBarBase*)intPtr;
            var hotBar = Framework.Instance()->GetUIModule()->GetRaptureHotbarModule()->Hotbars[hotBarIndex];
            var slotIndex = 0;

            foreach (var slot in actionBar->ActionBarSlotVector.AsSpan())
            {
                var highLightId = 0x53550000 + index;

                if (doingSomething(slot, hotBarIndex > 9 ? null : hotBar.Slots[slotIndex], (uint)highLightId))
                {
                    var iconAddon = slot.Icon;
                    if ((IntPtr)iconAddon == IntPtr.Zero) continue;
                    if (!iconAddon->AtkResNode.IsVisible()) continue;
                    actionBar->PulseActionBarSlot(slotIndex);
                    UIModule.PlaySound(12, 0, 0, 0);
                }
                slotIndex++;
                index++;
            }
            hotBarIndex++;
        }
    }


    public unsafe static void Dispose()
    {

    }
}
