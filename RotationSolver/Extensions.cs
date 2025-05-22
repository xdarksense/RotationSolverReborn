namespace RotationSolver
{
    public static class CommandTypeExtensions
    {
        public static string ToStateString(this StateCommandType stateType, JobRole role)
        {
            return stateType == StateCommandType.Auto ? $"{stateType} ({DataCenter.TargetingType})" : stateType.ToString();
        }

        public static string ToSpecialString(this SpecialCommandType specialType, JobRole role)
        {
            return specialType.ToString();
        }
    }
}