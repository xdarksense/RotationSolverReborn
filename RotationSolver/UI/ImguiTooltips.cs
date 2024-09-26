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

    const string TOOLTIP_ID = "RotationSolverReborn Tooltips";

    /// <summary>
    /// Displays a tooltip when the item is hovered.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    public static void HoveredTooltip(string? text)
    {
        if (ImGui.IsItemHovered() && !string.IsNullOrEmpty(text))
        {
            ShowTooltip(() => ImGui.Text(text));
        }
    }

    /// <summary>
    /// Displays a tooltip with the specified text.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    public static void ShowTooltip(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            ShowTooltip(() => ImGui.Text(text));
        }
    }

    /// <summary>
    /// Displays a tooltip with the specified action.
    /// </summary>
    /// <param name="act">The action to perform to render the tooltip content.</param>
    public static void ShowTooltip(Action act)
    {
        if (act == null || Service.Config == null || !Service.Config.ShowTooltips) return;

        ImGui.SetNextWindowBgAlpha(1);

        using var color = ImRaii.PushColor(ImGuiCol.BorderShadow, ImGuiColors.DalamudWhite);

        var globalScale = ImGuiHelpers.GlobalScale;
        ImGui.SetNextWindowSizeConstraints(new Vector2(150, 0) * globalScale, new Vector2(1200, 1500) * globalScale);
        ImGui.SetWindowPos(TOOLTIP_ID, ImGui.GetIO().MousePos);

        if (ImGui.Begin(TOOLTIP_ID, TOOLTIP_FLAG))
        {
            act();
            ImGui.End();
        }
    }
}