using System.Text.RegularExpressions;

namespace RotationSolver.Updaters;
internal static partial class RaidTimeUpdater
{
    internal static readonly Dictionary<uint, string> PathForRaids = new();

    private static readonly Dictionary<uint, TimelineItem[]> _savedTimeLines = new();

    public static string GetLink(uint id)
    {
        return string.Empty;
    }

    internal static void UpdateTimeline()
    {
        return;
    }

    static readonly Dictionary<ulong, bool> _isInCombat = new();
    private static void UpdateTimelineAddCombat()
    {
        return;
    }

    private static void UpdateTimelineEvent()
    {
        return;
    }

    private static readonly List<uint> _downloadingList = new();
    public static void DownloadTerritory(uint id)
    {
        return;
    }

    private static void DownloadTerritoryPrivate(uint id)
    {
        return;
    }

    internal static async void EnableAsync()
    {
        return;
    }

    internal static void Disable()
    {
        return;
    }

    private static void DutyState_DutyWiped(object? _, ushort e)
    {
        return;
    }

    private static void Chat_ChatMessage(Dalamud.Game.Text.XivChatType type, int senderId, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
    {
        return;
    }

    private static void GameNetwork_NetworkMessage(nint dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, Dalamud.Game.Network.NetworkMessageDirection direction)
    {
        return;
    }

    private static void OnCast(IntPtr dataPtr, uint targetActorId)
    {
        return;
    }

    private static void OnEffect(IntPtr dataPtr, uint targetActorId)
    {
        return;
    }

    private static void OnActorControl(IntPtr dataPtr)
    {
        return;
    }

    private static void OnSystemLogMessage(IntPtr dataPtr)
    {
        return;
    }

    private unsafe static ushort ReadUshort(IntPtr dataPtr, int offset)
    {
        return 0;
    }

    private unsafe static uint ReadUint(IntPtr dataPtr, int offset)
    {
        return 0;
    }

    private unsafe static float ReadFloat(IntPtr dataPtr, int offset)
    {
        return 0f;
    }

    private static string GetNameFromObjectId(ulong id)
    {
        return string.Empty;
    }

    private static async void ClientState_TTerritoryChanged(ushort id)
    {
        return;
    }

    internal static TimelineItem[] GetRaidTime(ushort id)
    {
        return Array.Empty<TimelineItem>();
    }

    static async Task<TimelineItem[]> DownloadRaidAsync(string path)
    {
        return Array.Empty<TimelineItem>();
    }

    static async Task<TimelineItem[]> GetRaidAsync(ushort id)
    {
        return Array.Empty<TimelineItem>();
    }

    static async Task<RaidLangs> DownloadRaidLangsAsync(string path)
    {
        return new RaidLangs();
    }

    static async Task<TimelineItem[]> DownloadRaidTimeAsync(string path, RaidLangs lang)
    {
        return Array.Empty<TimelineItem>();
    }

    [GeneratedRegex("jump [\\d\\.]+")]
    private static partial Regex JumpTime();

    [GeneratedRegex("jump \".*?\"")]
    private static partial Regex JumpName();

    [GeneratedRegex("window [\\d\\.,]+")]
    private static partial Regex WindowTime();

    [GeneratedRegex(" .*? {")]
    private static partial Regex Type();

    [GeneratedRegex("^[\\d\\.]+.*")]
    private static partial Regex TimeLineItem();

    [GeneratedRegex("^[\\d\\.]+")]
    private static partial Regex Time();

    [GeneratedRegex("\".*?\"")]
    private static partial Regex Name();

    [GeneratedRegex("^[\\d\\.]+ \".*?\"")]
    private static partial Regex TimeHeader();

    [GeneratedRegex("^[\\d\\.]+ label \".*?\"")]
    private static partial Regex LabelHeader();

    [GeneratedRegex("{.*}")]
    private static partial Regex ActionGetter();
}
