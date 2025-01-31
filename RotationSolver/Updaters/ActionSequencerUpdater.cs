using RotationSolver.Basic.Configuration.Conditions;

namespace RotationSolver.Updaters;

internal class ActionSequencerUpdater
{
    static string? _actionSequencerFolder;

    public static void UpdateActionSequencerAction()
    {
        if (DataCenter.ConditionSets == null) return;
        var customRotation = DataCenter.CurrentRotation;
        if (customRotation == null) return;

        var allActions = RotationUpdater.RightRotationActions;

        var set = DataCenter.RightSet;
        if (set == null) return;

        var disabledActions = new HashSet<uint>();
        foreach (var pair in set.DisableConditionDict)
        {
            if (pair.Value.IsTrue(customRotation))
            {
                disabledActions.Add(pair.Key);
            }
        }
        DataCenter.DisabledActionSequencer = disabledActions;

        var conditions = set.ConditionDict;
        if (conditions != null)
        {
            foreach (var conditionPair in conditions)
            {
                var nextAct = allActions.FirstOrDefault(a => a.ID == conditionPair.Key);
                if (nextAct == null || !conditionPair.Value.IsTrue(customRotation)) continue;

                DataCenter.ActionSequencerAction = nextAct;
                return;
            }
        }

        DataCenter.ActionSequencerAction = null;
    }

    public static void Enable(string folder)
    {
        _actionSequencerFolder = folder;
        if (!Directory.Exists(_actionSequencerFolder)) Directory.CreateDirectory(_actionSequencerFolder);

        LoadFiles();
    }

    public static void SaveFiles()
    {
        if (_actionSequencerFolder == null) return;
        try
        {
            Directory.Delete(_actionSequencerFolder, true);
            Directory.CreateDirectory(_actionSequencerFolder);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error deleting directory: {ex.Message}");
        }

        foreach (var set in DataCenter.ConditionSets)
        {
            set.Save(_actionSequencerFolder);
        }
    }

    public static void LoadFiles()
    {
        if (_actionSequencerFolder == null) return;

        DataCenter.ConditionSets = MajorConditionSet.Read(_actionSequencerFolder);
    }

    public static void AddNew()
    {
        bool hasUnnamed = false;
        foreach (var conditionSet in DataCenter.ConditionSets)
        {
            if (conditionSet.IsUnnamed)
            {
                hasUnnamed = true;
                break;
            }
        }

        if (!hasUnnamed)
        {
            var newConditionSets = new List<MajorConditionSet>(DataCenter.ConditionSets)
            {
                new MajorConditionSet()
            };
            DataCenter.ConditionSets = newConditionSets.ToArray();
        }
    }

    public static void Delete(string name)
    {
        var newConditionSets = new List<MajorConditionSet>();
        foreach (var conditionSet in DataCenter.ConditionSets)
        {
            if (conditionSet.Name != name)
            {
                newConditionSets.Add(conditionSet);
            }
        }
        DataCenter.ConditionSets = newConditionSets.ToArray();

        var filePath = Path.Combine(_actionSequencerFolder ?? string.Empty, $"{name}.json");
        File.Delete(filePath);
    }
}