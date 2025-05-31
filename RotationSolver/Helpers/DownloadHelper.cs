using ECommons.Logging;
using RotationSolver.UI;

namespace RotationSolver.Helpers;

public static class DownloadHelper
{
    public static IncompatiblePlugin[] IncompatiblePlugins { get; private set; } = [];

    public static async Task DownloadAsync()
    {
        IncompatiblePlugins = await DownloadOneAsync<IncompatiblePlugin[]>($"https://raw.githubusercontent.com/{Service.USERNAME}/{Service.REPO}/main/Resources/IncompatiblePlugins.json") ?? [];
    }

    private static async Task<T?> DownloadOneAsync<T>(string url)
    {
        using HttpClient client = new();
        try
        {
            string str = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(str);
        }
        catch (Exception ex)
        {
            WarningHelper.AddSystemWarning($"Failed to load downloading List because: {ex.Message}");
#if DEBUG
            PluginLog.Warning($"Failed to load downloading List: {ex.Message}");
#endif
            return default;
        }
    }
}
