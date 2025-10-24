﻿namespace RotationSolver.Basic.Rotations.Basic;

public partial class AstrologianRotation
{
    #region JobGauge

    /// <summary>
    /// 
    /// </summary>
    public override MedicineType MedicineType => MedicineType.Mind;

    /// <summary>
    /// NONE = 0, BALANCE = 1, BOLE = 2, ARROW = 3, SPEAR = 4, EWERS = 5, SPIRE = 6
    /// </summary>
    protected static CardType[] DrawnCard => JobGauge.DrawnCards;
    /// <summary>
    /// 
    /// </summary>
    public static bool HasBalance
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Balance) return true; }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasBole
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Bole) return true; }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasArrow
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Arrow) return true; }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasSpear
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Spear) return true; }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasEwer
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Ewer) return true; }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool HasSpire
    {
        get
        {
            foreach (var card in DrawnCard) { if (card == CardType.Spire) return true; }
            return false;
        }
    }

    /// <summary>
    /// Indicates the state of Minor Arcana and which card will be used next when activating Minor Arcana, LORD = 7, LADY = 8
    /// </summary>
    protected static CardType DrawnCrownCard => JobGauge.DrawnCrownCard;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasLord => DrawnCrownCard == CardType.Lord;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasLady => DrawnCrownCard == CardType.Lady;

    /// <summary>
    ///  Can use Umbral or Astral draw, active draw matching what the next draw will be, ASTRAL, UMBRAL
    /// </summary>
    protected static DrawType ActiveDraw => JobGauge.ActiveDraw;

    /// <summary>
    /// Has NeutralSect
    /// </summary>
    public static bool HasNeutralSect => Player.HasStatus(true, StatusID.NeutralSect);

    /// <summary>
    /// Has Lightspeed.
    /// </summary>
    public static bool HasLightspeed => Player.HasStatus(true, StatusID.Lightspeed);

    /// <summary>
    /// Has Divination.
    /// </summary>
    public static bool HasDivination => Player.HasStatus(true, StatusID.Divination);

    /// <summary>
    /// Has Macrocosmos.
    /// </summary>
    public static bool HasMacrocosmos => Player.HasStatus(true, StatusID.Macrocosmos);

    /// <summary>
    /// Is holding bubble.
    /// </summary>
    public static bool HasCollectiveUnconscious => Player.HasStatus(true, StatusID.CollectiveUnconscious_848);

    /// <summary>
    /// Able to execute Giant Dominance Stellar Detonation.
    /// </summary>
    public static bool HasGiantDominance => Player.HasStatus(true, StatusID.GiantDominance);

    /// <summary>
    /// Able to execute Earthly Dominance Stellar Detonation.
    /// </summary>
    public static bool HasEarthlyDominance => Player.HasStatus(true, StatusID.EarthlyDominance);

    /// <summary>
    /// Has Synastry.
    /// </summary>
    public static bool HasSynastry => Player.HasStatus(true, StatusID.Synastry);
    #endregion

    #region Debug

    /// <inheritdoc/>
    public override void DisplayBaseStatus()
    {
        ImGui.Text($"DrawnCard: {string.Join(", ", DrawnCard)}");
        ImGui.Text($"DrawnCrownCard: {DrawnCrownCard}");
        ImGui.Text($"ActiveDraw: {ActiveDraw}");
        ImGui.Text($"RaiseMPMinimum: {RaiseMPMinimum}");
    }
    #endregion

    #region PvE Actions
    private protected sealed override IBaseAction? Raise => AscendPvE;

    private static readonly StatusID[] CombustStatus =
    [
        StatusID.Combust,
        StatusID.CombustIi,
        StatusID.CombustIii,
        StatusID.CombustIii_2041,
    ];

    static partial void ModifyMaleficPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyBeneficPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            GCDSingleHeal = true,
        };
    }

    static partial void ModifyCombustPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = CombustStatus;
    }

    static partial void ModifyLightspeedPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Lightspeed];
        setting.ActionCheck = () => !IsLastAbility(ActionID.LightspeedPvE);
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyHeliosPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyAscendPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
        setting.ActionCheck = () => Player.CurrentMp >= RaiseMPMinimum;
    }

    static partial void ModifyEssentialDignityPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifyBeneficIiPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            GCDSingleHeal = true,
        };
    }

    static partial void ModifyAstralDrawPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () =>
        {
            if (ActiveDraw != DrawType.Astral) return false;
            foreach (var card in DrawnCard) { if (card == CardType.Spear) return false; }
            return true;
        };
    }

    static partial void ModifyUmbralDrawPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () =>
        {
            if (ActiveDraw != DrawType.Umbral) return false;
            foreach (var card in DrawnCard) { if (card == CardType.Balance) return false; }
            return true;
        };
    }

    static partial void ModifyPlayIPvE(ref ActionSetting setting) //37019
    {

    }

    static partial void ModifyPlayIiPvE(ref ActionSetting setting) //37020
    {

    }

    static partial void ModifyPlayIiiPvE(ref ActionSetting setting) //37021
    {

    }

    static partial void ModifyTheBalancePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasBalance;
        setting.TargetStatusProvide = [StatusID.TheBalance_3887, StatusID.Weakness,
        StatusID.BrinkOfDeath];
        setting.TargetType = TargetType.TheBalance;
        setting.IsFriendly = true;
    }

    static partial void ModifyTheArrowPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasArrow;
        setting.TargetStatusProvide = [StatusID.TheArrow_3888];
        setting.TargetType = TargetType.BeAttacked;
        setting.IsFriendly = true;
    }

    static partial void ModifyTheSpirePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSpire;
        setting.TargetStatusProvide = [StatusID.TheSpire_3892];
        setting.TargetType = TargetType.BeAttacked;
        setting.IsFriendly = true;
    }

    static partial void ModifyTheSpearPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSpear;
        setting.TargetStatusProvide = [StatusID.TheSpear_3889, StatusID.Weakness,
        StatusID.BrinkOfDeath];
        setting.TargetType = TargetType.TheSpear;
        setting.IsFriendly = true;
    }

    static partial void ModifyTheBolePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasBole;
        setting.TargetStatusProvide = [StatusID.TheBole_3890];
        setting.TargetType = TargetType.BeAttacked;
        setting.IsFriendly = true;
    }

    static partial void ModifyTheEwerPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasEwer;
        setting.TargetStatusProvide = [StatusID.TheEwer_3891];
        setting.TargetType = TargetType.BeAttacked;
        setting.IsFriendly = true;
    }

    static partial void ModifyAspectedBeneficPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.AspectedBenefic];
        setting.CreateConfig = () => new ActionConfig()
        {
            GCDSingleHeal = true,
        };
        setting.IsFriendly = true;
    }

    static partial void ModifyAspectedHeliosPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.AspectedHelios];
        setting.UnlockedByQuestID = 67551;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyGravityPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67553;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyCombustIiPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = CombustStatus;
    }

    static partial void ModifySynastryPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Synastry_846];
        setting.StatusProvide = [StatusID.Synastry];
        setting.UnlockedByQuestID = 67554;
        setting.IsFriendly = true;
    }

    static partial void ModifyDivinationPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Divination];
        setting.StatusProvide = [StatusID.Divining]; //need to double check this status
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
            AoeCount = 1,
        };
    }

    static partial void ModifyMaleficIiPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67558;
    }

    static partial void ModifyCollectiveUnconsciousPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.CollectiveUnconscious, StatusID.WheelOfFortune];
        setting.UnlockedByQuestID = 67560;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyCelestialOppositionPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Opposition];
        setting.UnlockedByQuestID = 67561;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyEarthlyStarPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.EarthlyDominance, StatusID.GiantDominance];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
            AoeCount = 1,
        };
    }

    static partial void ModifyStellarDetonationPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.GiantDominance];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyMaleficIiiPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyMinorArcanaPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyLordOfCrownsPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasLord;
        setting.IsFriendly = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            // Keeping as one for use in boss fights, Players may optionally increase required hostile count
            AoeCount = 1,
        };
    }

    static partial void ModifyLadyOfCrownsPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasLady;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyCombustIiiPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = CombustStatus;
    }

    static partial void ModifyMaleficIvPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyCelestialIntersectionPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Intersection];
        setting.IsFriendly = true;
    }

    static partial void ModifyHoroscopePvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Horoscope];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHoroscopePvE_16558(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Horoscope, StatusID.HoroscopeHelios];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyNeutralSectPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 15,
        };
        setting.IsFriendly = true;
        setting.StatusProvide = [StatusID.NeutralSect, StatusID.Suntouched];
    }

    static partial void ModifyFallMaleficPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyGravityIiPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyExaltationPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Exaltation];
        setting.IsFriendly = true;
    }

    static partial void ModifyMacrocosmosPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Macrocosmos];
        setting.StatusProvide = [StatusID.Macrocosmos];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyMicrocosmosPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Macrocosmos];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyOraclePvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Divining];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHeliosConjunctionPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.HeliosConjunction];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySunSignPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Suntouched];
        setting.MPOverride = () => 0;
        setting.IsFriendly = true;
        setting.TargetType = TargetType.Self;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region PvP Actions
    static partial void ModifyFallMaleficPvP(ref ActionSetting setting)
    {

    }

    static partial void ModifyFallMaleficPvP_29246(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.DoubleCastPvP) == ActionID.FallMaleficPvP_29246;
        setting.MPOverride = () => 0;
    }

    static partial void ModifyAspectedBeneficPvP(ref ActionSetting setting)
    {

    }

    static partial void ModifyAspectedBeneficPvP_29247(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.DoubleCastPvP) == ActionID.AspectedBeneficPvP_29247;
        setting.MPOverride = () => 0;
    }

    static partial void ModifyGravityIiPvP(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyGravityIiPvP_29248(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.DoubleCastPvP) == ActionID.GravityIiPvP_29248;
        setting.MPOverride = () => 0;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyDoubleCastPvP(ref ActionSetting setting)
    {
        // You should never send the server this Action.
        setting.ActionCheck = () => false;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyMacrocosmosPvP(ref ActionSetting setting)
    {
        setting.IsFriendly = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyMicrocosmosPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.MacrocosmosPvP) == ActionID.MicrocosmosPvP;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyMinorArcanaPvP(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifyLadyOfCrownsPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.MinorArcanaPvP) == ActionID.LadyOfCrownsPvP;
        setting.IsFriendly = true;
        setting.MPOverride = () => 0;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyLordOfCrownsPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.MinorArcanaPvP) == ActionID.LordOfCrownsPvP;
        setting.IsFriendly = false;
        setting.MPOverride = () => 0;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyOraclePvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.Divining_4332);
        setting.MPOverride = () => 0;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyEpicyclePvP(ref ActionSetting setting)
    {
        //setting.SpecialType = SpecialActionType.MovingForward;
        setting.IsFriendly = true;
    }

    static partial void ModifyRetrogradePvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.RetrogradeReady);
        setting.IsFriendly = true;
        setting.MPOverride = () => 0;
        setting.SpecialType = SpecialActionType.MovingBackward;
    }
    #endregion
}