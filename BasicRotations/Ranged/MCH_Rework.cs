namespace RebornRotations.Ranged;

[Rotation("Rework", CombatType.PvE, GameVersion = "7.25")]
[SourceCode(Path = "main/BasicRotations/Ranged/MCH_Rework.cs")]
[Api(4)]
public sealed class MCH_Rework : MachinistRotation
{
    #region Config Options
    [RotationConfig(CombatType.PvE, Name = "Use Automation Queen as soon as its available if you are about to overcap")]
    private bool DumbQueen { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use burst medicine in countdown (requires auto burst option on)")]
    private bool OpenerBurstMeds { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Prevent the use of defense abilties during hypercharge burst")]
    private bool BurstDefense { get; set; } = false;

    [RotationConfig(CombatType.PvE, Name = "Use Bioblaster while moving")]
    private bool BioMove { get; set; } = true;
    #endregion

    #region Countdown logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 4.8f && ReassemblePvE.CanUse(out IAction? act))
        {
            return act;
        }

        if (IsBurst && OpenerBurstMeds && remainTime <= 1f && UseBurstMedicine(out act))
        {
            return act;
        }

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region oGCD Logic
    [RotationDesc(ActionID.TacticianPvE, ActionID.DismantlePvE)]
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if ((!BurstDefense || (BurstDefense && !IsOverheated)) && TacticianPvE.CanUse(out act))
        {
            return true;
        }

        if ((!BurstDefense || (BurstDefense && !IsOverheated)) && DismantlePvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    // Logic for using attack abilities outside of GCD, focusing on burst windows and cooldown management.
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (FullMetalFieldPvE.EnoughLevel && HasFullMetalMachinist && IsLastAction(false, WildfirePvE))
        {
            return false;
        }

        // Reassemble Logic
        // Check next GCD action and conditions for Reassemble.
        bool isReassembleUsable =
            //Reassemble current # of charges and double proc protection
            ReassemblePvE.Cooldown.CurrentCharges > 0 && !HasReassembled &&
            (nextGCD.IsTheSameTo(true, [ChainSawPvE, ExcavatorPvE])
            || (!ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, SpreadShotPvE) && ((IBaseAction)nextGCD).Target.AffectedTargets.Length >= (SpreadShotMasteryTrait.EnoughLevel ? 4 : 5))
            || nextGCD.IsTheSameTo(false, [AirAnchorPvE])
            || (!ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, DrillPvE))
            || (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE))
            || (!CleanShotPvE.EnoughLevel && nextGCD.IsTheSameTo(false, HotShotPvE)));
        // Attempt to use Reassemble if it's ready
        if (isReassembleUsable)
        {
            if (ReassemblePvE.CanUse(out act, usedUp: true))
            {
                return true;
            }
        }

        if (HyperchargePvE.CanUse(out act))
        {
            if (!WildfirePvE.EnoughLevel)
            {
                return true;
            }
            if (HasWildfire && !FullMetalFieldPvE.EnoughLevel)
            {
                return true;
            }
            if (HasWildfire && FullMetalFieldPvE.EnoughLevel && IsLastAction(false, FullMetalFieldPvE))
            {
                return true;
            }
        }

        // Start Ricochet/Gauss cooldowns rolling if they are not already
        if (!RicochetPvE.Cooldown.IsCoolingDown && RicochetPvE.CanUse(out act))
        {
            return true;
        }
        if (!GaussRoundPvE.Cooldown.IsCoolingDown && GaussRoundPvE.CanUse(out act))
        {
            return true;
        }

        if (BarrelStabilizerPvE.CanUse(out act))
        {
            return true;
        }

        bool LowLevelHyperCheck = !AutoCrossbowPvE.EnoughLevel && SpreadShotPvE.CanUse(out _);

