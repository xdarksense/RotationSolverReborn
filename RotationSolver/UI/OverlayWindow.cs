using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using RotationSolver.UI.HighlightTeachingMode;
using System.Linq;
using System.Threading.Tasks;

namespace RotationSolver.UI;

/// <summary>
/// The Overlay Window
/// </summary>
internal class OverlayWindow : Window
{
    const ImGuiWindowFlags BaseFlags = ImGuiWindowFlags.NoBackground
    | ImGuiWindowFlags.NoBringToFrontOnFocus
    | ImGuiWindowFlags.NoDecoration
    | ImGuiWindowFlags.NoDocking
    | ImGuiWindowFlags.NoFocusOnAppearing
    | ImGuiWindowFlags.NoInputs
    | ImGuiWindowFlags.NoNav;

    public OverlayWindow()
        : base(nameof(OverlayWindow), BaseFlags, true)
    {
        IsOpen = true;
        AllowClickthrough = true;
        RespectCloseHotkey = false;
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);

        base.PreDraw();
    }

    public override async void Draw()
    {
        if (!HotbarHighlightManager.Enable || Svc.ClientState == null || Svc.ClientState.LocalPlayer == null)
            return;

        ImGui.GetStyle().AntiAliasedFill = false;

        try
        {
            await UpdateDrawingElementsAsync();

            if (HotbarHighlightManager._drawingElements2D != null)
            {
                foreach (var item in HotbarHighlightManager._drawingElements2D.OrderBy(GetDrawingOrder))
                {
                    item.Draw();
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, $"{nameof(OverlayWindow)} failed to draw on Screen.");
        }
    }

    private async Task UpdateDrawingElementsAsync()
    {
        if (!HotbarHighlightManager.UseTaskToAccelerate)
        {
            HotbarHighlightManager._drawingElements2D = await HotbarHighlightManager.To2DAsync();
        }
    }

    private int GetDrawingOrder(object drawing)
    {
        if (drawing is PolylineDrawing poly)
        {
            return poly._thickness == 0 ? 0 : 1;
        }
        else
        {
            return 2;
        }
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}