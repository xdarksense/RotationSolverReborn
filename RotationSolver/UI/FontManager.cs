using ECommons.DalamudServices;

namespace RotationSolver.UI
{
    public static class FontManager
    {
        public unsafe static ImFontPtr GetFont(float size)
        {
            // Get the recommended font style based on the specified size
            var style = new Dalamud.Interface.GameFonts.GameFontStyle(
                Dalamud.Interface.GameFonts.GameFontStyle.GetRecommendedFamilyAndSize(
                    Dalamud.Interface.GameFonts.GameFontFamily.Axis, size));

            // Create a new game font handle
            var handle = Svc.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(style);

            try
            {
                // Lock the handle to get the font
                using (var lockedHandle = handle.Lock())
                {
                    var font = lockedHandle.ImFont;

                    // Check if the font pointer is valid
                    if (new IntPtr(font.NativePtr) == IntPtr.Zero)
                    {
                        return ImGui.GetFont();
                    }

                    // Scale the font to the desired size
                    font.Scale = size / font.FontSize;
                    return font;
                }
            }
            catch (Exception)
            {
                return ImGui.GetFont();
            }
        }
    }
}