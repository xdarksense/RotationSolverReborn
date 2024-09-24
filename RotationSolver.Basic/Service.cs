using System.Collections.Concurrent;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic;

/// <summary>
/// Provides various services and utilities for the RotationSolver.
/// </summary>
internal class Service : IDisposable
{
    public const string COMMAND = "/rotation";
    public const string ALTCOMMAND = "/rsr";
    public const string USERNAME = "FFXIV-CombatReborn";
    public const string REPO = "RotationSolverReborn";
    public const int ApiVersion = 4;

    // From https://GitHub.com/PunishXIV/Orbwalker/blame/master/Orbwalker/Memory.cs#L74-L76
    [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C7", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
    static IntPtr forceDisableMovementPtr = IntPtr.Zero;
    private static unsafe ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);

    static bool _canMove = true;

    /// <summary>
    /// Gets or sets a value indicating whether the player can move.
    /// </summary>
    internal static unsafe bool CanMove
    {
        get => ForceDisableMovement == 0;
        set
        {
            var realCanMove = value || DataCenter.NoPoslock;
            if (_canMove == realCanMove) return;
            _canMove = realCanMove;

            if (!realCanMove)
            {
                ForceDisableMovement++;
            }
            else if (ForceDisableMovement > 0)
            {
                ForceDisableMovement--;
            }
        }
    }

    /// <summary>
    /// Gets the remaining countdown time.
    /// </summary>
    public static float CountDownTime => Countdown.TimeRemaining;

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public static Configs Config { get; set; } = new Configs();

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    public static Configs ConfigDefault { get; set; } = new Configs();

    /// <summary>
    /// Initializes a new instance of the <see cref="Service"/> class.
    /// </summary>
    public Service()
    {
        Svc.Hook.InitializeFromAttributes(this);
    }

    /// <summary>
    /// Gets the adjusted action ID.
    /// </summary>
    /// <param name="id">The action ID.</param>
    /// <returns>The adjusted action ID.</returns>
    public static ActionID GetAdjustedActionId(ActionID id)
        => (ActionID)GetAdjustedActionId((uint)id);

    /// <summary>
    /// Gets the adjusted action ID.
    /// </summary>
    /// <param name="id">The action ID.</param>
    /// <returns>The adjusted action ID.</returns>
    public static unsafe uint GetAdjustedActionId(uint id)
        => ActionManager.Instance()->GetAdjustedActionId(id);


    private static readonly ConcurrentDictionary<Type, Addon?> AddonCache = new();

    /// <summary>
    /// Gets the addons of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the addon.</typeparam>
    /// <returns>A collection of addon pointers.</returns>
    public static IEnumerable<IntPtr> GetAddons<T>() where T : struct
    {
        // Check the cache for the attribute or add it if not present
        var addon = AddonCache.GetOrAdd(typeof(T), t => t.GetCustomAttribute<Addon>());

        if (addon is null) return Array.Empty<nint>();

        return addon.AddonIdentifiers
            .Select(str => Svc.GameGui.GetAddonByName(str, 1))
            .Where(ptr => ptr != IntPtr.Zero);
    }

    /// <summary>
    /// Gets the Excel sheet of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the Excel row.</typeparam>
    /// <returns>The Excel sheet.</returns>
    public static ExcelSheet<T> GetSheet<T>() where T : ExcelRow => Svc.Data.GetExcelSheet<T>()!;

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations.
    /// </summary>
    public void Dispose()
    {
        if (!_canMove && ForceDisableMovement > 0)
        {
            ForceDisableMovement--;
        }
    }
}