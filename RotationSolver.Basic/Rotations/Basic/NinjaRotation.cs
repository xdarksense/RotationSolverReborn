namespace RotationSolver.Basic.Rotations.Basic;

partial class NinjaRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Dexterity;

    #region Job Gauge
    /// <summary>
    /// Gets the amount of Ninki available.
    /// </summary>
    public static byte Ninki => (byte)JobGauge.Ninki;

    /// <summary>
    /// Gets the current charges for Kazematoi.
    /// </summary>
    public static byte Kazematoi => (byte)JobGauge.Kazematoi;

    /// <summary>
    /// Is enough level for Jin
    /// </summary>
    public static bool HasJin => IncreaseAttackSpeedTrait.EnoughLevel;

    /// <summary>
    /// Determines if Trick Attack is in its effective period.
    /// </summary>
    public bool InTrickAttack => (KunaisBanePvE.Cooldown.IsCoolingDown || TrickAttackPvE.Cooldown.IsCoolingDown) && (!KunaisBanePvE.Cooldown.ElapsedAfter(17) || !TrickAttackPvE.Cooldown.ElapsedAfter(17));
    
    /// <summary>
    /// Determines if Mug is in its effective period.
    /// </summary>
    public bool InMug => MugPvE.Cooldown.IsCoolingDown && !MugPvE.Cooldown.ElapsedAfter(19);
    
    /// <summary>
    /// Checks if no ninjutsu action is currently selected or if the Rabbit Medium has been invoked.
    /// </summary>
    public static bool NoNinjutsu => AdjustId(ActionID.NinjutsuPvE) is ActionID.NinjutsuPvE or ActionID.RabbitMediumPvE;

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text($"Ninki: {Ninki}");
        ImGui.Text($"Kazematoi: {Kazematoi}");
        ImGui.Text($"HasJin: {HasJin}");
        ImGui.Text($"InTrickAttack: {InTrickAttack}");
        ImGui.Text($"InMug: {InMug}");
        ImGui.Text($"NoNinjutsu: {NoNinjutsu}");
    }
    #endregion

    static partial void ModifySpinningEdgePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyShadeShiftPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ShadeShift];
    }

    static partial void ModifyGustSlashPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.SpinningEdgePvE];
    }

    static partial void ModifyHidePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Hidden];
        setting.ActionCheck = () => !InCombat;
    }

    static partial void ModifyThrowingDaggerPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 65680;
        setting.SpecialType = SpecialActionType.MeleeRange;
    }

    static partial void ModifyMugPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 65681;
        setting.ActionCheck = () => IsLongerThan(10);
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyTrickAttackPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ShadowWalker, StatusID.Hidden];
        setting.TargetStatusProvide = [StatusID.TrickAttack_3254];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyAeolianEdgePvP(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.GustSlashPvE];
    }

    static partial void ModifyTenPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 65748;
    }

    static partial void ModifyNinjutsuPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyChiPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 65750;
    }

    static partial void ModifyDeathBlossomPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyAssassinatePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyShukuchiPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
        setting.UnlockedByQuestID = 65752;
    }

    static partial void ModifyJinPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 65768;
    }

    static partial void ModifyKassatsuPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Kassatsu];
        setting.ActionCheck = () => !Player.HasStatus(true, StatusID.TenChiJin);
        setting.UnlockedByQuestID = 65770;
    }

    static partial void ModifyHakkeMujinsatsuPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.DeathBlossomPvE];
        setting.UnlockedByQuestID = 67220;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyArmorCrushPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67221;
        setting.ComboIds = [ActionID.GustSlashPvE];
        setting.ActionCheck = () => Kazematoi <= 4;
    }

    static partial void ModifyDreamWithinADreamPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67222;
    }

    static partial void ModifyHellfrogMediumPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ninki >= 50;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyDokumoriPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Higi];
        setting.TargetStatusProvide = [StatusID.Dokumori];
        setting.ActionCheck = () => Ninki <= 60 && IsLongerThan(10);
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyBhavacakraPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ninki >= 50;
    }

    static partial void ModifyTenChiJinPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.TenChiJin, StatusID.TenriJindoReady];
        setting.UnlockedByQuestID = 68488;
        setting.ActionCheck = () => !IsMoving;
    }

    static partial void ModifyMeisuiPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ShadowWalker];
        setting.StatusProvide = [StatusID.Meisui];
        setting.ActionCheck = () => !Player.HasStatus(true, StatusID.Kassatsu) && InCombat;
    }

    static partial void ModifyBunshinPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ninki >= 50;
        setting.StatusProvide = [StatusID.Bunshin, StatusID.PhantomKamaitachiReady];
    }

    static partial void ModifyPhantomKamaitachiPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.PhantomKamaitachiReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHollowNozuchiPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Doton];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyForkedRaijuPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.RaijuReady];
    }

    static partial void ModifyFleetingRaijuPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.RaijuReady];
    }

    static partial void ModifyKunaisBanePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.Hidden) || Player.HasStatus(true, StatusID.ShadowWalker);
        setting.TargetStatusProvide = [StatusID.KunaisBane];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyDeathfrogMediumPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ninki <= 50;
        setting.StatusNeed = [StatusID.Higi];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyZeshoMeppoPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ninki <= 50;
        setting.StatusNeed = [StatusID.Higi];
    }

    static partial void ModifyTenriJindoPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.TenriJindoReady];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    /// <summary>
    ///  
    /// </summary>
    public NinjaRotation()
    {
        FumaShurikenPvE.Setting.Ninjutsu = [TenPvE];
        KatonPvE.Setting.Ninjutsu = [ChiPvE, TenPvE];
        RaitonPvE.Setting.Ninjutsu = [TenPvE, ChiPvE];
        HyotonPvE.Setting.Ninjutsu = [TenPvE, JinPvE];
        HutonPvE.Setting.Ninjutsu = [JinPvE, ChiPvE, TenPvE];
        DotonPvE.Setting.Ninjutsu = [JinPvE, TenPvE, ChiPvE];
        SuitonPvE.Setting.Ninjutsu = [TenPvE, ChiPvE, JinPvE];
        GokaMekkyakuPvE.Setting.Ninjutsu = [ChiPvE, TenPvE];
        HyoshoRanryuPvE.Setting.Ninjutsu = [TenPvE, JinPvE];
    }

    static partial void ModifyFumaShurikenPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyKatonPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyRaitonPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.RaijuReady];
    }

    static partial void ModifyHyotonPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Blind];
    }

    static partial void ModifyHutonPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ShadowWalker];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyDotonPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Doton];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifySuitonPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ShadowWalker];
    }

    static partial void ModifyGokaMekkyakuPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Kassatsu];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyHyoshoRanryuPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.Kassatsu];
    }

    // PvP
    static partial void ModifyShukuchiPvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.ShukuchiPvE)]
    protected sealed override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (ShukuchiPvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.FeintPvE)]
    protected sealed override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (FeintPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.ShadeShiftPvE)]
    protected override bool DefenseSingleAbility(IAction nextGCD, out IAction? act)
    {
        if (ShadeShiftPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(nextGCD, out act);
    }
}