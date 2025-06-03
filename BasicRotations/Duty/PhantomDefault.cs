using RotationSolver.Basic.Rotations.Duties;

namespace RebornRotations.Duty;

[Rotation("Phantom Jobs Loaded", CombatType.PvE)]

public sealed class PhantomDefault : PhantomRotation
{
    //TODO: Add support for changing these in the UI
    [RotationConfig(CombatType.PvE, Name = "Phantom Oracle - Use Invulnerability for Starfall")]
    public bool SaveInvulnForStarfall { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Save Phantom Attacks for class specific damage bonus?")]
    public bool SaveForBurstWindow { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use Dark over Shock")]
    public bool PreferDarkCannon { get; set; }

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Average party HP percent to predict to heal with judgement instead of damage things")]
    public float PredictJudgementThreshold { get; set; } = 0.7f;

    [Range(0, 1, ConfigUnitType.Percent)]
    [RotationConfig(CombatType.PvE, Name = "Average party HP percent to predict to heal instead of damage things")]
    public float PredictBlessingThreshold { get; set; } = 0.5f;

    HashSet<IBaseAction> _remainingCards = new HashSet<IBaseAction>(4);
    private IBaseAction? _currentCard = null;

    public override void DisplayStatus()
    {
        base.DisplayStatus();
        ImGui.Text($"Remaining Cards: {_remainingCards.Count}");
    }

    public override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (VigilancePvE.CanUse(out act))
        {
            return true;
        }

        if (HandleOraclePrediction(nextGCD, out act))
        {
            return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    public override bool InterruptAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (OccultFalconPvE.CanUse(out act))
        {
            return true;
        }

        if (CleansingPvE.CanUse(out act)) // Technically this is an interrupt but it probably won't come up often
        {
            return true;
        }

        if (RomeosBalladPvE.CanUse(out act))
        {
            return true;
        }

        return base.InterruptAbility(nextGCD, out act);
    }

    public override bool DispelAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (RecuperationPvE.CanUse(out act))
        {
            return true;
        }

        return base.DispelAbility(nextGCD, out act);
    }

    public override bool GeneralAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (BattleBellPvE.CanUse(out act) && !BattleBellPvE.Target.Target.HasStatus(false, StatusID.BattleBell) && !BattleBellPvE.Target.Target.HasStatus(true, StatusID.BattleBell))
        {
            return true;
        }

        if (RingingRespitePvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && OffensiveAriaPvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && HerosRimePvE.CanUse(out act))
        {
            return true;
        }

        if (InCombat && PhantomAimPvE.CanUse(out act))
        {
            return true;
        }

        if (!IsMoving && OccultSprintPvE.CanUse(out act))
        {
            return true;
        }

        if (!InCombat && SuspendPvE.CanUse(out act))
        {
            return true;
        }

        return base.GeneralAbility(nextGCD, out act);
    }