        if (FullMetalFieldPvE.EnoughLevel)
        {
            if ((Heat >= 50 || HasHypercharged) && !LowLevelHyperCheck)
            {
                if (WeaponRemain < GCDTime(1) / 2 && nextGCD.IsTheSameTo(false, FullMetalFieldPvE) && WildfirePvE.CanUse(out act))
                {
                    return true;
                }
            }
        }
        if (!FullMetalFieldPvE.EnoughLevel)
        {
            if (WildfirePvE.Cooldown.WillHaveOneChargeGCD(1) && (IsLastAbility(false, HyperchargePvE) || Heat >= 50 || HasHypercharged) && ToolChargeSoon(out _) && !LowLevelHyperCheck)
            {
                if (WeaponRemain < GCDTime(1) / 2 && WildfirePvE.CanUse(out act))
                {
                    return true;
                }
            }
        }

        // Use Hypercharge if wildfire will not be up in 30 seconds or if you hit 100 heat
        if (!LowLevelHyperCheck && !HasReassembled && (!WildfirePvE.Cooldown.WillHaveOneCharge(30) || (Heat == 100)))
        {
            if (!(LiveComboTime <= 9f && LiveComboTime > 0f) && ToolChargeSoon(out act))
            {
                return true;
            }
        }

        // Decide which oGCD to use based on which has more RecastTimeElapsed
        var whichToUse = RicochetPvE.EnoughLevel switch
        {
            true when RicochetPvE.Cooldown.RecastTimeElapsed > GaussRoundPvE.Cooldown.RecastTimeElapsed => "Ricochet",
            true when GaussRoundPvE.Cooldown.RecastTimeElapsed > RicochetPvE.Cooldown.RecastTimeElapsed => "GaussRound",
            true => "Ricochet", // Default to Ricochet if equal
            _ => "GaussRound"
        };

        switch (whichToUse)
        {
            case "Ricochet":
                if (RicochetPvE.CanUse(out act, usedUp: true))
                    return true;
                break;
            case "GaussRound":
                if (GaussRoundPvE.CanUse(out act, usedUp: true))
                    return true;
                break;
        }

        // Rook Autoturret/Queen Logic
        if (RookAutoturretPvE.CanUse(out act, skipTTKCheck: true))
        {
            if (DumbQueen && InCombat && nextGCD.IsTheSameTo(true, HotShotPvE, AirAnchorPvE, ChainSawPvE, ExcavatorPvE) && Battery == 100)
            {
                return true;
            }
        }

