using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;
using RotationSolver.Data;

using RotationSolver.Updaters;

namespace RotationSolver.Commands;

public static partial class RSCommands
{
    private static void DoOtherCommand(OtherCommandType otherType, string str)
    {
        switch (otherType)
        {
            case OtherCommandType.Rotations:
                var customCombo = DataCenter.RightNowRotation;
                if (customCombo == null) return;

                DoRotationCommand(customCombo, str);
                break;

            case OtherCommandType.DoActions:
                DoActionCommand(str);
                break;

            case OtherCommandType.ToggleActions:
                ToggleActionCommand(str);
                break;

            case OtherCommandType.Settings:
                DoSettingCommand(str);
                break;

            case OtherCommandType.NextAction:
                DoAction();
                break;
        }
    }

    private static void DoSettingCommand(string str)
    {
        var strs = str.Split(' ', 3);
        if (strs.Length < 2)
        {
            Svc.Chat.PrintError("Invalid setting command format.");
            return;
        }

        var settingName = strs[0];
        var command = strs.Length > 1 ? string.Join(' ', strs.Skip(1)) : null;

        if (string.IsNullOrEmpty(settingName))
        {
            Svc.Chat.PrintError("Invalid setting command format.");
            return;
        }

        if (settingName.Equals("TargetingTypes", StringComparison.OrdinalIgnoreCase))
        {
            HandleTargetingTypesCommand(settingName, command);
            return;
        }

        foreach (var property in typeof(Configs).GetRuntimeProperties().Where(p => p.GetMethod?.IsPublic ?? false))
        {
            if (!settingName.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            var type = property.PropertyType;
            if (type == typeof(ConditionBoolean))
                type = typeof(bool);

            object? convertedValue = null;
            bool valueParsedSuccessfully = true;

            if (type.IsEnum)
            {
                valueParsedSuccessfully = Enum.TryParse(type, command, ignoreCase: true, out var parsedEnum);
                if (valueParsedSuccessfully)
                {
                    convertedValue = parsedEnum;
                }
            }
            else
            {
                try
                {
                    convertedValue = Convert.ChangeType(command, type);
                }
                catch
                {
                    valueParsedSuccessfully = false;
                }
            }

            if (!valueParsedSuccessfully)
            {
                if (type == typeof(bool))
                {
                    var config = property.GetValue(Service.Config) as ConditionBoolean;
                    if (config != null)
                    {
                        config.Value = !config.Value;
                        convertedValue = config.Value;
                    }
                }
                else if (type.IsEnum)
                {
                    // If invalid enum value provided - increment to the next enum value
                    var currentEnumValue = property.GetValue(Service.Config) as Enum;
                    if (currentEnumValue != null)
                    {
                        convertedValue = GetNextEnumValue(currentEnumValue);
                    }
                }
            }

            if (convertedValue == null)
            {
                Svc.Chat.PrintError("Failed to parse the value.");
                return;
            }

            if (property.PropertyType == typeof(ConditionBoolean))
            {
                var relay = (ConditionBoolean)property.GetValue(Service.Config)!;
                relay.Value = (bool)convertedValue;
                convertedValue = relay;
            }

            property.SetValue(Service.Config, convertedValue);
            command = convertedValue.ToString();

            if (Service.Config.ShowToggledActionInChat)
            {
                Svc.Chat.Print(string.Format(UiString.CommandsChangeSettingsValue.GetDescription(), property.Name, command));
            }

            return;
        }

        Svc.Chat.PrintError("Failed to find the config in this rotation, please check it.");
    }

    private static void HandleTargetingTypesCommand(string settingName, string? command)
    {
        if (string.IsNullOrEmpty(command))
        {
            Svc.Chat.PrintError("Invalid command for TargetingTypes.");
            return;
        }

        var commandParts = command.Split(' ', 2);
        if (commandParts.Length < 1)
        {
            Svc.Chat.PrintError("Invalid command format for TargetingTypes.");
            return;
        }

        var action = commandParts[0];
        var value = commandParts.Length > 1 ? commandParts[1] : null;

        switch (action.ToLower())
        {
            case "add":
                if (string.IsNullOrEmpty(value) || !Enum.TryParse(typeof(TargetingType), value, true, out var parsedEnumAdd))
                {
                    Svc.Chat.PrintError("Invalid TargetingType value.");
                    return;
                }

                var targetingTypeAdd = (TargetingType)parsedEnumAdd;
                if (!Service.Config.TargetingTypes.Contains(targetingTypeAdd))
                {
                    Service.Config.TargetingTypes.Add(targetingTypeAdd);
                    Svc.Chat.Print($"Added {targetingTypeAdd} to TargetingTypes.");
                }
                else
                {
                    Svc.Chat.Print($"{targetingTypeAdd} is already in TargetingTypes.");
                }
                break;

            case "remove":
                if (string.IsNullOrEmpty(value) || !Enum.TryParse(typeof(TargetingType), value, true, out var parsedEnumRemove))
                {
                    Svc.Chat.PrintError("Invalid TargetingType value.");
                    return;
                }

                var targetingTypeRemove = (TargetingType)parsedEnumRemove;
                if (Service.Config.TargetingTypes.Contains(targetingTypeRemove))
                {
                    Service.Config.TargetingTypes.Remove(targetingTypeRemove);
                    Svc.Chat.Print($"Removed {targetingTypeRemove} from TargetingTypes.");
                }
                else
                {
                    Svc.Chat.Print($"{targetingTypeRemove} is not in TargetingTypes.");
                }
                break;

            case "removeall":
                Service.Config.TargetingTypes.Clear();
                Svc.Chat.Print("Removed all TargetingTypes.");
                break;

            default:
                Svc.Chat.PrintError("Invalid action for TargetingTypes.");
                break;
        }

        Service.Config.Save();
    }

    private static Enum GetNextEnumValue(Enum currentEnumValue)
    {
        var enumValues = Enum.GetValues(currentEnumValue.GetType()).Cast<Enum>().ToArray();
        var nextIndex = Array.IndexOf(enumValues, currentEnumValue) + 1;

        return enumValues.Length == nextIndex ? enumValues[0] : enumValues[nextIndex];
    }

    private static void ToggleActionCommand(string str)
    {
        foreach (var act in RotationUpdater.RightRotationActions)
        {
            if (str.StartsWith(act.Name))
            {
                var flag = str[act.Name.Length..].Trim();

                act.IsEnabled = bool.TryParse(flag, out var parse) ? parse : !act.IsEnabled;

                if (Service.Config.ShowToggledActionInChat)
                {
                    Svc.Chat.Print($"Toggled {act.Name} : {act.IsEnabled}");
                }

                return;
            }
        }
    }

    private static void DoActionCommand(string str)
    {
        var lastHyphenIndex = str.LastIndexOf('-');
        if (lastHyphenIndex == -1 || lastHyphenIndex == str.Length - 1)
        {
            Svc.Chat.PrintError(UiString.CommandsInsertActionFailure.GetDescription());
            return;
        }

        var actName = str.Substring(0, lastHyphenIndex).Trim();
        var timeStr = str.Substring(lastHyphenIndex + 1).Trim();

        if (double.TryParse(timeStr, out var time))
        {
            foreach (var iAct in RotationUpdater.RightRotationActions)
            {
                if (actName.Equals(iAct.Name, StringComparison.OrdinalIgnoreCase))
                {
                    DataCenter.AddCommandAction(iAct, time);

                    if (Service.Config.ShowToastsAboutDoAction)
                    {
                        Svc.Toasts.ShowQuest(string.Format(UiString.CommandsInsertAction.GetDescription(), time),
                            new Dalamud.Game.Gui.Toast.QuestToastOptions()
                            {
                                IconId = iAct.IconID,
                            });
                    }

                    return;
                }
            }
        }

        Svc.Chat.PrintError(UiString.CommandsInsertActionFailure.GetDescription());
    }


    private static void DoRotationCommand(ICustomRotation customCombo, string str)
    {
        var configs = customCombo.Configs;
        foreach (var config in configs)
        {
            if (config.DoCommand(configs, str))
            {
                if (Service.Config.ShowToggledActionInChat)
                {
                    Svc.Chat.Print(string.Format(UiString.CommandsChangeSettingsValue.GetDescription(),
                    config.DisplayName, config.Value));

                    return;
                }
            }
        }

        Svc.Chat.PrintError(UiString.CommandsInsertActionFailure.GetDescription());
    }
}