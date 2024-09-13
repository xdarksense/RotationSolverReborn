using RotationSolver.Data;

namespace RotationSolver.UI.SearchableConfigs;

internal class EnumSearch(PropertyInfo property) : Searchable(property)
{
    protected int Value
    {
        get => Convert.ToInt32(_property.GetValue(Service.Config));
        set => _property.SetValue(Service.Config, Enum.ToObject(_property.PropertyType, value));
    }

    protected override void DrawMain()
    {
        var currentValue = Value;

        // Create a map of enum values to their descriptions
        var enumValueToNameMap = new Dictionary<int, string>();
        foreach (Enum enumValue in Enum.GetValues(_property.PropertyType))
        {
            enumValueToNameMap[Convert.ToInt32(enumValue)] = enumValue.GetDescription();
        }

        var displayNames = enumValueToNameMap.Values.ToArray();

        if (displayNames.Length > 0)
        {
            // Set the width of the combo box
            ImGui.SetNextItemWidth(Math.Max(displayNames.Max(name => ImGui.CalcTextSize(name).X) + 30, DRAG_WIDTH) * Scale);

            // Find the current index of the selected value
            int currentIndex = enumValueToNameMap.Keys.ToList().IndexOf(currentValue);
            if (currentIndex == -1) currentIndex = 0; // Default to first item if not found

            // Cache the hash code to avoid multiple calls
            var hashCode = GetHashCode();

            // Draw the combo box
            if (ImGui.Combo($"##Config_{ID}{hashCode}", ref currentIndex, displayNames, displayNames.Length))
            {
                Value = enumValueToNameMap.Keys.ElementAt(currentIndex);
            }
        }

        // Show tooltip if item is hovered
        if (ImGui.IsItemHovered()) ShowTooltip();

        // Draw job icon if IsJob is true
        if (IsJob) DrawJobIcon();

        ImGui.SameLine();
        ImGui.TextWrapped(Name);

        // Show tooltip if item is hovered
        if (ImGui.IsItemHovered()) ShowTooltip(false);
    }
}
