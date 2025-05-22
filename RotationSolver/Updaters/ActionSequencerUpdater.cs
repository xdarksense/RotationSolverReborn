using RotationSolver.Basic.Configuration.Conditions;

namespace RotationSolver.Updaters;

internal class ActionSequencerUpdater
{
    private static string? _actionSequencerFolder;

    public static void UpdateActionSequencerAction()
    {
        if (DataCenter.ConditionSets == null)
        {
            return;
        }

        ICustomRotation? customRotation = DataCenter.CurrentRotation;
        if (customRotation == null)
        {
            return;
        }

        IAction[] allActions = RotationUpdater.CurrentRotationActions;

        MajorConditionValue set = DataCenter.CurrentConditionValue;
        if (set == null)
        {
            return;
        }

        HashSet<uint> disabledActions = [];
        foreach (KeyValuePair<uint, ConditionSet> pair in set.DisableConditionDict)
        {
            if (pair.Value.IsTrue(customRotation))
            {
                _ = disabledActions.Add(pair.Key);
            }
        }
        DataCenter.DisabledActionSequencer = disabledActions;

        Dictionary<uint, ConditionSet> conditions = set.ConditionDict;
        if (conditions != null)
        {
            foreach (KeyValuePair<uint, ConditionSet> conditionPair in conditions)
            {
                object? nextAct = null;
                foreach (IAction a in allActions)
                {
                    if (a.ID == conditionPair.Key)
                    {
                        nextAct = a;
                        break;
                    }
                }
                if (nextAct == null || !conditionPair.Value.IsTrue(customRotation))
                {
                    continue;
                }

                DataCenter.ActionSequencerAction = nextAct as IAction;
                return;
            }
        }

        DataCenter.ActionSequencerAction = null;
    }

    public static void Enable(string folder)
    {
        _actionSequencerFolder = folder;
        if (!Directory.Exists(_actionSequencerFolder))
        {
            _ = Directory.CreateDirectory(_actionSequencerFolder);
        }

        LoadFiles();
    }

    public static void SaveFiles()
    {
        if (_actionSequencerFolder == null)
        {
            return;
        }

        try
        {
            Directory.Delete(_actionSequencerFolder, true);
            _ = Directory.CreateDirectory(_actionSequencerFolder);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error deleting directory: {ex.Message}");
        }

        foreach (MajorConditionValue set in DataCenter.ConditionSets)
        {
            set.Save(_actionSequencerFolder);
        }
    }

    public static void LoadFiles()
    {
        if (_actionSequencerFolder == null)
        {
            return;
        }

        DataCenter.ConditionSets = MajorConditionValue.Read(_actionSequencerFolder);
    }

    public static void AddNew()
    {
        bool hasUnnamed = false;
        foreach (MajorConditionValue conditionSet in DataCenter.ConditionSets)
        {
            if (conditionSet.IsUnnamed)
            {
                hasUnnamed = true;
                break;
            }
        }

        if (!hasUnnamed)
        {
            List<MajorConditionValue> newConditionSets = new(DataCenter.ConditionSets)
            {
                new MajorConditionValue()
            };
            DataCenter.ConditionSets = newConditionSets.ToArray();
        }
    }

    public static void Delete(string name)
    {
        List<MajorConditionValue> newConditionSets = [];
        foreach (MajorConditionValue conditionSet in DataCenter.ConditionSets)
        {
            if (conditionSet.Name != name)
            {
                newConditionSets.Add(conditionSet);
            }
        }
        DataCenter.ConditionSets = newConditionSets.ToArray();

        string filePath = Path.Combine(_actionSequencerFolder ?? string.Empty, $"{name}.json");
        File.Delete(filePath);
    }
}