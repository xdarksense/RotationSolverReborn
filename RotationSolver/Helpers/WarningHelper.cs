namespace RotationSolver.Basic.Helpers;

public static class WarningHelper
{
    public static void AddSystemWarning(string message)
    {
        if (DataCenter.SystemWarnings == null)
        {
            DataCenter.SystemWarnings = new Dictionary<string, DateTime>();
        }
        if (!DataCenter.SystemWarnings.ContainsKey(message))
        {
            DataCenter.SystemWarnings.Add(message, DateTime.Now);
        }
    }
}