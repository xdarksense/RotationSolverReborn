namespace RotationSolver.Basic.Rotations.Basic;

partial class ScholarRotation
{
    /// <inheritdoc/>
    public override MedicineType MedicineType => MedicineType.Mind;

    #region Job Gauge
    /// <summary>
    /// 
    /// </summary>
    public static byte FairyGauge => JobGauge.FairyGauge;

    /// <summary>
    /// 
    /// </summary>
    public static bool HasAetherflow => JobGauge.Aetherflow > 0;

    static float SeraphTimeRaw => JobGauge.SeraphTimer / 1000f;

    /// <summary>
    /// 
    /// </summary>
    public static float SeraphTime => SeraphTimeRaw - DataCenter.DefaultGCDRemain;

    /// <inheritdoc/>
    public override void DisplayStatus()
    {
        ImGui.Text("FairyGauge: " + FairyGauge.ToString());
        ImGui.Text("HasAetherflow: " + HasAetherflow.ToString());
        ImGui.Text("SeraphTime: " + SeraphTime.ToString());
        ImGui.Text("Has Fairy Out: " + DataCenter.HasPet.ToString());
    }
    #endregion
    private sealed protected override IBaseAction Raise => ResurrectionPvE;

    static partial void ModifyRuinPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyBioPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Bio, StatusID.BioIi, StatusID.Biolysis];
    }

    static partial void ModifyPhysickPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifySummonEosPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => !DataCenter.HasPet && !Player.HasStatus(true, StatusID.Dissipation);
    }

    static partial void ModifyResurrectionPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifyWhisperingDawnPvE_16537(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DataCenter.HasPet;
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyBioIiPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Bio, StatusID.BioIi, StatusID.Biolysis];
    }

    static partial void ModifyAdloquiumPvE(ref ActionSetting setting)
    {
        setting.StatusFromSelf = false;
        setting.TargetStatusProvide =
        [
            StatusID.EukrasianDiagnosis,
            StatusID.EukrasianPrognosis,
            StatusID.Galvanize
        ];
        setting.IsFriendly = true;
        setting.UnlockedByQuestID = 66633;
    }

    static partial void ModifySuccorPvE(ref ActionSetting setting)
    {
        setting.StatusFromSelf = false;
        setting.StatusProvide =
        [
            StatusID.EukrasianDiagnosis,
            StatusID.EukrasianPrognosis,
            StatusID.Galvanize
        ];
        setting.IsFriendly = true;
        setting.UnlockedByQuestID = 66634;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyRuinIiPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyFeyIlluminationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DataCenter.HasPet;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyAetherflowPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => InCombat && !HasAetherflow;
        setting.IsFriendly = true;
    }

    static partial void ModifyEnergyDrainPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasAetherflow;
    }

    static partial void ModifyLustratePvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasAetherflow;
        setting.UnlockedByQuestID = 66637;
    }

    static partial void ModifyArtOfWarPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifySacredSoilPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasAetherflow;
        setting.UnlockedByQuestID = 66638;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyIndomitabilityPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasAetherflow;
        setting.UnlockedByQuestID = 67208;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyBroilPvE(ref ActionSetting setting)
    {
        setting.UnlockedByQuestID = 67209;
    }

    static partial void ModifyDeploymentTacticsPvE(ref ActionSetting setting)
    {
        setting.TargetStatusNeed = [StatusID.Galvanize];
        setting.UnlockedByQuestID = 67210;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyEmergencyTacticsPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.EmergencyTactics];
        setting.UnlockedByQuestID = 67211;
        setting.IsFriendly = true;
    }

    static partial void ModifyDissipationPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Dissipation];
        setting.ActionCheck = () => !HasAetherflow && SeraphTime <= 0 && InCombat && DataCenter.HasPet;
        setting.UnlockedByQuestID = 67212;
        setting.IsFriendly = true;
    }

    static partial void ModifyExcogitationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => HasAetherflow;
    }

    static partial void ModifyBroilIiPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyChainStratagemPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => InCombat;
        setting.StatusProvide = [StatusID.ImpactImminent];
        setting.TargetStatusProvide = [StatusID.ChainStratagem];
        setting.CreateConfig = () => new ActionConfig()
        {
            TimeToKill = 10,
        };
    }

    static partial void ModifyAetherpactPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => FairyGauge >= 10 && DataCenter.HasPet && SeraphTime <= 0;
        setting.UnlockedByQuestID = 68463;
    }

    static partial void ModifyDissolveUnionPvE(ref ActionSetting setting)
    {
        setting.IsFriendly = true;
    }

    static partial void ModifyBiolysisPvE(ref ActionSetting setting)
    {
        setting.TargetStatusProvide = [StatusID.Bio, StatusID.BioIi, StatusID.Biolysis];
    }

    static partial void ModifyBroilIiiPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyRecitationPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Recitation];
        setting.IsFriendly = true;
    }

    static partial void ModifyFeyBlessingPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SeraphTime <= 0 && DataCenter.HasPet;
        setting.IsFriendly = true;
    }

    static partial void ModifySummonSeraphPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DataCenter.HasPet;
        setting.IsFriendly = true;
    }

    static partial void ModifyConsolationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => SeraphTime > 0;
        setting.IsFriendly = true;
    }

    static partial void ModifyBroilIvPvE(ref ActionSetting setting)
    {

    }

    static partial void ModifyArtOfWarIiPvE(ref ActionSetting setting)
    {
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 3,
        };
    }

    static partial void ModifyProtractionPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Protraction];
    }

    static partial void ModifyExpedientPvE(ref ActionSetting setting)
    {
        setting.StatusProvide = [StatusID.Expedience, StatusID.DesperateMeasures];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyBanefulImpactionPvE(ref ActionSetting setting)
    {
        setting.StatusNeed = [StatusID.ImpactImminent];
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifyConcitationPvE(ref ActionSetting setting)
    {
        setting.StatusProvide =
        [
            StatusID.EukrasianDiagnosis,
            StatusID.EukrasianPrognosis,
            StatusID.Galvanize
        ];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }

    static partial void ModifySeraphismPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => DataCenter.HasPet && InCombat;
        setting.IsFriendly = true;
    }

    static partial void ModifyManifestationPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.Seraphism);
        setting.TargetStatusProvide =
        [
            StatusID.EukrasianDiagnosis,
            StatusID.EukrasianPrognosis,
            StatusID.Galvanize
        ];
        setting.IsFriendly = true;
    }

    static partial void ModifyAccessionPvE(ref ActionSetting setting)
    {
        setting.ActionCheck = () => Player.HasStatus(true, StatusID.Seraphism);
        setting.StatusProvide =
        [
            StatusID.EukrasianDiagnosis,
            StatusID.EukrasianPrognosis,
            StatusID.Galvanize
        ];
        setting.IsFriendly = true;
        setting.CreateConfig = () => new ActionConfig()
        {
            AoeCount = 1,
        };
    }
}