    public override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }
        #region Utility/Non-scaling abilities that don't care about burst
        if (PhantomDoomPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultQuickPvE.CanUse(out act))
        {
            return true;
        }

        if (StealPvE.CanUse(out act))
        {
            return true;
        }
        #endregion Utility/Non-scaling abilities that don't care about burst

        if (DeadlyBlowPvE.CanUse(out act, skipComboCheck: true)) // Ideally we want to use this in burst windows, but 30 second cooldown means we can use it outside of burst windows too
        {
            if (BerserkerLevel == 2)
            {
                return true;
            }
            if (BerserkerLevel >= 3 && (!RagePvE.IsEnabled || Player.WillStatusEndGCD(1, 0, true, StatusID.PentupRage) || (RagePvE.Cooldown.IsCoolingDown && !Player.HasStatus(true, StatusID.PentupRage))))
            {
                return true;
            }
        }

        #region Burst abilities
        if (ShouldHoldBurst())
        {
            return false;
        }

        if (ZeninagePvE.CanUse(out act))
        {
            return true;
        }
        
        if (PhantomKickPvE.CanUse(out act))
        {
            return true;
        }

        //if (OccultCounterPvE.CanUse(out act))
        //{
        //    return true;
        //}
        #endregion Burst abilities

        return base.AttackAbility(nextGCD, out act);
    }

    public override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (PhantomGuardPvE.CanUse(out act))
        {
            return true;
        }

        if (InvulnerabilityPvE.CanUse(out act) && !SaveInvulnForStarfall)
        {
            return true;
        }

        if (ShirahadoriPvE.CanUse(out act))
        {
            return true;
        }

        if (CounterstancePvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleAbility(nextGCD, out act);
    }

    public override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (MightyMarchPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultUnicornPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomRejuvenationPvE.CanUse(out act))
        {
            return true;
        }

        if (OccultMageMasherPvE.CanUse(out act))
        {
            return true;
        }

        if (PilferWeaponPvE.CanUse(out act))
        {
            return true;
        }

        if (TrapDetectionPvE.CanUse(out act))
        {
            return true;
        }

        if (ShirahadoriPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseAreaAbility(nextGCD, out act);
    }

    public override bool HealSingleAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (OccultHealPvE.CanUse(out act))
        {
            return true;
        }

        if (BlessingPvE.CanUse(out act) && (PartyMembersAverHP < PredictBlessingThreshold || Player.GetHealthRatio() < PredictBlessingThreshold)) // Phantom heal gets a larger threshold than normal healing
        {
            return true;
        }

        if ((PartyMembersAverHP < PredictJudgementThreshold || Player.GetHealthRatio() < PredictJudgementThreshold) && PhantomJudgmentPvE.CanUse(out act)) // Heal the party or self if we're below the heal + damage threshold
        {
            return true;
        }

        if (OccultChakraPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealSingleAbility(nextGCD, out act);
    }

    public override bool HealAreaAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (BlessingPvE.CanUse(out act) && (PartyMembersAverHP < PredictBlessingThreshold || Player.GetHealthRatio() < PredictBlessingThreshold)) // Phantom heal gets a larger threshold than normal healing
        {
            return true;
        }

        if ((PartyMembersAverHP < PredictJudgementThreshold || Player.GetHealthRatio() < PredictJudgementThreshold) && PhantomJudgmentPvE.CanUse(out act)) // Heal the party or self if we're below the heal + damage threshold
        {
            return true;
        }

        return base.HealAreaAbility(nextGCD, out act);
    }

    public override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (OccultFeatherfootPvE.CanUse(out act))
        {
            return true;
        }

        return base.MoveForwardAbility(nextGCD, out act);
    }

    public override bool MyInterruptGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (MineuchiPvE.CanUse(out act))
        {
            return true;
        }

        return base.MyInterruptGCD(out act);
    }

    public override bool RaiseGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (RevivePvE.CanUse(out act))
        {
            return true;
        }

        return base.RaiseGCD(out act);
    }

    public override bool DispelGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (OccultDispelPvE.CanUse(out act))
        {
            return true;
        }

        return base.DispelGCD(out act);
    }

    public override bool HealSingleGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (OccultResuscitationPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealSingleGCD(out act);
    }

    public override bool HealAreaGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (SunbathPvE.CanUse(out act))
        {
            return true;
        }

        if (CloudyCaressPvE.CanUse(out act))
        {
            return true;
        }

        return base.HealAreaGCD(out act);
    }

    public override bool DefenseSingleGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (PrayPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseSingleGCD(out act);
    }

    public override bool DefenseAreaGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (CloudyCaressPvE.CanUse(out act))
        {
            return true;
        }

        if (BlessedRainPvE.CanUse(out act))
        {
            return true;
        }

        return base.DefenseAreaGCD(out act);
    }

    public override bool GeneralGCD(out IAction? act)
    {
        act = null;
        if (HasLockoutStatus)
        {
            return false;
        }

        if (InCombat && AetherialGainPvE.CanUse(out act))
        {
            return true;
        }

        if (!InCombat && HastyMiragePvE.CanUse(out act))
        {
            return true;
        }

        if (ShouldHoldBurst())
        {
            return false;
        }

        if (InCombat && PredictPvE.CanUse(out act))
        {
            return true;
        }

        if (SilverCannonPvE.CanUse(out act))
        {
            return true;
        }

        //TODO: If enemy is undead should we prioritize this over SilverCannon?
        //TODO2: Figure out a way to identify target is undead
        if (HolyCannonPvE.CanUse(out act))
        {
            return true;
        }

        // Only one of shock or dark can be used, prioritize Shock unless PreferDarkCannon is set
        if (ShockCannonPvE.CanUse(out act) && !PreferDarkCannon)
        {
            return true;
        }

        if (DarkCannonPvE.CanUse(out act))
        {
            return true;
        }

        if (PhantomFirePvE.CanUse(out act))
        {
            return true;
        }

        if (OccultCometPvE.CanUse(out act)) // Adding this to general swiftcast check is slightly more expensive for the many operations it will never be valid in
        {
            if (!IsRDM && !IsPLD)
            {
                return true;
            }
            if ((IsRDM && HasSwift) || (IsPLD && Player.HasStatus(true, StatusID.Requiescat)))
            {
                return true;
            }
        }

        if (IainukiPvE.CanUse(out act))
        {
            return true;
        }

        return base.GeneralGCD(out act);
    }

    private bool HandleOraclePrediction(IAction nextGCD, out IAction? act)
    {
        act = null;

        _currentCard ??= PredictPvE;

        // Track the current card we're seeing and discard the previous card if it's changed
        if (HasBlessing)
        {
            if (_currentCard != BlessingPvE) //card expired, remove the old card
            {
                _remainingCards.Remove(_currentCard);
            }
            _currentCard = BlessingPvE;
        }
        else if (HasCleansing)
        {
            if (_currentCard != CleansingPvE) //card expired, remove the old card
            {
                _remainingCards.Remove(_currentCard);
            }
            _currentCard = CleansingPvE;
        }
        else if (HasPhantomJudgment)
        {
            if (_currentCard != PhantomJudgmentPvE) //card expired, remove the old card
            {
                _remainingCards.Remove(_currentCard);
            }
            _currentCard = PhantomJudgmentPvE;
        }
        else if (HasStarfall)
        {
            if (_currentCard != StarfallPvE) //card expired, remove the old card
            {
                _remainingCards.Remove(_currentCard);
            }
            _currentCard = StarfallPvE;
        }
        else
        {
            // No predictions, clear the deck for next time
            ResetDeck();
            return false;
        }

        // Desired Card Actions before we get into forced actions
        // Check starfall-invuln combo
        if (StarfallPvE.CanUse(out act))
        {
            if (ShouldHoldBurst() && !Player.WillStatusEnd(3, true, StatusID.PredictionOfStarfall) && !Player.WillStatusEnd(3, false, StatusID.PredictionOfStarfall)) // If we're holding burst, we don't want to use starfall yet, but only to a point
            {
                return false;
            }

            if (HasTankInvuln) // already invuln'd
            {
                return true;
            }
            if (Player.GetEffectiveHpPercent() > 90 && (!HasTankStance || Player.GetEffectiveHpPercent() > 120) && (!SaveInvulnForStarfall || OracleLevel < 6)) // Not the tank or won't kill ourselves (directly) and either can't or won't be invuln'ing self for this
            {
                return true;
            }
        }

        // Do the invuln part of the combo
        if (InCombat && HasStarfall && SaveInvulnForStarfall && InvulnerabilityPvE.CanUse(out act))
        {
            // If we have starfall and can use Invulnerability and we've opted to save invuln for this we're going to make sure we use it on ourselves
            if (InvulnerabilityPvE.Target.Target != Player)
            {
                InvulnerabilityPvE.Target = new TargetResult(Player, [Player], Player.Position);
            }
            return true;
        }

        if (HasTankStance && (!SaveInvulnForStarfall || OracleLevel < 6)) // Tanking and either can't or won't invuln self
        {
            if (CleansingPvE.CanUse(out act)) // Cleansing is our highest potency option that doesn't rely on invulns
            {
                if (!ShouldHoldBurst() || Player.WillStatusEnd(3, true, StatusID.PredictionOfCleansing) || Player.WillStatusEnd(3, false, StatusID.PredictionOfCleansing))
                {
                    return true;
                }
            }
        }

        // Otherwise we need to go through our cards
        if (_remainingCards.Count == 1) // Last card! Play literally anything; if we screwed up starfall at least we go out with a bang
        {
            return _remainingCards.First().CanUse(out act); // If we can't use this for some reason, we're probably dead anyway
        }

        if (_remainingCards.Count == 2) // We have a little time but need to think it through
        {
            if (_remainingCards.Contains(StarfallPvE)) // We still have a starfall in the deck
            {
                if (_currentCard == StarfallPvE && InCombat) // We've opted not to use it above, so either we're below health threshold, or are tanking and not invuln
                {
                    // Let's just wait it out, maybe we'll get enough health to use it. We've got a card left in case.
                    return false;
                }

                // Otherwise it must be the last card
                if (OracleLevel >= 6 && !InvulnerabilityPvE.Cooldown.IsCoolingDown && SaveInvulnForStarfall) // We can use invuln, so this is fine
                {
                    return false;
                }

                if ((HasTankStance && Player.GetEffectiveHpPercent() < 120) // If we're tanking and got here we can't invuln and we don't seem safe enough to try this gimmick
                    || (Player.StatusTime(false, GetStatusForAction(_currentCard)) < 3f && Player.GetEffectiveHpPercent() <= 90)) // Or we don't have a lot of time and probably can't survive starfall
                {
                    // We must need to use our current card
                    if (_currentCard.CanUse(out act))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void ResetDeck()
    {
        _remainingCards.Add(BlessingPvE);
        _remainingCards.Add(CleansingPvE);
        _remainingCards.Add(PhantomJudgmentPvE);
        _remainingCards.Add(StarfallPvE);
    }

    private StatusID GetStatusForAction(IBaseAction card)
    {
        if (card == BlessingPvE)
        {
            return StatusID.PredictionOfBlessing;
        }
        else if (card == CleansingPvE)
        {
            return StatusID.PredictionOfCleansing;
        }
        else if (card == PhantomJudgmentPvE)
        {
            return StatusID.PredictionOfJudgment;
        }
        else if (card == StarfallPvE)
        {
            return StatusID.PredictionOfStarfall;
        }

        return StatusID.None;
    }

    private bool ShouldHoldBurst()
    {
        if (!SaveForBurstWindow)
        {
            return false;
        }

        return !InBurstWindow();
    }
}
