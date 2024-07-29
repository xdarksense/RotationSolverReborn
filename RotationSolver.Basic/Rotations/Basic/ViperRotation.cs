namespace RotationSolver.Basic.Rotations.Basic;

public partial class ViperRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Dexterity;

    /// <summary>
    /// Gets how many uses of uncoiled fury the player has.
    /// </summary>
    public static byte RattlingCoilStacks => JobGauge.RattlingCoilStacks;

    /// <summary>
    /// Gets Max stacks of Anguine Tribute.
    /// </summary>
    public static byte MaxRattling => EnhancedVipersRattleTrait.EnoughLevel ? (byte)3 : (byte)2;

    /// <summary>
    /// Gets Serpent Offering stacks and gauge.
    /// </summary>
    public static byte SerpentOffering => JobGauge.SerpentOffering;

    /// <summary>
    ///  Gets value indicating the use of 1st, 2nd, 3rd, 4th generation and Ouroboros.
    /// </summary>
    public static byte AnguineTribute => JobGauge.AnguineTribute;

    /// <summary>
    /// Gets the last Weaponskill used in DreadWinder/Pit of Dread combo. Dreadwinder = 1, HuntersCoil, SwiftskinsCoil, PitOfDread, HuntersDen, SwiftskinsDen
    /// </summary>
    public static DreadCombo DreadCombo => JobGauge.DreadCombo;

    /// <summary>
    /// Gets current ability for Serpent's Tail. NONE = 0, DEATHRATTLE = 1, LASTLASH = 2, FIRSTLEGACY = 3, SECONDLEGACY = 4, THIRDLEGACY = 5, FOURTHLEGACY = 6, TWINSREADY = 7, THRESHREADY = 8, UNCOILEDREADY = 9
    /// </summary>
    public static SerpentCombo SerpentCombo => JobGauge.SerpentCombo;

    /// <summary>
    /// Indicates if base Twins combo is ready.
    /// </summary>
    public static bool TWINSREADY => (byte)JobGauge.SerpentCombo == 7;

    /// <summary>
    /// Indicates if base Thresh combo is ready.
    /// </summary>
    public static bool THRESHREADY => (byte)JobGauge.SerpentCombo == 8;

    /// <summary>
    /// Indicates if base Uncoiled combo is ready.
    /// </summary>
    public static bool UNCOILEDREADY => (byte)JobGauge.SerpentCombo == 9;

    /// <summary>
    /// Gets Max stacks of Anguine Tribute.
    /// </summary>
    public static byte MaxAnguine => EnhancedSerpentsLineageTrait.EnoughLevel ? (byte)5 : (byte)4;

    /// <summary>
    /// Indicates if the player has upcoming Hind attack.
    /// </summary>
    public static bool HasHind => Player.HasStatus(true, StatusID.HindsbaneVenom) || Player.HasStatus(true, StatusID.HindstungVenom);

    /// <summary>
    /// Indicates if the player has upcoming Flanks attack.
    /// </summary>
    public static bool HasFlank => Player.HasStatus(true, StatusID.FlanksbaneVenom) || Player.HasStatus(true, StatusID.FlankstungVenom);

    /// <summary>
    /// Indicates if the player has upcoming Bane attack.
    /// </summary>
    public static bool HasBane => Player.HasStatus(true, StatusID.HindsbaneVenom) || Player.HasStatus(true, StatusID.FlanksbaneVenom);

    /// <summary>
    /// Indicates if the player has upcoming Bane attack.
    /// </summary>
    public static bool HasSting => Player.HasStatus(true, StatusID.HindstungVenom) || Player.HasStatus(true, StatusID.FlankstungVenom);

    /// <summary>
    /// Indicates if the player has no venom prepped.
    /// </summary>
    public static bool HasNoVenom => !Player.HasStatus(true, StatusID.HindstungVenom) && !Player.HasStatus(true, StatusID.FlankstungVenom) && !Player.HasStatus(true, StatusID.HindsbaneVenom) && !Player.HasStatus(true, StatusID.FlanksbaneVenom);

    static partial void ModifySteelFangsPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DreadCombo == 0 && !UNCOILEDREADY && !TWINSREADY && !THRESHREADY && (HasNoVenom || HasFlank);
    }

    static partial void ModifyHuntersStingPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DreadCombo == 0 && !UNCOILEDREADY && !TWINSREADY && !THRESHREADY && (HasNoVenom || HasFlank);
        setting.StatusProvide = [StatusID.HuntersInstinct];
        setting.CreateConfig = () => new ActionConfig()
        {
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifyDreadFangsPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasNoVenom || HasHind;
        setting.TargetStatusProvide = [StatusID.NoxiousGnash];
        setting.CreateConfig = () => new ActionConfig()
        {
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifyWrithingSnapPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MeleeRange;
    }

    static partial void ModifySwiftskinsStingPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasHind;
        setting.StatusProvide = [StatusID.Swiftscaled];
        setting.CreateConfig = () => new ActionConfig()
        {
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifySteelMawPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasNoVenom || HasFlank;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyFlankstingStrikePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasNoVenom || (HasFlank && HasSting);
        setting.ComboIds = [ActionID.HuntersStingPvE];
        setting.StatusProvide = [StatusID.HindstungVenom];
    }

    static partial void ModifyFlanksbaneFangPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasFlank && HasBane;
        setting.ComboIds = [ActionID.HuntersStingPvE];
        setting.StatusNeed = [StatusID.FlanksbaneVenom];
        setting.StatusProvide = [StatusID.HindsbaneVenom];
    }

    static partial void ModifyHindstingStrikePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasHind && HasSting;
        setting.ComboIds = [ActionID.SwiftskinsStingPvE];
        setting.StatusNeed = [StatusID.HindstungVenom];
        setting.StatusProvide = [StatusID.FlanksbaneVenom];
    }

    static partial void ModifyHindsbaneFangPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasHind && HasBane;
        setting.ComboIds = [ActionID.SwiftskinsStingPvE];
        setting.StatusNeed = [StatusID.HindstungVenom];
        setting.StatusProvide = [StatusID.FlankstungVenom];
    }

    static partial void ModifyDreadMawPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasNoVenom || HasHind;
        setting.TargetStatusProvide = [StatusID.NoxiousGnash];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifySlitherPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyHuntersBitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasNoVenom || HasFlank;
        setting.StatusProvide = [StatusID.HuntersInstinct];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifySwiftskinsBitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasHind;
        setting.StatusProvide = [StatusID.Swiftscaled];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyJaggedMawPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.GrimskinsVenom];
        setting.StatusNeed = [StatusID.GrimhuntersVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyBloodiedMawPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.GrimhuntersVenom];
        setting.StatusNeed = [StatusID.GrimskinsVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifySerpentsTailPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyDeathRattlePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.DEATHRATTLE;
    }

    static partial void ModifyLastLashPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.LASTLASH;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyDreadwinderPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RattlingCoilStacks < MaxRattling && AnguineTribute == 0 && DreadCombo == 0 && !TWINSREADY && !UNCOILEDREADY;
        setting.TargetStatusProvide = [StatusID.NoxiousGnash];
        setting.CreateConfig = () => new ActionConfig()
        {
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifyHuntersCoilPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentOffering <= 95 && DreadCombo == DreadCombo.Dreadwinder && !UNCOILEDREADY;
        setting.StatusProvide = [StatusID.HuntersVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            ShouldCheckStatus = false,
        };
    }

    static partial void ModifySwiftskinsCoilPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentOffering <= 95 && DreadCombo == DreadCombo.HuntersCoil && !UNCOILEDREADY;
        setting.StatusProvide = [StatusID.SwiftskinsVenom];
    }

    static partial void ModifyPitOfDreadPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RattlingCoilStacks < MaxRattling && AnguineTribute == 0 && DreadCombo == 0 && !THRESHREADY;
        setting.TargetStatusProvide = [StatusID.NoxiousGnash];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyHuntersDenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentOffering <= 95 && DreadCombo == DreadCombo.PitOfDread;
        setting.StatusProvide = [StatusID.HuntersInstinct, StatusID.FellhuntersVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }
    static partial void ModifySwiftskinsDenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentOffering <= 95 && DreadCombo == DreadCombo.HuntersDen;
        setting.StatusProvide = [StatusID.Swiftscaled, StatusID.FellskinsVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyTwinfangPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyTwinbloodPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyTwinfangBitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TWINSREADY;
        setting.StatusProvide = [StatusID.SwiftskinsVenom];
        setting.StatusNeed = [StatusID.HuntersVenom];
    }

    static partial void ModifyTwinbloodBitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TWINSREADY;
        setting.StatusProvide = [StatusID.HuntersVenom];
        setting.StatusNeed = [StatusID.SwiftskinsVenom];
    }

    static partial void ModifyTwinfangThreshPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => THRESHREADY;
        setting.StatusProvide = [StatusID.FellskinsVenom];
        setting.StatusNeed = [StatusID.FellhuntersVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyTwinbloodThreshPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => THRESHREADY;
        setting.StatusProvide = [StatusID.FellhuntersVenom];
        setting.StatusNeed = [StatusID.FellskinsVenom];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyUncoiledFuryPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RattlingCoilStacks >= 1 && !UNCOILEDREADY;
        setting.StatusProvide = [StatusID.PoisedForTwinfang];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySerpentsIrePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RattlingCoilStacks < MaxRattling && InCombat;
        setting.StatusProvide = [StatusID.ReadyToReawaken];
    }

    static partial void ModifyReawakenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DreadCombo == 0 && !UNCOILEDREADY && !TWINSREADY && !THRESHREADY && (SerpentOffering >= 50 || Player.HasStatus(true, StatusID.ReadyToReawaken));
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFirstGenerationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (MaxAnguine == 5 && AnguineTribute == 5) || (MaxAnguine == 4 && AnguineTribute == 4);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySecondGenerationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (MaxAnguine == 5 && AnguineTribute == 4) || (MaxAnguine == 4 && AnguineTribute == 3);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyThirdGenerationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (MaxAnguine == 5 && AnguineTribute == 3) || (MaxAnguine == 4 && AnguineTribute == 2);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFourthGenerationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (MaxAnguine == 5 && AnguineTribute == 2) || (MaxAnguine == 4 && AnguineTribute == 1);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyUncoiledTwinfangPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PoisedForTwinfang];
        setting.StatusProvide = [StatusID.PoisedForTwinblood];
        setting.ActionCheck = () => UNCOILEDREADY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyUncoiledTwinbloodPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PoisedForTwinblood];
        setting.ActionCheck = () => UNCOILEDREADY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyOuroborosPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => EnhancedSerpentsLineageTrait.EnoughLevel && AnguineTribute == 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFirstLegacyPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.FIRSTLEGACY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySecondLegacyPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.SECONDLEGACY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyThirdLegacyPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.THIRDLEGACY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFourthLegacyPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SerpentCombo == SerpentCombo.FOURTHLEGACY;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
}