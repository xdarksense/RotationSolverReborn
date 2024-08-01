namespace RotationSolver.Basic.Helpers
{
    internal static class WarningHelper
    {
        internal static bool AddSystemWarning(string warning)
        {
            if (!DataCenter.SystemWarnings.ContainsKey(warning))
            {
                DataCenter.SystemWarnings.Add(warning, DateTime.Now);
                return true;
            }
            return false;
        }
    }
}
