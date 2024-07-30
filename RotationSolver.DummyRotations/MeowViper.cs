//using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
//using ImGuiNET;
//using RotationSolver.Basic.Actions;
//using RotationSolver.Basic.Attributes;
//using RotationSolver.Basic.Data;
//using RotationSolver.Basic.Helpers;
//using RotationSolver.Basic.Rotations;
//using RotationSolver.Basic.Rotations.Basic;

//namespace DefaultRotations.Melee;

//[Rotation("MeowViper", CombatType.PvE, GameVersion = "7.01")]
//[SourceCode(Path = "main/DefaultRotations/Melee/VPR_Testing.cs")]
//[Api(3)]
//public sealed class VPR_Testing : ViperRotation
//{
//    #region Emergency Logic
//    // Determines emergency actions to take based on the next planned GCD action.
//    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
//    {
//        if (OuroborosPvE.CanUse(out act)) return true;
//        if (FourthGenerationPvE.CanUse(out act)) return true;
//        if (ThirdGenerationPvE.CanUse(out act)) return true;
//        if (SecondGenerationPvE.CanUse(out act)) return true;
//        if (FirstGenerationPvE.CanUse(out act)) return true;
//        if (TwinfangBitePvE.CanUse(out act) && HasHunterVenom) return true;
//        if (TwinbloodBitePvE.CanUse(out act) && HasSwiftVenom) return true;
//        if (UncoiledTwinfangPvE.CanUse(out act) && HasPoisedFang) return true;
//        if (UncoiledTwinbloodPvE.CanUse(out act) && HasPoisedBlood) return true;

//        return base.EmergencyAbility(nextGCD, out act);
//    }
//    #endregion

//    #region oGCD Logic
//    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
//    {
//        if (SerpentsIrePvE.CanUse(out act)) return true;
//        if (TwinfangThreshPvE.CanUse(out act) && HasFellHuntersVenom) return true;
//        if (TwinbloodThreshPvE.CanUse(out act) && HasFellskinsVenom) return true;

//        if (LastLashPvE.CanUse(out act)) return true;
//        if (DeathRattlePvE.CanUse(out act)) return true;
//        return base.AttackAbility(nextGCD, out act);
//    }

//    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
//    {
//        if (SlitherPvE.CanUse(out act)) return true;
//        return base.MoveForwardAbility(nextGCD, out act);
//    }
//    #endregion

//    #region GCD Logic

//    protected override bool GeneralGCD(out IAction? act)
//    {
//        ////Reawaken Combo
//        if (OuroborosPvE.CanUse(out act)) return true;
//        if (FourthGenerationPvE.CanUse(out act)) return true;
//        if (ThirdGenerationPvE.CanUse(out act)) return true;
//        if (SecondGenerationPvE.CanUse(out act)) return true;
//        if (FirstGenerationPvE.CanUse(out act)) return true;

//        //Overcap protection
//        //if ((DreadwinderPvE.Cooldown.CurrentCharges > 0 || !SerpentsIrePvE.Cooldown.IsCoolingDown) &&
//        //    ((RattlingCoilStacks is 3 && EnhancedVipersRattleTrait.EnoughLevel) ||
//        //    (RattlingCoilStacks is 2 && !EnhancedVipersRattleTrait.EnoughLevel)))
//        //{
//        //    if (UncoiledFuryPvE.CanUse(out act)) return true;
//        //}

//        if (HuntersDenPvE.CanUse(out act, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, skipStatusProvideCheck: true)) return true;
//        if (SwiftskinsDenPvE.CanUse(out act, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, skipStatusProvideCheck: true)) return true;
//        if (HuntersCoilPvE.CanUse(out act, skipComboCheck: true)) return true;
//        if (SwiftskinsCoilPvE.CanUse(out act, skipComboCheck: true)) return true;

//        //Reawakend Usage
//        //if (SerpentsIrePvE.Cooldown.RecastTimeRemainOneCharge > (100 - SerpentOffering)
//        //    && (DreadwinderPvE.Cooldown.CurrentCharges == 0
//        //    || (DreadwinderPvE.Cooldown.CurrentCharges == 1
//        //    && DreadwinderPvE.Cooldown.RecastTimeRemainOneCharge > 10)) &&
//        //    SwiftTime > 10 &&
//        //    HuntersTime > 10 &&
//        //    !HasHunterVenom && !HasSwiftVenom &&
//        //    !HasPoisedBlood && !HasPoisedFang)
//        //{
//        //    if (ReawakenPvE.CanUse(out act, skipComboCheck: true, skipCastingCheck: true, skipAoeCheck: true, skipStatusProvideCheck: true)) return true;
//        //}

//        //if (Vicepit.CanUse(out act, usedUp: true)) return true;
//        //if (Vicewinder.CanUse(out act, usedUp: true)) return true;


//        //if (HasGrimSkin)
//        //{
//        //    if (DreadMawPvE.CanUse(out act)) return true;
//        //}
//        //if (SteelMawPvE.CanUse(out act)) return true;

//        //if (HunterLessThanSwift)
//        //{
//        //    if (SteelMawPvE.CanUse(out act)) return true;
//        //}
//        //if (DreadMawPvE.CanUse(out act)) return true;

//        //if (SteelMawPvE.CanUse(out act)) return true;
//        //if (HasBane)
//        //{
//        //    if (DreadFangsPvE.CanUse(out act)) return true;
//        //}
//        //if (SteelFangsPvE.CanUse(out act)) return true;
//        //if (HasFlank)
//        //{
//        //    if (SteelFangsPvE.CanUse(out act)) return true;
//        //}
//        //if (DreadFangsPvE.CanUse(out act)) return true;

//        //if (SteelFangsPvE.CanUse(out act)) return true;

//        //if (!Player.HasStatus(true, StatusID.ReadyToReawaken) && !HasHunterVenom && !HasSwiftVenom && IsHunter && IsSwift)
//        //{
//        //    if (UncoiledFuryPvE.CanUse(out act)) return true;
//        //}

//        //if (WrithingSnapPvE.CanUse(out act)) return true;

//        //return base.GeneralGCD(out act);
//    }
//    #endregion
//}