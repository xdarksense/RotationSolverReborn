using Dalamud.Interface.Colors;

namespace RotationSolver.Basic.Rotations.Basic;

partial class GunbreakerRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Strength;


    /// <summary>
    /// 
    /// </summary>
    public override bool CanHealSingleSpell => false;

    /// <summary>
    /// 
    /// </summary>
    public override bool CanHealAreaSpell => false;

    #region Job Gauge
    /// <summary>
    /// Gets the amount of ammo available.
    /// </summary>
    public static byte Ammo => JobGauge.Ammo;

    /// <summary>
    /// 
    /// </summary>
    public static byte AmmoComboStep => JobGauge.AmmoComboStep;

    /// <summary>
    /// 
    /// </summary>
    public static byte MaxAmmo => CartridgeChargeIiTrait.EnoughLevel ? (byte)3 : (byte)2;

    /// <summary>
    /// Gets the max combo time of the Gnashing Fang combo.
    /// </summary>
    public static short MaxTimerDuration => JobGauge.MaxTimerDuration;
    #endregion

    #region Debug Status

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text("Ammo: " + Ammo.ToString());
        ImGui.Text("AmmoComboStep: " + AmmoComboStep.ToString());
        ImGui.Text("MaxAmmo: " + MaxAmmo.ToString());
        ImGui.Text("MaxTimerDuration: " + MaxTimerDuration.ToString());
        ImGui.TextColored(ImGuiColors.DalamudYellow, "PvP Actions");
        ImGui.Text("SavageClawPvPReady: " + SavageClawPvPReady.ToString());
        ImGui.Text("WickedTalonPvPReady: " + WickedTalonPvPReady.ToString());
        ImGui.Spacing();
        ImGui.Text("HypervelocityPvPReady: " + HypervelocityPvPReady.ToString());
        ImGui.Text("FatedBrandPvPReady: " + FatedBrandPvPReady.ToString());
        ImGui.Text("JugularRipPvPReady: " + JugularRipPvPReady.ToString());
        ImGui.Text("AbdomenTearPvPReady: " + AbdomenTearPvPReady.ToString());
        ImGui.Text("EyeGougePvPReady: " + EyeGougePvPReady.ToString());
        ImGui.Spacing();
        ImGui.Text("HasTerminalTrigger: " + HasTerminalTrigger.ToString());
    }
    #endregion

    #region PvE Actions

    static partial void ModifyKeenEdgePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyNoMercyPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ReadyToBreak];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyBrutalShellPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.KeenEdgePvE];
    }

    static partial void ModifyCamouflagePvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Self;
    }

    static partial void ModifyDemonSlicePvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    private protected sealed override IBaseAction TankStance => RoyalGuardPvE;

    static partial void ModifyLightningShotPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MeleeRange;
    }

    static partial void ModifyDangerZonePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifySolidBarrelPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.BrutalShellPvE];
    }

    static partial void ModifyBurstStrikePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ReadyToBlast, StatusID.ReadyToBlast_3041];
        setting.ActionCheck = () => Ammo > 0;
    }

    static partial void ModifyNebulaPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.RampartStatus;
        setting.ActionCheck = () => Player.IsTargetOnSelf();
    }

    static partial void ModifyDemonSlaughterPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.DemonSlicePvE];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyAuroraPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Aurora];
    }

    static partial void ModifySuperbolidePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Superbolide];
        setting.ActionCheck = () => InCombat;
        setting.TargetType = TargetType.Self;
    }

    static partial void ModifySonicBreakPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ReadyToBreak];
        setting.TargetStatusProvide = [StatusID.SonicBreak];
    }

    static partial void ModifyTrajectoryPvE(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyGnashingFangPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => AmmoComboStep == 0 && Ammo > 0;
        setting.StatusProvide = [StatusID.ReadyToRip];
    }

    static partial void ModifySavageClawPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.GnashingFangPvE) == ActionID.SavageClawPvE;
        setting.ComboIds = [ActionID.GnashingFangPvE];
        setting.StatusProvide = [StatusID.ReadyToTear];
    }

    static partial void ModifyWickedTalonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.GnashingFangPvE) == ActionID.WickedTalonPvE;
        setting.ComboIds = [ActionID.SavageClawPvE];
        setting.StatusProvide = [StatusID.ReadyToGouge];
    }

    static partial void ModifyBowShockPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.BowShock];
    }

    static partial void ModifyHeartOfLightPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.HeartOfLight];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyHeartOfStonePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.HeartOfStone];
        setting.ActionCheck = () => Player.IsParty() || Player.IsTargetOnSelf();
    }

    static partial void ModifyContinuationPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 68802;
    }

    static partial void ModifyJugularRipPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.JugularRipPvE;
        setting.ComboIds = [ActionID.GnashingFangPvE];
        setting.StatusNeed = [StatusID.ReadyToRip];
    }

    static partial void ModifyAbdomenTearPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.AbdomenTearPvE;
        setting.ComboIds = [ActionID.SavageClawPvE];
        setting.StatusNeed = [StatusID.ReadyToTear];
    }

    static partial void ModifyEyeGougePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.EyeGougePvE;
        setting.ComboIds = [ActionID.WickedTalonPvE];
        setting.StatusNeed = [StatusID.ReadyToGouge];
    }

    static partial void ModifyFatedCirclePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ReadyToRaze];
        setting.ActionCheck = () => Ammo > 0;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyBloodfestPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ReadyToReign];
        setting.ActionCheck = () => MaxAmmo - Ammo > 1;
    }

    static partial void ModifyBlastingZonePvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyHeartOfCorundumPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.CatharsisOfCorundum, StatusID.ClarityOfCorundum];
        setting.ActionCheck = () => Player.IsParty() || Player.IsTargetOnSelf();
    }

    static partial void ModifyHypervelocityPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.HypervelocityPvE;
        setting.ComboIds = [ActionID.BurstStrikePvE];
        setting.StatusNeed = [StatusID.ReadyToBlast];
    }

    static partial void ModifyDoubleDownPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Ammo >= 1;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyGreatNebulaPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.RampartStatus;
        setting.ActionCheck = () => Player.IsTargetOnSelf();
    }

    static partial void ModifyFatedBrandPvE(ref ActionSetting setting)
    {
        setting.ComboIds = [ActionID.FatedCirclePvE];
        setting.StatusNeed = [StatusID.ReadyToRaze];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 2,
        };
    }

    static partial void ModifyReignOfBeastsPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ReadyToReign];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1
        };
    }

    static partial void ModifyNobleBloodPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1
        };
        //setting.ComboIds = [ActionID.ReignOfBeastsPvE];
        // TODO: Having configs here breaks the rotation, investigate why
    }

    static partial void ModifyLionHeartPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1
        };
        //setting.ComboIds = [ActionID.NobleBloodPvE];
    }

    #endregion

    #region PvP Actions Unassignable

    /// <summary>
    /// Gnashing Fang 2
    /// </summary>
    public static bool SavageClawPvPReady => Service.GetAdjustedActionId(ActionID.GnashingFangPvP) == ActionID.SavageClawPvP;

    /// <summary>
    /// Gnashing Fang 3
    /// </summary>
    public static bool WickedTalonPvPReady => Service.GetAdjustedActionId(ActionID.GnashingFangPvP) == ActionID.WickedTalonPvP;

    /// <summary>
    /// 
    /// </summary>
    public static bool HypervelocityPvPReady => Service.GetAdjustedActionId(ActionID.ContinuationPvP) == ActionID.HypervelocityPvP;

    /// <summary>
    /// 
    /// </summary>
    public static bool FatedBrandPvPReady => Service.GetAdjustedActionId(ActionID.ContinuationPvP) == ActionID.FatedBrandPvP;

    /// <summary>
    /// 
    /// </summary>
    public static bool JugularRipPvPReady => Service.GetAdjustedActionId(ActionID.ContinuationPvP) == ActionID.JugularRipPvP;

    /// <summary>
    /// 
    /// </summary>
    public static bool AbdomenTearPvPReady => Service.GetAdjustedActionId(ActionID.ContinuationPvP) == ActionID.AbdomenTearPvP;

    /// <summary>
    /// 
    /// </summary>
    public static bool EyeGougePvPReady => Service.GetAdjustedActionId(ActionID.ContinuationPvP) == ActionID.EyeGougePvP;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public static bool HasTerminalTrigger => !Player.WillStatusEndGCD(0, 0, true, StatusID.RelentlessRush);

    #region PvP Actions

    static partial void ModifyGnashingFangPvP(ref ActionSetting setting)
    {

    }

    static partial void ModifyFatedCirclePvP(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyContinuationPvP(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyRoughDividePvP(ref ActionSetting setting)
    {
        setting.SpecialType = SpecialActionType.MovingForward;
    }

    static partial void ModifyBlastingZonePvP(ref ActionSetting setting)
    {

    }

    static partial void ModifyHeartOfCorundumPvP(ref ActionSetting setting)
    {

    }

    static partial void ModifySavageClawPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SavageClawPvPReady;
    }

    static partial void ModifyWickedTalonPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => WickedTalonPvPReady;
    }

    static partial void ModifyHypervelocityPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HypervelocityPvPReady;
    }

    static partial void ModifyFatedBrandPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => FatedBrandPvPReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyJugularRipPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => JugularRipPvPReady;
    }

    static partial void ModifyAbdomenTearPvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => AbdomenTearPvPReady;
    }

    static partial void ModifyEyeGougePvP(ref ActionSetting setting)
    {
        setting.ActionCheck = () => EyeGougePvPReady;
    }

    /// <inheritdoc/>
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (SuperbolidePvE.CanUse(out act)
            && Player.GetHealthRatio() <= Service.Config.HealthForDyingTanks) return true;
        return base.EmergencyAbility(nextGCD, out act);
    }

    /// <inheritdoc/>
    [RotationDesc(ActionID.TrajectoryPvE)]
    protected sealed override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (TrajectoryPvE.CanUse(out act)) return true;
        return false;
    }

    #endregion
}
