namespace RotationSolver.Basic.Rotations.Duties;

/// <summary>
/// Represents a rotation for phantom duties in the game.
/// </summary>
[DutyTerritory(1252)] // TODO: Verify IDs.
public partial class PhantomRotation : DutyRotation
{
}

public partial class DutyRotation
{
    #region Freelancer
    /// <summary>
    /// Modifies the settings for Occult Resuscitation.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultResuscitationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => FreelancerLevel >= 5;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Treasuresight.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultTreasuresightPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => FreelancerLevel >= 10;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Knight
    /// <summary>
    /// Modifies the settings for Phantom Guard.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomGuardPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 1;
    }

    /// <summary>
    /// Modifies the settings for Pray.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPrayPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 2;
        setting.StatusProvide = [StatusID.Pray];
    }

    /// <summary>
    /// Modifies the settings for Occult Heal.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultHealPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 3;
    }

    /// <summary>
    /// Modifies the settings for Pledge.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPledgePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => KnightLevel >= 6;
    }
    #endregion

    #region Monk
    /// <summary>
    /// Modifies the settings for Phantom Kick.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomKickPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MonkLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Counter.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultCounterPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MonkLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Counterstance.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyCounterstancePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MonkLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Chakra.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultChakraPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => MonkLevel >= 5;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Bard
    /// <summary>
    /// Modifies the settings for Offensive Aria.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOffensiveAriaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BardLevel >= 1 && !Player.HasStatus(false, StatusID.HerosRime) && !Player.HasStatus(true, StatusID.HerosRime);
        setting.StatusProvide = [StatusID.OffensiveAria, StatusID.HerosRime];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Romeo's Ballad.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRomeosBalladPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BardLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Mighty March.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyMightyMarchPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BardLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Hero's Rime.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyHerosRimePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BardLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Chemist
    /// <summary>
    /// Modifies the settings for Occult Potion.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultPotionPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ChemistLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Ether.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultEtherPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ChemistLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Revive.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRevivePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ChemistLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Elixir.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultElixirPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ChemistLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region TimeMage
    /// <summary>
    /// Modifies the settings for Occult Slowga.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultSlowgaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TimeMageLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Comet.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultCometPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TimeMageLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Mage Masher.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultMageMasherPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TimeMageLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Dispel.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultDispelPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TimeMageLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Quick.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultQuickPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => TimeMageLevel >= 5;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Cannoneer
    /// <summary>
    /// Modifies the settings for Phantom Fire.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomFirePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CannoneerLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomAimPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyHolyCannonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CannoneerLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomAimPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyDarkCannonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CannoneerLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomAimPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyShockCannonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CannoneerLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for PhantomAimPvE.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifySilverCannonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => CannoneerLevel >= 6;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Oracle
    /// <summary>
    /// Modifies the settings for Predict.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPredictPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => OracleLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Recuperation.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRecuperationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => OracleLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Phantom Doom.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomDoomPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => OracleLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Phantom Rejuvenation.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomRejuvenationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => OracleLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Invulnerability.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyInvulnerabilityPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => OracleLevel >= 6;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Berserker
    /// <summary>
    /// Modifies the settings for Rage.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRagePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BerserkerLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Deadly Blow.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyDeadlyBlowPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => BerserkerLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Ranger
    /// <summary>
    /// Modifies the settings for Phantom Aim.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPhantomAimPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RangerLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Featherfoot.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultFeatherfootPvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Move;
        setting.ActionCheck = () => RangerLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Falcon.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultFalconPvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Interrupt;
        setting.ActionCheck = () => RangerLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Occult Unicorn.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultUnicornPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => RangerLevel >= 6;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Thief
    /// <summary>
    /// Modifies the settings for Occult Sprint.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyOccultSprintPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ThiefLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Steal.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyStealPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ThiefLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Vigilance.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyVigilancePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ThiefLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Trap Detection.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyTrapDetectionPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ThiefLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Pilfer Weapon.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyPilferWeaponPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => ThiefLevel >= 5;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Samurai
    /// <summary>
    /// Modifies the settings for Mineuchi.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyMineuchiPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SamuraiLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Shirahadori.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyShirahadoriPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SamuraiLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Iainuki.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyIainukiPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SamuraiLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Zeninage.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyZeninagePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SamuraiLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion

    #region Geomancer
    /// <summary>
    /// Modifies the settings for Battle Bell.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyBattleBellPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => GeomancerLevel >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Weather.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyWeatherPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => GeomancerLevel >= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Ringing Respite.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifyRingingRespitePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => GeomancerLevel >= 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    /// Modifies the settings for Suspend.
    /// </summary>
    /// <param name="setting">The action setting to modify.</param>
    static partial void ModifySuspendPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => GeomancerLevel >= 4;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
    #endregion
}