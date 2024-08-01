using ECommons.DalamudServices;

namespace RotationSolver.UI
{
    public static class FontManager
    {
        public unsafe static ImFontPtr GetFont(float size)
        {
            var style = new Dalamud.Interface.GameFonts.GameFontStyle(Dalamud.Interface.GameFonts.GameFontStyle.GetRecommendedFamilyAndSize(Dalamud.Interface.GameFonts.GameFontFamily.Axis, size));

            var handle = Svc.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(style);

            try
            {
                var font = handle.Lock().ImFont;

                if ((nint)font.NativePtr == nint.Zero)
                {
                    return ImGui.GetFont();
                }
                font.Scale = size / font.FontSize;
                return font;
            }
            catch
            {
                return ImGui.GetFont();
            }
        }
    }
}
