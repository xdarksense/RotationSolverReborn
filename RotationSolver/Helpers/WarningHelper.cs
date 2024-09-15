using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.DalamudServices;
using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

public static class WarningHelper
{
    private static readonly ConcurrentQueue<string> _showWarnings = new();
    private static bool _run = false;
    private static readonly object _lock = new();

    public static SeString RS_String => new(new IconPayload(BitmapFontIcon.ExclamationRectangle),
              RotationSolverPlugin.OpenLinkPayload,
              new UIForegroundPayload(31),
              new TextPayload("Rotation Solver Reborn"),
              UIForegroundPayload.UIForegroundOff,
              RawPayload.LinkTerminator,
              new TextPayload(": "));

    public static SeString Close_String => new(new IconPayload(BitmapFontIcon.DoNotDisturb), RotationSolverPlugin.HideWarningLinkPayload ?? throw new InvalidOperationException("HideWarningLinkPayload is null"),
              new UIForegroundPayload(2),
              new TextPayload("(Hide Warning)"),
              UIForegroundPayload.UIForegroundOff,
              RawPayload.LinkTerminator);

    public static void ShowWarning(this string message, int times = 3, DalamudLinkPayload? link = null)
    {
        if (Service.Config.HideWarning) return;

        var seString = RS_String.Append(link == null
            ? new SeString(new TextPayload(message))
            : new SeString(link,
              new TextPayload(message),
              RawPayload.LinkTerminator)).Append(Close_String);

        Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry()
        {
            Message = seString,
            Type = Dalamud.Game.Text.XivChatType.ErrorMessage,
        });

        for (int i = 0; i < times; i++)
        {
            _showWarnings.Enqueue(message);
        }

        lock (_lock)
        {
            if (!_run)
            {
                _run = true;
                Task.Run(RunShowError);
            }
        }
    }

    internal static bool AddSystemWarning(string warning)
    {
        if (!DataCenter.SystemWarnings.ContainsKey(warning))
        {
            DataCenter.SystemWarnings.Add(warning, DateTime.Now);
            return true;
        }
        return false;
    }

    private static async Task RunShowError()
    {
        while (_showWarnings.TryDequeue(out var message))
        {
            Svc.Toasts.ShowError(message);
            await Task.Delay(3000);
        }

        lock (_lock)
        {
            _run = false;
        }
    }
}