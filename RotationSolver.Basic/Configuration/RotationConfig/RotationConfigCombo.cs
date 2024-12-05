namespace RotationSolver.Basic.Configuration.RotationConfig;

/// <summary>
/// Represents a combo box rotation configuration.
/// </summary>
internal class RotationConfigCombo : RotationConfigBase
{
    /// <summary>
    /// Gets the display values for the combo box.
    /// </summary>
    public string[] DisplayValues { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotationConfigCombo"/> class.
    /// </summary>
    /// <param name="rotation">The custom rotation instance.</param>
    /// <param name="property">The property information.</param>
    public RotationConfigCombo(ICustomRotation rotation, PropertyInfo property)
        : base(rotation, property)
    {
        if (!property.PropertyType.IsEnum)
        {
            throw new ArgumentException("Property type must be an enum", nameof(property));
        }

        var names = new List<string>();
        foreach (Enum v in Enum.GetValues(property.PropertyType))
        {
            names.Add(v.ToString());
        }

        DisplayValues = names.ToArray();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        var indexStr = base.ToString();
        if (!int.TryParse(indexStr, out var index) || index < 0 || index >= DisplayValues.Length)
        {
            return DisplayValues[0];
        }
        return DisplayValues[index];
    }

    /// <summary>
    /// Executes a command to update the combo box configuration.
    /// </summary>
    /// <param name="set">The rotation config set.</param>
    /// <param name="str">The command string.</param>
    /// <returns><c>true</c> if the command was executed; otherwise, <c>false</c>.</returns>
    public override bool DoCommand(IRotationConfigSet set, string str)
    {
        if (!base.DoCommand(set, str)) return false;

        string numStr = str[Name.Length..].Trim();
        var length = DisplayValues.Length;

        int nextId = (int.Parse(Value) + 1) % length;
        if (int.TryParse(numStr, out int num))
        {
            nextId = num % length;
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                if (DisplayValues[i].Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    nextId = i;
                    break;
                }
            }
        }

        Value = nextId.ToString();
        return true;
    }
}