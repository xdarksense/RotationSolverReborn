namespace RotationSolver.Basic.Helpers
{
    internal static class WarningHelper
    {
        internal static bool AddSystemWarning(string warning)
        {
            if (DataCenter.SystemWarnings == null)
            {
                throw new InvalidOperationException("SystemWarnings dictionary is not initialized.");
            }

            lock (DataCenter.SystemWarnings)
            {
                if (!DataCenter.SystemWarnings.ContainsKey(warning))
                {
                    DataCenter.SystemWarnings.Add(warning, DateTime.Now);
                    return true;
                }
            }
            return false;
        }
    }
}