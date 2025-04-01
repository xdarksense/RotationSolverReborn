using RotationSolver.Updaters;

namespace RotationSolver.UI;

internal class CooldownWindow() : CtrlWindow(nameof(CooldownWindow))
{
    public override void Draw()
    {
        if (DataCenter.CurrentRotation == null) return;

        var config = Service.Config;
        var width = config.CooldownWindowIconSize;
        const float IconSpacingFactor = 6f / 82f;
        var style = ImGui.GetStyle();
        var columnWidth = ImGui.GetColumnWidth();
        var count = Math.Max(1, (int)MathF.Floor(columnWidth / (width * (1 + IconSpacingFactor) + style.ItemSpacing.X)));

        var allGroupedActions = RotationUpdater.AllGroupedActions;
        if (allGroupedActions == null) return;

        foreach (var pair in allGroupedActions)
        {
            var showItems = pair
                .Where(a => a.IsInCooldown && (a is not IBaseAction b || !b.Info.IsLimitBreak))
                .Where(a => config.ShowGcdCooldown || !(a is IBaseAction b && b.Info.IsGeneralGCD))
                .Where(a => config.ShowItemsCooldown || !(a is IBaseItem))
                .OrderBy(a => a.SortKey)
                .ToList();

            if (!showItems.Any()) continue;

            ImGui.Text(pair.Key);

            ImGui.Columns(count, null, false);
            uint itemIndex = 0;
            foreach (var item in showItems)
            {
                try
                {
                    ControlWindow.DrawIAction(item, width, 1f);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error drawing action: {ex.Message}");
                }
                itemIndex++;
                if (itemIndex % count != 0)
                {
                    ImGui.NextColumn();
                }
            }
            ImGui.Columns(1);
            ImGui.NewLine();
        }
    }
}