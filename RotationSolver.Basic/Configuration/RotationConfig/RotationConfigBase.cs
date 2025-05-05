using ECommons.DalamudServices;

namespace RotationSolver.Basic.Configuration.RotationConfig;

/// <summary>
/// Base class for rotation configuration.
/// </summary>
internal abstract class RotationConfigBase : IRotationConfig
{
    private readonly PropertyInfo _property;
    private readonly ICustomRotation _rotation;

    /// <summary>
    /// Gets the name of the configuration.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the default value of the configuration.
    /// </summary>
    public string DefaultValue { get; }

    /// <summary>
    /// Gets the display name of the configuration.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the combat type of the configuration.
    /// </summary>
    public CombatType Type { get; }

    /// <summary>
    /// Gets or sets the value of the configuration.
    /// </summary>
    public string Value
    {
        get
        {
            if (!Service.Config.RotationConfigurations.TryGetValue(Name, out var config)) return DefaultValue;
            return config;
        }
        set
        {
            Service.Config.RotationConfigurations[Name] = value;
            SetValue(value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotationConfigBase"/> class.
    /// </summary>
    /// <param name="rotation">The custom rotation instance.</param>
    /// <param name="property">The property information.</param>
    protected RotationConfigBase(ICustomRotation rotation, PropertyInfo property)
    {
        _property = property ?? throw new ArgumentNullException(nameof(property));
        _rotation = rotation ?? throw new ArgumentNullException(nameof(rotation));

        Name = property.Name;
        DefaultValue = property.GetValue(rotation)?.ToString() ?? string.Empty;
        var attr = property.GetCustomAttribute<RotationConfigAttribute>();
        if (attr != null)
        {
            DisplayName = attr.Name;
            Type = attr.Type;
        }
        else
        {
            DisplayName = Name;
            Type = CombatType.None;
        }

        // Set up initial value
        if (Service.Config.RotationConfigurations.TryGetValue(Name, out var value))
        {
            SetValue(value);
        }
    }

    /// <summary>
    /// Sets the value of the property.
    /// </summary>
    /// <param name="value">The value to set.</param>
    private void SetValue(string value)
    {
        var type = _property.PropertyType;
        if (type == null) return;

        try
        {
            _property.SetValue(_rotation, ChangeType(value, type));
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Failed to convert type.");
            _property.SetValue(_rotation, ChangeType(DefaultValue, type));
        }
    }

    /// <summary>
    /// Changes the type of the value.
    /// </summary>
    /// <param name="value">The value to change.</param>
    /// <param name="type">The target type.</param>
    /// <returns>The converted value.</returns>
    private static object ChangeType(string value, Type type)
    {
        if (type.IsEnum)
        {
            // Attempt to match the value with the enum name
            if (Enum.IsDefined(type, value))
            {
                return Enum.Parse(type, value);
            }

            // Attempt to match the value with the Description attribute
            foreach (var field in type.GetFields())
            {
                var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null && descriptionAttribute.Description == value)
                {
                    return Enum.Parse(type, field.Name);
                }
            }

            // Log a warning and return the default value if no match is found
            Svc.Log.Warning($"Invalid enum value '{value}' for type '{type.Name}'. Using default value.");
            return Enum.GetValues(type).GetValue(0) ?? throw new InvalidOperationException($"No default value available for enum type '{type.Name}'.");
        }
        else if (type == typeof(bool))
        {
            return bool.Parse(value);
        }

        return Convert.ChangeType(value, type) ?? throw new InvalidOperationException($"Failed to convert value '{value}' to type '{type.Name}'.");
    }

    /// <summary>
    /// Executes a command.
    /// </summary>
    /// <param name="set">The rotation config set.</param>
    /// <param name="str">The command string.</param>
    /// <returns><c>true</c> if the command was executed; otherwise, <c>false</c>.</returns>
    public virtual bool DoCommand(IRotationConfigSet set, string str) => str.StartsWith(Name);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => Value;
}