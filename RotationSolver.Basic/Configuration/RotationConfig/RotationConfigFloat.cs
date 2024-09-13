namespace RotationSolver.Basic.Configuration.RotationConfig;

internal class RotationConfigFloat : RotationConfigBase
{
    public float Min, Max, Speed;

    public ConfigUnitType UnitType { get; set; }

    public RotationConfigFloat(ICustomRotation rotation, PropertyInfo property)
        : base(rotation, property)
    {
        var attr = property.GetCustomAttribute<RangeAttribute>();
        if (attr != null)
        {
            Min = attr.MinValue;
            Max = attr.MaxValue;
            Speed = attr.Speed;
            UnitType = attr.UnitType;
        }
        else
        {
            Min = 0.0f;
            Max = 1.0f;
            Speed = 0.005f;
            UnitType = ConfigUnitType.Percent;
        }
    }

    public override bool DoCommand(IRotationConfigSet set, string str)
    {
        if (str == null) return false;
        if (!base.DoCommand(set, str)) return false;

        // Ensure the string has sufficient length before slicing
        if (str.Length <= Name.Length) return false;

        string numStr = str[Name.Length..].Trim();

        // Parse the float value and set it
        if (float.TryParse(numStr, out float parsedValue))
        {
            Value = parsedValue.ToString(); // Convert float to string
            return true;
        }

        return false;
    }
}