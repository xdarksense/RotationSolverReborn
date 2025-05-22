using RotationSolver.Basic.Configuration;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RotationSolver.Basic.Helpers
{
    internal class PriorityTargetHelper
    {
        private static readonly string FilePath = "PrioTargetId.json";

        // List of OIDs (DataId)
        private static readonly HashSet<uint> priorityOids = LoadPriorityOids();

        // Method to check if the given DataId is a priority target
        public static bool IsPriorityTarget(uint dataId)
        {
            return priorityOids.Contains(dataId);
        }

        // Method to add a DataId to the priority list
        public static void AddPriorityTarget(uint dataId)
        {
            if (!priorityOids.Contains(dataId))
            {
                _ = priorityOids.Add(dataId);
                _ = OtherConfiguration.SavePrioTargetId();
            }
        }

        // Method to load priority OIDs from JSON file
        private static HashSet<uint> LoadPriorityOids()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<HashSet<uint>>(json) ?? [];
            }
            return [];
        }
    }
}