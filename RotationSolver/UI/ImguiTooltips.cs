using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace RotationSolver.UI;

internal static class ImguiTooltips
{
    const ImGuiWindowFlags TOOLTIP_FLAG =
          ImGuiWindowFlags.Tooltip |
          ImGuiWindowFlags.NoMove |
          ImGuiWindowFlags.NoSavedSettings |
          ImGuiWindowFlags.NoBringToFrontOnFocus |
          ImGuiWindowFlags.NoDecoration |
          ImGuiWindowFlags.NoInputs |
          ImGuiWindowFlags.AlwaysAutoResize;

    const string TOOLTIP_ID = "RotationSolver Tooltips";

    /// <summary>
    /// Displays a tooltip when the item is hovered.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    public static void HoveredTooltip(string? text)
    {
        if (!ImGui.IsItemHovered()) return;
        ShowTooltip(text);
    }

    /// <summary>
    /// Displays a tooltip with the specified text.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    public static void ShowTooltip(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        ShowTooltip(() => ImGui.Text(text));
    }

    /// <summary>
    /// Displays a tooltip with the specified action.
    /// </summary>
    /// <param name="act">The action to perform to render the tooltip content.</param>
    public static void ShowTooltip(Action act)
    {
        if (act == null) return;
        if (Service.Config == null || !Service.Config.ShowTooltips) return;

        ImGui.SetNextWindowBgAlpha(1);

        using var color = ImRaii.PushColor(ImGuiCol.BorderShadow, ImGuiColors.DalamudWhite);

        ImGui.SetNextWindowSizeConstraints(new Vector2(150, 0) * ImGuiHelpers.GlobalScale, new Vector2(1200, 1500) * ImGuiHelpers.GlobalScale);
        ImGui.SetWindowPos(TOOLTIP_ID, ImGui.GetIO().MousePos);

        if (ImGui.Begin(TOOLTIP_ID, TOOLTIP_FLAG))
        {
            act();
            ImGui.End();
        }
    }
}