        if (UseQueen(out act, nextGCD))
        {
            return true;
        }

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region GCD Logic
    protected override bool GeneralGCD(out IAction? act)
    {
        // ensure combo is not broken, okay to drop during overheat
        if (IsLastComboAction(true, SlugShotPvE) &&LiveComboTime >= GCDTime(1) && LiveComboTime <= GCDTime(2) && !IsOverheated)
        {
            // 3
            if (CleanShotPvE.CanUse(out act))
            {
                return true;
            }
        }

        // ensure combo is not broken, okay to drop during overheat
        if (IsLastComboAction(true, SplitShotPvE) && LiveComboTime >= GCDTime(1) && LiveComboTime <= GCDTime(2) && !IsOverheated)
        {
            // 2
            if (SlugShotPvE.CanUse(out act))
            {
                return true;
            }
        }

        // Overheated AOE
        if (AutoCrossbowPvE.CanUse(out act))
        {
            return true;
        }

        // Overheated ST
        if (HeatBlastPvE.CanUse(out act))
        {
            return true;
        }

        // Drill AOE
        if ((BioMove || (!IsMoving && !BioMove)) && BioblasterPvE.CanUse(out act, usedUp: true))
        {
            return true;
        }

        // ST Big GCDs
        if (!SpreadShotPvE.CanUse(out _))
        {
            // use AirAnchor if possible
            if (HotShotMasteryTrait.EnoughLevel
                && AirAnchorPvE.CanUse(out act))
            {
                return true;
            }

            // for opener: only use the first charge of Drill after AirAnchor when there are two
            if (EnhancedMultiweaponTrait.EnoughLevel
                && !CombatElapsedLessGCD(6)
                && !ChainSawPvE.Cooldown.WillHaveOneCharge(6)
                && !CleanShotPvE.CanUse(out _) && !SlugShotPvE.CanUse(out _)
                && DrillPvE.CanUse(out act, usedUp: false || (DrillPvE.Cooldown.CurrentCharges == 1 && DrillPvE.Cooldown.WillHaveXChargesGCD(2, 1, 0))))
            {
                return true;
            }

            if (!EnhancedMultiweaponTrait.EnoughLevel
                && DrillPvE.CanUse(out act))
            {
                return true;
            }

            if (!AirAnchorPvE.EnoughLevel
                && HotShotPvE.CanUse(out act))
            {
                return true;
            }
        }

        // ChainSaw is always used after Drill
        if (ChainSawPvE.CanUse(out act))
        {
            return true;
        }

        // use combo finisher asap
        if (ExcavatorPvE.CanUse(out act))
        {
            return true;
        }

        // use FMF after ChainSaw combo in 'alternative opener'
        if (FullMetalFieldPvE.CanUse(out act))
        {
            //Ensure the FMF is used if FMF status will end soon
            if (Player.WillStatusEndGCD(1, 0, true, StatusID.FullMetalMachinist))
            {
                return true;
            }
            if (!ChainSawPvE.Cooldown.WillHaveOneChargeGCD(2))
            {
                return true;
            }
        }

        // dont use the second charge of Drill if it's in opener, also save Drill for burst  --- need to combine this with the logic above!!!
        if (EnhancedMultiweaponTrait.EnoughLevel
            && !CombatElapsedLessGCD(6)
            && !ChainSawPvE.Cooldown.WillHaveOneCharge(6)
            && !CleanShotPvE.CanUse(out _) && !SlugShotPvE.CanUse(out _)
            && DrillPvE.CanUse(out act, usedUp: true))
        {
            return true;
        }

        // 1 AOE
        if (SpreadShotPvE.CanUse(out act))
        {
            return true;
        }

        // 3 ST
        if (CleanShotPvE.CanUse(out act))
        {
            return true;
        }
        // 2 ST
        if (SlugShotPvE.CanUse(out act))
        {
            return true;
        }
        // 1 ST
        if (SplitShotPvE.CanUse(out act))
        {
            return true;
        }

        return base.GeneralGCD(out act);
    }
    #endregion

    // Logic for Hypercharge
    private bool ToolChargeSoon(out IAction? act)
    {
        float REST_TIME = 8f;
        if
            //Cannot AOE
            (!SpreadShotPvE.CanUse(out _)
            &&
            // AirAnchor Enough Level % AirAnchor 
            ((AirAnchorPvE.EnoughLevel && AirAnchorPvE.Cooldown.WillHaveOneCharge(REST_TIME))
            ||
            // HotShot Charge Detection
            (!AirAnchorPvE.EnoughLevel && HotShotPvE.EnoughLevel && HotShotPvE.Cooldown.WillHaveOneCharge(REST_TIME))
            ||
            // Drill Charge Detection
            (DrillPvE.EnoughLevel && DrillPvE.Cooldown.WillHaveXCharges(DrillPvE.Cooldown.MaxCharges, REST_TIME))
            ||
            // Chainsaw Charge Detection
            (ChainSawPvE.EnoughLevel && ChainSawPvE.Cooldown.WillHaveOneCharge(REST_TIME))))
        {
            act = null;
            return false;
        }
        else
        {
            return HyperchargePvE.CanUse(out act);
        }
    }

    private bool UseQueen(out IAction? act, IAction nextGCD)
    {
        if (WildfirePvE.Cooldown.WillHaveOneChargeGCD(4)
            || !WildfirePvE.Cooldown.ElapsedAfter(10)
            || (nextGCD.IsTheSameTo(true, CleanShotPvE) && Battery == 100)
            || (nextGCD.IsTheSameTo(true, HotShotPvE, AirAnchorPvE, ChainSawPvE, ExcavatorPvE) && (Battery == 90 || Battery == 100)))
        {
            if (InCombat && RookAutoturretPvE.CanUse(out act))
            {
                return true;
            }
        }
        act = null;
        return false;
    }
}