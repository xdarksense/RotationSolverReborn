namespace RotationSolver.Basic.Rotations.Basic;

public partial class PictomancerRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Intelligence;

    #region Job Gauge
    /// <summary>
    /// Tracks use of subjective pallete
    /// </summary>
    public static byte PaletteGauge => JobGauge.PalleteGauge;

    /// <summary>
    /// Number of paint the player has.
    /// </summary>
    public static byte Paint => JobGauge.Paint;

    /// <summary>
    /// Creature Motif Stack
    /// </summary>
    public static bool CreatureMotifDrawn => JobGauge.CreatureMotifDrawn;

    /// <summary>
    /// Weapon Motif Stack
    /// </summary>
    public static bool WeaponMotifDrawn => JobGauge.WeaponMotifDrawn;

    /// <summary>
    /// Landscape Motif Stack
    /// </summary>
    public static bool LandscapeMotifDrawn => JobGauge.LandscapeMotifDrawn;

    /// <summary>
    /// Moogle Portrait Stack
    /// </summary>
    public static bool MooglePortraitReady => JobGauge.MooglePortraitReady;

    /// <summary>
    /// Madeen Portrait Stack
    /// </summary>
    public static bool MadeenPortraitReady => JobGauge.MadeenPortraitReady;

    /// <summary>
    /// Which creature flags are present. Pom = 1, Wings = 2, Claw = 4, MooglePortait = 0x10, MadeenPortrait = 0x20, these are the small paintings above Maw/Pom
    /// </summary>
    public static CreatureFlags CreatureFlags => JobGauge.CreatureFlags;

    /// <summary>
    /// Is Pom Motif ready
    /// </summary>
    public static bool isPomMotifReady => ((byte)JobGauge.CreatureFlags & 32) == 32 || ((byte)JobGauge.CreatureFlags & 0) == 0;

    /// <summary>
    /// Is Wing Motif ready
    /// </summary>
    public static bool isWingMotifReady => ((byte)JobGauge.CreatureFlags & 33) == 33 || ((byte)JobGauge.CreatureFlags & 1) == 1;

    /// <summary>
    /// Is Claw Motif ready
    /// </summary>
    public static bool isClawMotifReady => ((byte)JobGauge.CreatureFlags & 19) == 19 || ((byte)JobGauge.CreatureFlags & 3) == 3;

    /// <summary>
    /// Indicates that the player is not in a Dread Combo.
    /// </summary>
    public static bool isMawMotifReady => ((byte)JobGauge.CreatureFlags & 23) == 23 || ((byte)JobGauge.CreatureFlags & 7) == 7;


    /// <summary>
    /// Which canvas flags are present.  Pom = 1, Wing = 2, Claw = 4, Maw = 8, Weapon = 0x10, Landscape = 0x20, these are the motif flags
    /// </summary>
    public static CanvasFlags CanvasFlags => JobGauge.CanvasFlags;

    /// <summary>
    /// Is Wing ready
    /// </summary>
    public static bool isPomMuseReady => ((byte)JobGauge.CanvasFlags & 1) == 1 || ((byte)JobGauge.CanvasFlags & 17) == 17 || ((byte)JobGauge.CanvasFlags & 33) == 33 || ((byte)JobGauge.CanvasFlags & 49) == 49;

    /// <summary>
    /// Is Claw ready
    /// </summary>
    public static bool isWingMuseReady => ((byte)JobGauge.CanvasFlags & 2) == 2 || ((byte)JobGauge.CanvasFlags & 18) == 18 || ((byte)JobGauge.CanvasFlags & 34) == 34 || ((byte)JobGauge.CanvasFlags & 50) == 50;

    /// <summary>
    /// Is Pom ready
    /// </summary>
    public static bool isClawMuseReady => ((byte)JobGauge.CanvasFlags & 4) == 4 || ((byte)JobGauge.CanvasFlags & 20) == 20 || ((byte)JobGauge.CanvasFlags & 36) == 36 || ((byte)JobGauge.CanvasFlags & 52) == 52;

    /// <summary>
    /// Is Maw ready
    /// </summary>
    public static bool isMawMuseReady => ((byte)JobGauge.CanvasFlags & 8) == 8 || ((byte)JobGauge.CanvasFlags & 24) == 24 || ((byte)JobGauge.CanvasFlags & 40) == 40 || ((byte)JobGauge.CanvasFlags & 56) == 56;

    /// <summary>
    /// Is Hammer ready
    /// </summary>
    public static bool isHammerMuseReady => ((byte)JobGauge.CanvasFlags & 16) == 16 || ((byte)JobGauge.CanvasFlags & 17) == 17 || ((byte)JobGauge.CanvasFlags & 18) == 18 || ((byte)JobGauge.CanvasFlags & 20) == 20
        || ((byte)JobGauge.CanvasFlags & 24) == 24 || ((byte)JobGauge.CanvasFlags & 48) == 48 || ((byte)JobGauge.CanvasFlags & 49) == 49 
        || ((byte)JobGauge.CanvasFlags & 50) == 50 || ((byte)JobGauge.CanvasFlags & 52) == 52 
        || ((byte)JobGauge.CanvasFlags & 56) == 56;

    /// <summary>
    /// Is Starry ready
    /// </summary>
    public static bool isStarryMuseReady => ((byte)JobGauge.CanvasFlags & 32) == 32 || ((byte)JobGauge.CanvasFlags & 33) == 33 || ((byte)JobGauge.CanvasFlags & 34) == 34 || ((byte)JobGauge.CanvasFlags & 36) == 36
        || ((byte)JobGauge.CanvasFlags & 40) == 40 || ((byte)JobGauge.CanvasFlags & 48) == 48 || ((byte)JobGauge.CanvasFlags & 49) == 49
        || ((byte)JobGauge.CanvasFlags & 50) == 50 || ((byte)JobGauge.CanvasFlags & 52) == 52
        || ((byte)JobGauge.CanvasFlags & 56) == 56;

    #endregion

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text($"PaletteGauge: {PaletteGauge}");
        ImGui.Text($"Paint: {Paint}");
        ImGui.Text($"CreatureMotifDrawn: {CreatureMotifDrawn}");
        ImGui.Text($"WeaponMotifDrawn: {WeaponMotifDrawn}");
        ImGui.Text($"LandscapeMotifDrawn: {LandscapeMotifDrawn}");
        ImGui.Text($"MooglePortraitReady: {MooglePortraitReady}");
        ImGui.Text($"MadeenPortraitReady: {MadeenPortraitReady}");
        ImGui.Text($"CreatureFlags: {CreatureFlags}");
        ImGui.Text($"isPomMotifReady: {isPomMotifReady}");
        ImGui.Text($"isWingMotifReady: {isWingMotifReady}");
        ImGui.Text($"isClawMotifReady: {isClawMotifReady}");
        ImGui.Text($"isMawMotifReady: {isMawMotifReady}");
        ImGui.Text($"CanvasFlags: {CanvasFlags}");
        ImGui.Text($"isPomMuseReady: {isPomMuseReady}");
        ImGui.Text($"isWingMuseReady: {isWingMuseReady}");
        ImGui.Text($"isClawMuseReady: {isClawMuseReady}");
        ImGui.Text($"isMawMuseReady: {isMawMuseReady}");
        ImGui.Text($"isHammerMuseReady: {isHammerMuseReady}");
        ImGui.Text($"isStarryMuseReady: {isStarryMuseReady}");
        ImGui.Text($"MaxStrikingMuse: {MaxStrikingMuse}");
        ImGui.Text($"Level100: {Level100}");
        ImGui.Text($"HasSubtractivePalette: {HasSubtractivePalette}");
        ImGui.Text($"HasAetherhues: {HasAetherhues}");
        ImGui.Text($"HasAetherhues2: {HasAetherhues2}");
        ImGui.Text($"HasSubtractiveSpectrum: {HasSubtractiveSpectrum}");
        ImGui.Text($"HasHyperphantasia: {HasHyperphantasia}");
        ImGui.Text($"HasHammerTime: {HasHammerTime}");
        ImGui.Text($"HasMonochromeTones: {HasMonochromeTones}");
        ImGui.Text($"HasStarryMuse: {HasStarryMuse}");
        ImGui.Text($"HammerStacks: {HammerStacks}");
        ImGui.Text($"SubtractiveStacks: {SubtractiveStacks}");
    }

    #region Job States

    /// <summary>
    /// Number of max charges Striking Muse can have
    /// </summary>
    public static byte MaxStrikingMuse => EnhancedPictomancyIiTrait.EnoughLevel ? (byte)2 : (byte)1;

    /// <summary>
    /// Determines if player is max level or not
    /// </summary>
    public static bool Level100 => EnhancedPictomancyVTrait.EnoughLevel;

    #endregion

    #region Statuses
    /// <summary>
    /// Indicates if the player has Aetherhues.
    /// </summary>
    public static bool HasAetherhues => Player.HasStatus(true, StatusID.Aetherhues);

    /// <summary>
    /// Indicates if the player has Aetherhues II.
    /// </summary>
    public static bool HasAetherhues2 => Player.HasStatus(true, StatusID.AetherhuesIi);

    /// <summary>
    /// Indicates if the player has Subtractive Palette.
    /// </summary>
    public static bool HasSubtractivePalette => Player.HasStatus(true, StatusID.SubtractivePalette);

    /// <summary>
    /// Indicates if the player has Subtractive Spectrum.
    /// </summary>
    public static bool HasSubtractiveSpectrum => Player.HasStatus(true, StatusID.SubtractiveSpectrum);

    /// <summary>
    /// Indicates if the player has Hyperphantasia.
    /// </summary>
    public static bool HasHyperphantasia => Player.HasStatus(true, StatusID.Hyperphantasia);

    /// <summary>
    /// Indicates if the player has Hammer Time.
    /// </summary>
    public static bool HasHammerTime => Player.HasStatus(true, StatusID.HammerTime);

    /// <summary>
    /// Indicates if the player has Monochrome Tones.
    /// </summary>
    public static bool HasMonochromeTones => Player.HasStatus(true, StatusID.MonochromeTones);

    /// <summary>
    /// Indicates if the player has Starry Muse.
    /// </summary>
    public static bool HasStarryMuse => Player.HasStatus(true, StatusID.StarryMuse);

    /// <summary>
    /// Holds the remaining amount of HammerTime stacks
    /// </summary>
    public static byte HammerStacks
    {
        get
        {
            byte stacks = Player.StatusStack(true, StatusID.HammerTime);
            return stacks == byte.MaxValue ? (byte)3 : stacks;
        }
    }

    /// <summary>
    /// Holds the remaining amount of SubtractivePalette stacks
    /// </summary>
    public static byte SubtractiveStacks
    {
        get
        {
            byte stacks = Player.StatusStack(true, StatusID.SubtractivePalette);
            return stacks == byte.MaxValue ? (byte)3 : stacks;
        }
    }

    #endregion

    static partial void ModifyFireInRedPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasSubtractivePalette;
        setting.StatusProvide = [StatusID.Aetherhues];
    }

    static partial void ModifyAeroInGreenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasSubtractivePalette && HasAetherhues;
        setting.StatusProvide = [StatusID.AetherhuesIi];
    }

    static partial void ModifyTemperaCoatPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.TemperaCoat];
    }

    static partial void ModifyWaterInBluePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (Paint <= 4) && (PaletteGauge <= 75) && !HasSubtractivePalette && HasAetherhues2;
    }

    static partial void ModifySmudgePvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyFireIiInRedPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasSubtractivePalette;
        setting.StatusProvide = [StatusID.Aetherhues];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyCreatureMotifPvE(ref ActionSetting setting)
    {
        
    }

    static partial void ModifyLivingMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CreatureMotifDrawn;
    }

    static partial void ModifyMogOfTheAgesPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MooglePortraitReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyPomMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !CreatureMotifDrawn && isPomMotifReady;
    }

    static partial void ModifyWingMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !CreatureMotifDrawn && isWingMotifReady;
    }

    static partial void ModifyPomMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => isPomMuseReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyWingedMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => isWingMuseReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyAeroIiInGreenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasSubtractivePalette && HasAetherhues;
        setting.StatusProvide = [StatusID.AetherhuesIi];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyWaterIiInBluePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (Paint <= 4) && (PaletteGauge <= 75) && !HasSubtractivePalette && HasAetherhues2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyWeaponMotifPvE(ref ActionSetting setting)
    {
        
    }

    static partial void ModifySteelMusePvE(ref ActionSetting setting)
    {
        
    }

    static partial void ModifyHammerStampPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HammerStacks == 3 || (!EnhancedPictomancyIiTrait.EnoughLevel && HammerStacks > 0);
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHammerMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasHammerTime && !WeaponMotifDrawn;
    }

    static partial void ModifyStrikingMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => WeaponMotifDrawn && InCombat;
    }

    static partial void ModifyBlizzardInCyanPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSubtractivePalette;
        setting.StatusProvide = [StatusID.Aetherhues];
    }

    static partial void ModifyBlizzardIiInCyanPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSubtractivePalette;
        setting.StatusProvide = [StatusID.Aetherhues];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifySubtractivePalettePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasSubtractivePalette && (PaletteGauge >= 50 || HasSubtractiveSpectrum) && !HasMonochromeTones;
        setting.StatusProvide = [StatusID.SubtractivePalette];
    }

    static partial void ModifyStoneInYellowPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSubtractivePalette && HasAetherhues;
        setting.StatusProvide = [StatusID.AetherhuesIi];
    }

    static partial void ModifyThunderInMagentaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Paint <= 4 && HasSubtractivePalette && HasAetherhues2;
    }

    static partial void ModifyStoneIiInYellowPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasSubtractivePalette && HasAetherhues;
        setting.StatusProvide = [StatusID.AetherhuesIi];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyThunderIiInMagentaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Paint <= 4 && HasSubtractivePalette && HasAetherhues2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyLandscapeMotifPvE(ref ActionSetting setting)
    {
        
    }

    static partial void ModifyScenicMusePvE(ref ActionSetting setting)
    {
        
    }

    static partial void ModifyStarrySkyMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !HasStarryMuse && !LandscapeMotifDrawn;
    }

    static partial void ModifyStarryMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => isStarryMuseReady && InCombat;
        setting.StatusProvide = [StatusID.Starstruck, StatusID.SubtractiveSpectrum, StatusID.Inspiration, StatusID.Hyperphantasia, StatusID.RainbowBright];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHolyInWhitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Paint > 0 && !HasMonochromeTones;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHammerBrushPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HammerStacks == 2;
        setting.ComboIds = [ActionID.HammerStampPvE];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyPolishingHammerPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HammerStacks == 1;
        setting.ComboIds = [ActionID.HammerBrushPvE];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyTemperaGrassaPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.TemperaCoat];
        setting.StatusProvide = [StatusID.TemperaGrassa];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyCometInBlackPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Paint > 0 && HasMonochromeTones;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyRainbowDripPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.RainbowBright];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyClawMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !CreatureMotifDrawn && isClawMotifReady;
    }

    static partial void ModifyMawMotifPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !CreatureMotifDrawn && isMawMotifReady;
    }

    static partial void ModifyClawedMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => isClawMuseReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyFangedMusePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => isMawMuseReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyRetributionOfTheMadeenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MadeenPortraitReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyStarPrismPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Starstruck];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
}
