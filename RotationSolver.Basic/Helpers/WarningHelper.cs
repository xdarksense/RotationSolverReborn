using ECommons.Logging;

namespace RotationSolver.Basic.Helpers
{
    internal static class WarningHelper
    {
        internal static bool AddSystemWarning(string warning)
        {
            if (DataCenter.SystemWarnings == null)
            {
                // Log the error before throwing the exception
                PluginLog.Error("SystemWarnings dictionary is not initialized.");
                throw new InvalidOperationException("SystemWarnings dictionary is not initialized.");
            }

            lock (DataCenter.SystemWarnings)
            {
                try
                {
                    if (!DataCenter.SystemWarnings.ContainsKey(warning))
                    {
                        DataCenter.SystemWarnings.Add(warning, DateTime.Now);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    PluginLog.Error($"Failed to add system warning: {ex.Message}");
                }
            }
            return false;
        }
    }
}