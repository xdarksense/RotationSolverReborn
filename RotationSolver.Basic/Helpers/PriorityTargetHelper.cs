namespace RotationSolver.Basic.Helpers
{
    internal class PriorityTargetHelper
    {
        // List of OIDs (DataId)
        private static readonly HashSet<uint> priorityOids = new HashSet<uint>
        {
            0x415E, // Example OID
            // Add more OIDs here
        };

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
                priorityOids.Add(dataId);
            }
        }
    }
}