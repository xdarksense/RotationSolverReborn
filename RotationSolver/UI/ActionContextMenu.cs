using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.UI;

internal static class ActionContextMenu
{
    private static IContextMenu? contextMenu;

    public static void Init()
    {
        contextMenu = Svc.ContextMenu;
        contextMenu.OnMenuOpened += AddActionMenu;
    }

    private static void AddActionMenu(IMenuOpenedArgs args)
    {
        var contextAction = new BaseAction((ActionID)Svc.GameGui.HoveredAction.ActionID, false);

        Svc.Log.Debug(
            $"Menu spawned from {args.AddonName}, {args.AddonPtr}, {args.AgentPtr}, {args.EventInterfaces}, {args.MenuType}, {args.Target}");
        Svc.Log.Debug($"Menu spawned over {contextAction.Name}/{Svc.GameGui.HoveredAction.ActionKind}");

        var entry = new MenuItem
        {
            Name = "RotationSolverReborn",
            IsSubmenu = true,
            PrefixChar = 'R',
            PrefixColor = 545
        };

        entry.OnClicked += args => BuildSubMenu(args, contextAction);

        args.AddMenuItem(entry);
    }

    private static void BuildSubMenu(IMenuItemClickedArgs args, BaseAction contextAction)
    {
        var entries = new List<MenuItem>();

        if (contextAction.IsEnabled)
        {
            var enabledEntry = new MenuItem
            {
                Name = $"Disable {contextAction.Name}",
                PrefixChar = 'R',
                PrefixColor = 545
            };
            
            enabledEntry.OnClicked += clickedEntry => contextAction.IsEnabled = false;
            entries.Add(enabledEntry);
        }
        else
        {
            var enabledEntry = new MenuItem
            {
                Name = $"Enable {contextAction.Name}",
                PrefixChar = 'R',
                PrefixColor = 545
            };
            
            enabledEntry.OnClicked += clickedEntry => contextAction.IsEnabled = true;
            entries.Add(enabledEntry);
        }
        
        args.OpenSubmenu(entries);
    }
}