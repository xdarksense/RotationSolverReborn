namespace RotationSolver.Basic.Rotations.Basic;

partial class SamuraiRotation
{
    #region JobGauge
    /// <summary>
    /// 
    /// </summary>
    public static bool HasSetsu => JobGauge.HasSetsu;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasGetsu => JobGauge.HasGetsu;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasKa => JobGauge.HasKa;

    /// <summary>
    /// 
    /// </summary>
    public static byte Kenki => JobGauge.Kenki;

    /// <summary>
    /// 
    /// </summary>
    public static byte MeditationStacks => JobGauge.MeditationStacks;

    /// <summary>
    /// 
    /// </summary>
    public static Kaeshi Kaeshi => JobGauge.Kaeshi;

    /// <summary>
    /// 
    /// </summary>
    public static byte SenCount => (byte)((HasGetsu ? 1 : 0) + (HasSetsu ? 1 : 0) + (HasKa ? 1 : 0));
    #endregion

    #region Status Tracking

    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Strength;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasMoon => Player.HasStatus(true, StatusID.Fugetsu);

    /// <summary>
    /// 
    /// </summary>
    public static bool HasFlower => Player.HasStatus(true, StatusID.Fuka);

    /// <summary>
    /// 
    /// </summary>
    public static bool IsMoonTimeLessThanFlower
        => Player.StatusTime(true, StatusID.Fugetsu) < Player.StatusTime(true, StatusID.Fuka);

    #endregion

    #region PvE Actions

    static partial void ModifyHakazePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 95;
    }

    static partial void ModifyJinpuPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 95;
        setting.ComboIds = [ActionID.HakazePvE, ActionID.GyofuPvE];
    }

    static partial void ModifyEnpiPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MeleeRange;
    }

    static partial void ModifyThirdEyePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ThirdEye];
    }

    static partial void ModifyShifuPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 95;
        setting.ComboIds = [ActionID.HakazePvE, ActionID.GyofuPvE];
    }

    static partial void ModifyFugaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 95;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyGekkoPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.JinpuPvE];
        setting.ActionCheck = () => Kenki <= 90;
    }

    static partial void ModifyIaijutsuPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyMangetsuPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.FukoPvE];
        setting.ActionCheck = () => Kenki <= 90;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyKashaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 90;
        setting.ComboIds = [ActionID.ShifuPvE];
    }

    static partial void ModifyOkaPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.FukoPvE];
        setting.ActionCheck = () => Kenki <= 90;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyYukikazePvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.HakazePvE, ActionID.GyofuPvE];
        setting.ActionCheck = () => Kenki <= 85;
    }

    static partial void ModifyMeikyoShisuiPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.MeikyoShisui, StatusID.Tendo];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 0,
        };
    }

    static partial void ModifyHissatsuShintenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki >= 25;
    }

    static partial void ModifyHissatsuGyotenPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
        setting.ActionCheck = () => Kenki >= 10;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyHissatsuYatenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki >= 10;
    }

    static partial void ModifyMeditatePvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 68101;
        setting.ActionCheck = () => !IsMoving;
    }

    static partial void ModifyHissatsuKyutenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki >= 25;
    }

    static partial void ModifyHagakurePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => (SenCount == 1 && Kenki <= 90) || (SenCount == 2 && Kenki <= 80) || (SenCount == 3 && Kenki <= 70);
    }

    static partial void ModifyIkishotenPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.OgiNamikiriReady, StatusID.ZanshinReady];
        setting.ActionCheck = () => InCombat && Kenki <= 50;
    }

    static partial void ModifyHissatsuGurenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki >= 25;
        setting.UnlockedByQuestID = 68106;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHissatsuSeneiPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki >= 25;
    }

    static partial void ModifyTsubamegaeshiPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.GnashingFangPvE) == ActionID.TsubamegaeshiPvE;
    }

    static partial void ModifyShohaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => InCombat && MeditationStacks == 3;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyTengentsuPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Tengentsu];
    }

    static partial void ModifyFukoPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 90;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyOgiNamikiriPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.OgiNamikiriReady];
        setting.ActionCheck = () => MeditationStacks <= 2;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyKaeshiNamikiriPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kaeshi == Kaeshi.NAMIKIRI;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyGyofuPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kenki <= 95;
    }

    static partial void ModifyZanshinPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ZanshinReady];
        setting.ActionCheck = () => Kenki >= 50;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    //Iaijutsu 

    static partial void ModifyHiganbanaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SenCount == 1 && MeditationStacks <= 2;
        setting.TargetStatusProvide = [StatusID.Higanbana];
    }

    static partial void ModifyTenkaGokenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SenCount == 2 && MeditationStacks <= 2;
        setting.IsFriendly = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyMidareSetsugekkaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SenCount == 3 && MeditationStacks <= 2 && !Player.HasStatus(true, StatusID.Tendo);
    }

    static partial void ModifyKaeshiGokenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Kaeshi == Kaeshi.GOKEN;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyKaeshiSetsugekkaPvE(ref ActionSetting setting)
    {
        //setting.ActionCheck = () => Kaeshi == Kaeshi.SETSUGEKKA;
        setting.StatusNeed = [StatusID.Tsubamegaeshi];
    }

    static partial void ModifyTendoGokenPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SenCount == 2 && MeditationStacks <= 2;
        setting.StatusProvide = [StatusID.Tsubamegaeshi];
        setting.StatusNeed = [StatusID.Tendo];
        setting.IsFriendly = false;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyTendoSetsugekkaPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SenCount == 3 && MeditationStacks <= 2 && Player.HasStatus(true, StatusID.Tendo);
        setting.StatusProvide = [StatusID.Tsubamegaeshi];
        setting.StatusNeed = [StatusID.Tendo];
    }

    static partial void ModifyTendoKaeshiGokenPvE(ref ActionSetting setting)
    {
        //setting.ActionCheck = () => (byte)Kaeshi == 5;
        setting.IsFriendly = false;
        setting.StatusNeed = [StatusID.Tendo];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyTendoKaeshiSetsugekkaPvE(ref ActionSetting setting)
    {
        //setting.ActionCheck = () => (byte)Kaeshi == 6; // Temporary until Dalamud enums are updated
        setting.StatusNeed = [StatusID.Tsubamegaeshi_4218];
    }

    #endregion

    #region PvP Actions
    // PvP
    static partial void ModifyHissatsuSotenPvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }
    #endregion
}
