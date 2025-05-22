namespace RotationSolver.Helpers;

public static class WarningHelper
{
    public static void AddSystemWarning(string message)
    {
        DataCenter.SystemWarnings ??= [];
        if (!DataCenter.SystemWarnings.ContainsKey(message))
        {
            DataCenter.SystemWarnings.Add(message, DateTime.Now);
        }
    }
}