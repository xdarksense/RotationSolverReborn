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
                // Split command into parts
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int index = -1;

                // Try to parse the second argument as TargetingType if present
                if (parts.Length > 1)
                {
                    string value = parts[1];
                    if (Enum.TryParse(typeof(TargetingType), value, true, out object? parsedEnumSet))
                    {
                        TargetingType targetingTypeSet = (TargetingType)parsedEnumSet;
                        int idx = Service.Config.TargetingTypes.IndexOf(targetingTypeSet);
                        if (idx >= 0)
                        {
                            Service.Config.TargetingIndex = idx;
                            Svc.Chat.Print($"Set current TargetingType to {targetingTypeSet}.");
                            index = idx;
                        }
                        else
                        {
                            Svc.Chat.PrintError($"{targetingTypeSet} is not in TargetingTypes list.");
                            return;
                        }
                    }
                    else if (!int.TryParse(value, out index))
                    {
                        index = -1;
                    }
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