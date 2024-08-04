using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using ECommons.Reflection;

namespace RotationSolver.Helpers;

internal static class SpeechHelper
{
    private static IDalamudPlugin? _textToTalk;
    private static MethodInfo? _say;
    private static object? _manager;
    private static MethodInfo? _stop;

    internal static void Speak(string text)
    {
        if (_textToTalk == null)
        {
            if (!DalamudReflector.TryGetDalamudPlugin("TextToTalk", out _textToTalk))
            {
                return;
            }
        }

        _say ??= _textToTalk?.GetType().GetMethod("Say", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        _manager ??= _textToTalk?.GetType().GetField("backendManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(_textToTalk);
        _stop ??= _manager?.GetType().GetMethod("CancelAllSpeech", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        try
        {
            _stop?.Invoke(_manager, null);

            _say?.Invoke(_textToTalk, new object?[] { null, new SeString(new TextPayload("Rotation Solver")), XivChatType.SystemMessage, text, 2 });
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, "Something went wrong with TextToTalk.");
        }
    }
}