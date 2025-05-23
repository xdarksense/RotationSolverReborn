namespace RebornRotations.PVPRotations.Healer;

[Rotation("Default PVP", CombatType.PvP, GameVersion = "7.21")]
[SourceCode(Path = "main/RebornRotations/PVPRotations/Healer/SGE_Default.PVP.cs")]
[Api(4)]
public class SGE_DefaultPVP : SageRotation
{
    #region Configurations

    [RotationConfig(CombatType.PvP, Name = "Use Purify")]
    public bool UsePurifyPvP { get; set; } = true;

    [RotationConfig(CombatType.PvP, Name = "Stop attacking while in Guard.")]
    public bool RespectGuard { get; set; } = true;
    #endregion

    #region Standard PVP Utilities
    private bool DoPurify(out IAction? action)
    {
        action = null;
        if (!UsePurifyPvP)
        {
            return false;
        }

        List<int> purifiableStatusesIDs = new()
        {
            // Stun, DeepFreeze, HalfAsleep, Sleep, Bind, Heavy, Silence
            1343, 3219, 3022, 1348, 1345, 1344, 1347
        };

        return purifiableStatusesIDs.Any(id => Player.HasStatus(false, (StatusID)id)) && PurifyPvP.CanUse(out action);
    }
    #endregion

    #region oGCDs
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? action)
    {
        action = null;
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return false;
        }

        if (DoPurify(out action))
        {
            return true;
        }

        return !Player.HasStatus(true, StatusID.Kardia_2871) && KardiaPvP.CanUse(out action) || base.EmergencyAbility(nextGCD, out action);
    }

    protected override bool AttackAbility(IAction nextGCD, out IAction? action)
    {
        action = null;
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return false;
        }

        if (DiabrosisPvP.CanUse(out action))
        {
            return true;
        }

        return !Target.HasStatus(true, StatusID.Toxikon) && ToxikonPvP.CanUse(out action, usedUp: true) || base.AttackAbility(nextGCD, out action);
    }
    #endregion

    #region GCDs
    protected override bool DefenseSingleGCD(out IAction? action)
    {
        action = null;
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return false;
        }

        return StoneskinIiPvP.CanUse(out action) || base.DefenseSingleGCD(out action);
    }

    protected override bool HealSingleGCD(out IAction? action)
    {
        action = null;
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return false;
        }

        return HaelanPvP.CanUse(out action) || base.HealSingleGCD(out action);
    }

    protected override bool GeneralGCD(out IAction? action)
    {
        action = null;
        if (RespectGuard && Player.HasStatus(true, StatusID.Guard))
        {
            return false;
        }

        if (PneumaPvP.CanUse(out action))
        {
            return true;
        }

        if (PsychePvP.CanUse(out action))
        {
            return true;
        }

        if (PhlegmaIiiPvP.CanUse(out action))
        {
            return true;
        }

        if (!IsLastAction(ActionID.EukrasiaPvP) && InCombat && !Target.HasStatus(true, StatusID.EukrasianDosisIii_3108) && !Player.HasStatus(true, StatusID.Eukrasia) && EukrasiaPvP.CanUse(out action, usedUp: true))
        {
            return true;
        }

        return DosisIiiPvP.CanUse(out action) || base.GeneralGCD(out action);
    }
    #endregion
}