namespace RotationSolver.Extensions
{
    public static class CommandTypeExtensions
    {
        public static string ToStateString(this StateCommandType stateType, JobRole role)
        {
            if (DataCenter.IsPvP && stateType == StateCommandType.Auto)
            {
                return $"{stateType} (LowHP)";
            }
            else if (stateType == StateCommandType.Auto)
            {
                return $"{stateType} ({DataCenter.TargetingType})";
            }
            return stateType.ToString();
        }

        public static string ToSpecialString(this SpecialCommandType specialType, JobRole role)
        {
            return specialType.ToString();
        }
    }
}