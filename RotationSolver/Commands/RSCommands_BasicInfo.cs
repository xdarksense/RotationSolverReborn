using Dalamud.Game.Command;
using ECommons.DalamudServices;
using RotationSolver.Data;

namespace RotationSolver.Commands
{
    public static partial class RSCommands
    {
        internal static void Enable()
        {
            _ = Svc.Commands.AddHandler(Service.COMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = UiString.Commands_Rotation.GetDescription(),
                ShowInHelp = true,
            });
            _ = Svc.Commands.AddHandler(Service.ALTCOMMAND, new CommandInfo(OnCommand)
            {
                HelpMessage = UiString.Commands_Rotation.GetDescription(),
                ShowInHelp = true,
            });
        }

        internal static void Disable()
        {
            _ = Svc.Commands.RemoveHandler(Service.COMMAND);
            _ = Svc.Commands.RemoveHandler(Service.ALTCOMMAND);
        }

        private static void OnCommand(string command, string arguments)
        {
            DoOneCommand(arguments);
        }

        private static void DoOneCommand(string command)
        {
            if (command.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                command = "off";
            }

            if (TryGetOneEnum<StateCommandType>(command, out StateCommandType stateType))
            {
                string? indexStr = command.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                if (!int.TryParse(indexStr, out int index))
                {
                    index = -1;
                }
                DoStateCommandType(stateType, index);
            }
            else if (TryGetOneEnum<SpecialCommandType>(command, out SpecialCommandType specialType))
            {
                DoSpecialCommandType(specialType);
            }
            else if (TryGetOneEnum<OtherCommandType>(command, out OtherCommandType otherType))
            {
                string extraCommand = command[otherType.ToString().Length..].Trim();
                DoOtherCommand(otherType, extraCommand);
            }
            else
            {
                RotationSolverPlugin.OpenConfigWindow();
            }
        }

        private static bool TryGetOneEnum<T>(string command, out T type) where T : struct, Enum
        {
            type = default;
            foreach (T c in Enum.GetValues<T>())
            {
                if (command.StartsWith(c.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    type = c;
                    return true;
                }
            }
            return false;
        }

        internal static string GetCommandStr(this Enum command, string extraCommand = "")
        {
            string cmdStr = $"{Service.COMMAND} {command}";
            if (!string.IsNullOrEmpty(extraCommand))
            {
                cmdStr += $" {extraCommand}";
            }
            return cmdStr;
        }
    }
}