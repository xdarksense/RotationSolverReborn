namespace RebornRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.15")]
[SourceCode(Path = "main/BasicRotations/Magical/PCT_Default.cs")]
[Api(4)]
public sealed class PCT_Default : PictomancerRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use HolyInWhite or CometInBlack while moving")]
    public bool HolyCometMoving { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Paint overcap protection.")]
    public bool UseCapCometHoly { get; set; } = true;

    [Range(1, 5, ConfigUnitType.None, 1)]
    [RotationConfig(CombatType.PvE, Name = "Paint overcap protection limit. How many paint you need to be at for it to use Holy out of burst (Setting is ignored when you have Hyperphantasia)")]
    public int HolyCometMax { get; set; } = 5;

    [RotationConfig(CombatType.PvE, Name = "Use swiftcast on Rainbow Drip (Priority over below settings)")]
    public bool RainbowDripSwift { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use swiftcast on Motif")]
    public bool MotifSwiftCastSwift { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Which Motif to use swiftcast on")]
    public CanvasFlags MotifSwiftCast { get; set; } = CanvasFlags.Weapon;

    #endregion

    #region Countdown logic
    // Defines logic for actions to take during the countdown before combat starts.
    protected override IAction? CountDownAction(float remainTime)
    {
        IAction act;
        if (!InCombat)
        {
            if (!CreatureMotifDrawn && PomMotifPvE.CanUse(out act)) return act;
            if (!WeaponMotifDrawn && HammerMotifPvE.CanUse(out act)) return act;
            if (!LandscapeMotifDrawn && StarrySkyMotifPvE.CanUse(out act) && !HasHyperphantasia) return act;
        }

        if (remainTime <= RainbowDripPvE.Info.CastTime + CountDownAhead && RainbowDripPvE.CanUse(out act))
        {
            return act;
        }

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Additional oGCD Logic

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (RainbowDripSwift && nextGCD == RainbowDripPvE && SwiftcastPvE.CanUse(out act)) return true;

            if (MotifSwiftCastSwift)
            {
                if (MotifSwiftCast switch
                {
                    CanvasFlags.Pom => nextGCD == PomMotifPvE,
                    CanvasFlags.Wing => nextGCD == WingMotifPvE,
                    CanvasFlags.Claw => nextGCD == ClawMotifPvE,
                    CanvasFlags.Maw => nextGCD == MawMotifPvE,
                    CanvasFlags.Weapon => nextGCD == HammerMotifPvE,
                    CanvasFlags.Landscape => nextGCD == StarrySkyMotifPvE,
                    _ => false
                } && SwiftcastPvE.CanUse(out act))
                {
                    return true;
                }
            }
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.SmudgePvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (SmudgePvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TemperaCoatPvE, ActionID.TemperaGrassaPvE, ActionID.AddlePvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        // Mitigations
        if (TemperaCoatPvE.CanUse(out act)) return true;
        if (TemperaGrassaPvE.CanUse(out act)) return true;
        if (AddlePvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.TemperaCoatPvE)]
    protected sealed override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        // Mitigations
        if (TemperaCoatPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    #endregion

    #region oGCD Logic

    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        bool burstTimingCheckerStriking = !ScenicMusePvE.Cooldown.WillHaveOneCharge(60) || HasStarryMuse || !StarryMusePvE.EnoughLevel;
        // Bursts
        int adjustCombatTimeForOpener = Player.Level < 92 ? 2 : 5;
        if (StarryMusePvE.CanUse(out act) && CombatTime > adjustCombatTimeForOpener && IsBurst) return true;
        if (CombatTime > adjustCombatTimeForOpener && StrikingMusePvE.CanUse(out act, usedUp: true) && burstTimingCheckerStriking) return true;
        if (SubtractivePalettePvE.CanUse(out act)) return true;

        if (HasStarryMuse)
        {
            if (FangedMusePvE.CanUse(out act, usedUp: true)) return true;
            if (RetributionOfTheMadeenPvE.CanUse(out act)) return true;
        }
        if (RetributionOfTheMadeenPvE.CanUse(out act)) return true;
        if (MogOfTheAgesPvE.CanUse(out act)) return true;
        if (StrikingMusePvE.CanUse(out act, usedUp: true) && burstTimingCheckerStriking) return true;
        if (PomMusePvE.CanUse(out act, usedUp: true)) return true;
        if (WingedMusePvE.CanUse(out act, usedUp: true)) return true;
        if (ClawedMusePvE.CanUse(out act, usedUp: true)) return true;

        //Basic Muses - not real actions
        //if (ScenicMusePvE.CanUse(out act)) return true;
        //if (SteelMusePvE.CanUse(out act, usedUp: true)) return true;
        //if (LivingMusePvE.CanUse(out act, usedUp: true)) return true;
        return base.AttackAbility(nextGCD, out act);
    }

    protected override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        if ((MergedStatus.HasFlag(AutoStatus.DefenseArea) || Player.WillStatusEndGCD(2, 0, true, StatusID.TemperaCoat)) && TemperaGrassaPvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic

    protected override bool GeneralGCD(out IAction? act)
    {
        // Weapon Painting Burst
        if (PolishingHammerPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (HammerBrushPvE.CanUse(out act, skipComboCheck: true)) return true;
        if (HammerStampPvE.CanUse(out act, skipComboCheck: true)) return true;

        if (HolyCometMoving && IsMoving && HolyInWhitePvE.CanUse(out act, usedUp: true)) return true;

        //Use up paint if in Hyperphantasia
        if (HasHyperphantasia && CometInBlackPvE.CanUse(out act)) return true;

        //Paint overcap protection
        if (Paint == HolyCometMax && HolyInWhitePvE.CanUse(out act)) return true;

        // Landscape Paining Burst
        if (HasRainbowBright && RainbowDripPvE.CanUse(out act, skipCastingCheck: HasRainbowBright)) return true;
        if (StarPrismPvE.CanUse(out act)) return true;

        // timings for motif casting
        if (ScenicMusePvE.Cooldown.RecastTimeRemainOneCharge <= 15 && !HasStarryMuse && !HasHyperphantasia)
        {
            if (StarrySkyMotifPvE.CanUse(out act) && !HasHyperphantasia) return true;
        }
        if ((LivingMusePvE.Cooldown.HasOneCharge || LivingMusePvE.Cooldown.RecastTimeRemainOneCharge <= CreatureMotifPvE.Info.CastTime * 1.7) && !HasStarryMuse && !HasHyperphantasia)
        {
            if (PomMotifPvE.CanUse(out act)) return true;
            if (WingMotifPvE.CanUse(out act)) return true;
            if (ClawMotifPvE.CanUse(out act)) return true;
            if (MawMotifPvE.CanUse(out act)) return true;
        }
        if ((SteelMusePvE.Cooldown.HasOneCharge || SteelMusePvE.Cooldown.RecastTimeRemainOneCharge <= WeaponMotifPvE.Info.CastTime) && !HasStarryMuse && !HasHyperphantasia)
        {
            if (HammerMotifPvE.CanUse(out act)) return true;
        }

        bool isMovingAndSwift = IsMoving && !Player.HasStatus(true, StatusID.Swiftcast);
        // white/black paint use while moving
        if (isMovingAndSwift)
        {
            if (PolishingHammerPvE.CanUse(out act)) return true;
            if (HammerBrushPvE.CanUse(out act)) return true;
            if (HammerStampPvE.CanUse(out act)) return true;
            if (HolyCometMoving)
            {
                if (CometInBlackPvE.CanUse(out act)) return true;
                if (HolyInWhitePvE.CanUse(out act)) return true;
            }
        }

        //Advanced Paintings
        if (PomMotifPvE.CanUse(out act)) return true;
        if (WingMotifPvE.CanUse(out act)) return true;
        if (ClawMotifPvE.CanUse(out act)) return true;
        if (MawMotifPvE.CanUse(out act)) return true;
        if (HammerMotifPvE.CanUse(out act)) return true;
        if (StarrySkyMotifPvE.CanUse(out act)) return true;

        //Basic Paintings - Not real actions
        //if (LandscapeMotifPvE.CanUse(out act)) return true;
        //if (WeaponMotifPvE.CanUse(out act)) return true;
        //if (CreatureMotifPvE.CanUse(out act)) return true;

        if (Paint == HolyCometMax && UseCapCometHoly)
        {
            if (CometInBlackPvE.CanUse(out act)) return true;
            if (HolyInWhitePvE.CanUse(out act)) return true;
        }

        //AOE Subtractive Inks
        if (ThunderIiInMagentaPvE.CanUse(out act)) return true;
        if (StoneIiInYellowPvE.CanUse(out act)) return true;
        if (BlizzardIiInCyanPvE.CanUse(out act)) return true;

        //AOE Additive Inks
        if (WaterIiInBluePvE.CanUse(out act)) return true;
        if (AeroIiInGreenPvE.CanUse(out act)) return true;
        if (FireIiInRedPvE.CanUse(out act)) return true;

        //ST Subtractive Inks
        if (ThunderInMagentaPvE.CanUse(out act)) return true;
        if (StoneInYellowPvE.CanUse(out act)) return true;
        if (BlizzardInCyanPvE.CanUse(out act)) return true;

        //ST Additive Inks
        if (WaterInBluePvE.CanUse(out act)) return true;
        if (AeroInGreenPvE.CanUse(out act)) return true;
        if (FireInRedPvE.CanUse(out act)) return true;
        return base.GeneralGCD(out act);
    }

    #endregion
}