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
    /// Gets the maximum amount of ammo available.
    /// </summary>
    public static byte MaxAmmo
    {
        get
        {
            if (CartridgeChargeIiTrait.EnoughLevel)
            {
                return 3;
            }
            else if (CartridgeChargeTrait.EnoughLevel)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Gets the max combo time of the Gnashing Fang combo.
    /// </summary>
    public static short MaxTimerDuration => JobGauge.MaxTimerDuration;

    /// <summary>
    /// 
    /// </summary>
    public static bool InGnashingFang => AmmoComboStep > 0;
    #endregion

    #region PvE Actions Unassignable

    /// <summary>
    /// 
    /// </summary>
    public static bool SavageClawPvEReady => Service.GetAdjustedActionId(ActionID.GnashingFangPvE) == ActionID.SavageClawPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool WickedTalonPvEReady => Service.GetAdjustedActionId(ActionID.GnashingFangPvE) == ActionID.WickedTalonPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool JugularRipPvEReady => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.JugularRipPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool AbdomenTearPvEReady => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.AbdomenTearPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool EyeGougePvEReady => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.EyeGougePvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool HypervelocityPvEReady => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.HypervelocityPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool FatedBrandPvEReady => Service.GetAdjustedActionId(ActionID.ContinuationPvE) == ActionID.FatedBrandPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool NobleBloodPvEReady => Service.GetAdjustedActionId(ActionID.ReignOfBeastsPvE) == ActionID.NobleBloodPvE;

    /// <summary>
    /// 
    /// </summary>
    public static bool LionHeartPvEReady => Service.GetAdjustedActionId(ActionID.ReignOfBeastsPvE) == ActionID.LionHeartPvE;
    #endregion

    #region Debug Status

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text("Ammo: " + Ammo.ToString());
        ImGui.Text("AmmoComboStep: " + AmmoComboStep.ToString());
        ImGui.Text("MaxAmmo: " + MaxAmmo.ToString());
        ImGui.Text("MaxTimerDuration: " + MaxTimerDuration.ToString());
        ImGui.TextColored(ImGuiColors.DalamudViolet, "PvE Actions");
        ImGui.Text("SavageClawPvEReady: " + SavageClawPvEReady.ToString());
        ImGui.Text("WickedTalonPvEReady: " + WickedTalonPvEReady.ToString());
        ImGui.Spacing();
        ImGui.Text("JugularRipPvEReady: " + JugularRipPvEReady.ToString());
        ImGui.Text("AbdomenTearPvEReady: " + AbdomenTearPvEReady.ToString());
        ImGui.Text("EyeGougePvEReady: " + EyeGougePvEReady.ToString());
        ImGui.Text("HypervelocityPvEReady: " + HypervelocityPvEReady.ToString());
        ImGui.Spacing();
        ImGui.Text("FatedBrandPvEReady: " + FatedBrandPvEReady.ToString());
        ImGui.Spacing();
        ImGui.Text("NobleBloodPvEReady: " + NobleBloodPvEReady.ToString());
        ImGui.Text("LionHeartPvEReady: " + LionHeartPvEReady.ToString());
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
            TimeToKill = 5,
        };
    }

    static partial void ModifyBrutalShellPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyCamouflagePvE(ref ActionSetting setting)
    {
        setting.TargetType = TargetType.Self;
        setting.IsFriendly = true;
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

    }

    static partial void ModifyBurstStrikePvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.ReadyToBlast, StatusID.ReadyToBlast_3041];
        setting.ActionCheck = () => Ammo > 0;
    }

    static partial void ModifyNebulaPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = StatusHelper.RampartStatus;
        setting.TargetType = TargetType.Self;
        setting.IsFriendly = true;
    }

    static partial void ModifyDemonSlaughterPvE(ref ActionSetting setting)
    {
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
        setting.ActionCheck = () => SavageClawPvEReady;
        setting.StatusProvide = [StatusID.ReadyToTear];
    }

    static partial void ModifyWickedTalonPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => WickedTalonPvEReady;
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
        setting.IsFriendly = true;
    }

    static partial void ModifyContinuationPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 68802;
    }

    static partial void ModifyJugularRipPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => JugularRipPvEReady;
        setting.StatusNeed = [StatusID.ReadyToRip];
    }

    static partial void ModifyAbdomenTearPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => AbdomenTearPvEReady;
        setting.StatusNeed = [StatusID.ReadyToTear];
    }

    static partial void ModifyEyeGougePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => EyeGougePvEReady;
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
        setting.ActionCheck = () => HypervelocityPvEReady;
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
        setting.TargetType = TargetType.Self;
        setting.IsFriendly = true;
    }

    static partial void ModifyFatedBrandPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => FatedBrandPvEReady;
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
        setting.ActionCheck = () => NobleBloodPvEReady;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1
        };
        //setting.ComboIds = [ActionID.ReignOfBeastsPvE];
        // TODO: Having configs here breaks the rotation, investigate why
    }

    static partial void ModifyLionHeartPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => LionHeartPvEReady;
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
    #endregion

    /// <inheritdoc/>
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (SuperbolidePvE.CanUse(out act)
            && Player.GetHealthRatio() <= Service.Config.HealthForDyingTanks) return true;
        return base.EmergencyAbility(nextGCD, out act);
    }
}