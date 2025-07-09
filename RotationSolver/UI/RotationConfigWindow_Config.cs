using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Configuration.Conditions;
using RotationSolver.Data;

using RotationSolver.UI.SearchableConfigs;
using RotationSolver.Updaters;

namespace RotationSolver.UI;

public partial class RotationConfigWindow
{
    private string _searchText = string.Empty;
    private ISearchable[] _searchResults = [];

    internal static SearchableCollection _allSearchable = new();

    private void SearchingBox()
    {
        if (ImGui.InputTextWithHint("##Rotation Solver Reborn Search Box", UiString.ConfigWindow_Searching.GetDescription(), ref _searchText, 128, ImGuiInputTextFlags.AutoSelectAll))
        {
            _searchResults = _allSearchable.SearchItems(_searchText);
        }
    }

    #region Basic
    private static void DrawBasic()
    {
        _baseHeader?.Draw();
    }

    private static readonly CollapsingHeaderGroup _baseHeader = new(new Dictionary<Func<string>, Action>
    {
        { UiString.ConfigWindow_Basic_Timer.GetDescription, DrawBasicTimer },
        { UiString.ConfigWindow_Basic_NamedConditions.GetDescription, DrawBasicNamedConditions },
        { UiString.ConfigWindow_Basic_Others.GetDescription, DrawBasicOthers },
    });

    private static void DrawBasicTimer()
    {
        _allSearchable.DrawItems(Configs.BasicTimer);
    }

    private static readonly CollapsingHeaderGroup _autoSwitch = new(new Dictionary<Func<string>, Action>
    {
        {
            UiString.ConfigWindow_Basic_SwitchCancelConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.SwitchCancelConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Basic_SwitchManualConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.SwitchManualConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Basic_SwitchAutoConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.SwitchAutoConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
    })
    {
        HeaderSize = 18,
    };

    private static readonly Dictionary<int, bool> _isOpen = [];

    private static void DrawBasicNamedConditions()
    {
        // Ensure there is always an empty named condition at the end
        bool hasEmpty = false;
        (string Name, ConditionSet Condition)[] namedConditions = DataCenter.CurrentConditionValue.NamedConditions;
        for (int i = 0; i < namedConditions.Length; i++)
        {
            if (string.IsNullOrEmpty(namedConditions[i].Name))
            {
                hasEmpty = true;
                break;
            }
        }
        if (!hasEmpty)
        {
            // Append (string.Empty, new ConditionSet()) to the array
            (string Name, ConditionSet Condition)[] oldArray = DataCenter.CurrentConditionValue.NamedConditions;
            (string Name, ConditionSet Condition)[] newArray = new (string Name, ConditionSet Condition)[oldArray.Length + 1];
            for (int i = 0; i < oldArray.Length; i++)
            {
                newArray[i] = oldArray[i];
            }
            newArray[oldArray.Length] = (string.Empty, new ConditionSet());
            DataCenter.CurrentConditionValue.NamedConditions = newArray;
        }

        ImGui.Spacing();

        int removeIndex = -1;
        for (int i = 0; i < DataCenter.CurrentConditionValue.NamedConditions.Length; i++)
        {
            bool value = _isOpen.TryGetValue(i, out bool open) && open;

            FontAwesomeIcon toggle = value ? FontAwesomeIcon.ArrowUp : FontAwesomeIcon.ArrowDown;
            float ItemSpacing = 20 * Scale; // Changed from const to local variable
            float width = ImGui.GetWindowWidth() - ImGuiEx.CalcIconSize(FontAwesomeIcon.Ban).X
                - ImGuiEx.CalcIconSize(toggle).X - (ImGui.GetStyle().ItemSpacing.X * 2) - ItemSpacing;

            ImGui.SetNextItemWidth(width);
            _ = ImGui.InputTextWithHint($"##Rotation Solver Named Condition{i}", UiString.ConfigWindow_Condition_ConditionName.GetDescription(),
                ref DataCenter.CurrentConditionValue.NamedConditions[i].Name, 1024);

            ImGui.SameLine();

            if (ImGuiEx.IconButton(toggle, $"##Rotation Solver Toggle Named Condition{i}"))
            {
                _isOpen[i] = value = !value;
            }

            ImGui.SameLine();

            if (ImGuiEx.IconButton(FontAwesomeIcon.Ban, $"##Rotation Solver Remove Named Condition{i}"))
            {
                removeIndex = i;
            }

            if (value && DataCenter.CurrentRotation != null)
            {
                DataCenter.CurrentConditionValue.NamedConditions[i].Condition?.DrawMain(DataCenter.CurrentRotation);
            }
        }

        // Remove the named condition if needed
        if (removeIndex > -1)
        {
            // Convert array to list, remove, then back to array
            (string Name, ConditionSet Condition)[] oldArray = DataCenter.CurrentConditionValue.NamedConditions;
            List<(string Name, ConditionSet Condition)> newList = new(oldArray.Length - 1);
            for (int i = 0; i < oldArray.Length; i++)
            {
                if (i != removeIndex)
                {
                    newList.Add(oldArray[i]);
                }
            }
            DataCenter.CurrentConditionValue.NamedConditions = newList.ToArray();
        }
    }

    private static void DrawBasicOthers()
    {
        MajorConditionValue set = DataCenter.CurrentConditionValue;

        const string popUpId = "Right Set Popup";
        if (ImGui.Selectable(set.Name, false, ImGuiSelectableFlags.None, new Vector2(0, 20)))
        {
            ImGui.OpenPopup(popUpId);
        }
        ImguiTooltips.HoveredTooltip(UiString.ConfigWindow_ConditionSetDesc.GetDescription());

        using ImRaii.IEndObject popup = ImRaii.Popup(popUpId);
        if (popup)
        {
            MajorConditionValue[] combos = DataCenter.ConditionSets;
            for (int i = 0; i < combos.Length; i++)
            {
                void DeleteFile()
                {
                    ActionSequencerUpdater.Delete(combos[i].Name);
                }

                if (combos[i].Name == set.Name)
                {
                    ImGuiHelper.SetNextWidthWithName(set.Name);
                    _ = ImGui.InputText("##MajorConditionValue", ref set.Name, 100);
                }
                else
                {
                    string key = "Condition Set At " + i.ToString();
                    ImGuiHelper.DrawHotKeysPopup(key, string.Empty, (UiString.ConfigWindow_List_Remove.GetDescription(), DeleteFile, ["Delete"]));

                    if (ImGui.Selectable(combos[i].Name))
                    {
                        Service.Config.ActionSequencerIndex = i;
                    }

                    ImGuiHelper.ExecuteHotKeysPopup(key, string.Empty, string.Empty, false,
                        (DeleteFile, [VirtualKey.DELETE]));
                }
            }

            ImGui.PushFont(UiBuilder.IconFont);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            if (ImGui.Selectable(FontAwesomeIcon.Plus.ToIconString()))
            {
                ActionSequencerUpdater.AddNew();
            }
            ImGui.PopStyleColor();

            if (ImGui.Selectable(FontAwesomeIcon.FileDownload.ToIconString()))
            {
                ActionSequencerUpdater.LoadFiles();
            }

            ImGui.PopFont();
            ImguiTooltips.HoveredTooltip(UiString.ActionSequencer_Load.GetDescription());
        }
        _allSearchable.DrawItems(Configs.BasicParams);
    }
    #endregion

    #region UI
    private static void DrawUI()
    {
        _UIHeader?.Draw();
    }

    private static readonly CollapsingHeaderGroup _UIHeader = new(new Dictionary<Func<string>, Action>
    {
        {
            UiString.ConfigWindow_UI_Information.GetDescription,
            () => _allSearchable.DrawItems(Configs.UiInformation)
        },
        {
            UiString.ConfigWindow_UI_Windows.GetDescription,
            () => _allSearchable.DrawItems(Configs.UiWindows)
        },
    });

    #endregion

    #region Auto
    private const int HeaderSize = 18;

    /// <summary>
    /// Draws the auto section of the configuration window.
    /// </summary>
    private void DrawAuto()
    {
        ImGui.TextWrapped(UiString.ConfigWindow_Auto_Description.GetDescription());
        _autoHeader?.Draw();
    }

    private static readonly CollapsingHeaderGroup _autoHeader = new(new Dictionary<Func<string>, Action>
    {
        { UiString.ConfigWindow_Basic_AutoSwitch.GetDescription, DrawBasicAutoSwitch },
        { UiString.ConfigWindow_Auto_ActionUsage.GetDescription, DrawActionUsageControl },
        { UiString.ConfigWindow_Auto_HealingCondition.GetDescription, DrawHealingActionCondition },
        { UiString.ConfigWindow_Auto_PvPSpecific.GetDescription, DrawPvPSpecificControls },
        { UiString.ConfigWindow_Auto_StateCondition.GetDescription, () => _autoState?.Draw() },
    })
    {
        HeaderSize = HeaderSize,
    };

    private static void DrawBasicAutoSwitch()
    {
        _allSearchable.DrawItems(Configs.BasicAutoSwitch);
        _autoSwitch?.Draw();
    }

    private static void DrawPvPSpecificControls()
    {
        ImGui.TextWrapped(UiString.ConfigWindow_Auto_PvPSpecific.GetDescription());
        ImGui.Separator();
        _allSearchable.DrawItems(Configs.PvPSpecificControls);
    }

    /// <summary>
    /// Draws the Action Usage and Control section.
    /// </summary>
    private static void DrawActionUsageControl()
    {
        ImGui.TextWrapped(UiString.ConfigWindow_Auto_ActionUsage_Description.GetDescription());
        ImGui.Separator();
        _allSearchable.DrawItems(Configs.AutoActionUsage);
    }

    /// <summary>
    /// Draws the healing action condition section.
    /// </summary>
    private static void DrawHealingActionCondition()
    {
        ImGui.TextWrapped(UiString.ConfigWindow_Auto_HealingCondition_Description.GetDescription());
        ImGui.Separator();
        _allSearchable.DrawItems(Configs.HealingActionCondition);
    }

    private static readonly CollapsingHeaderGroup _autoState = new(new Dictionary<Func<string>, Action>
    {
        {
            UiString.ConfigWindow_Auto_HealAreaConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.HealAreaConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_HealSingleConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.HealSingleConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_DefenseAreaConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.DefenseAreaConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_DefenseSingleConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.DefenseSingleConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_DispelStancePositionalConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.DispelStancePositionalConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_RaiseShirkConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.RaiseShirkConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_MoveForwardConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.MoveForwardConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_MoveBackConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.MoveBackConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_AntiKnockbackConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.AntiKnockbackConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_SpeedConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.SpeedConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
        {
            UiString.ConfigWindow_Auto_NoCastingConditionSet.GetDescription,
            () => DataCenter.CurrentConditionValue.NoCastingConditionSet?.DrawMain(DataCenter.CurrentRotation)
        },
    })
    {
        HeaderSize = HeaderSize,
    };
    #endregion

    #region Target
    private static void DrawTarget()
    {
        _targetHeader?.Draw();
    }

    /// <summary>
    /// Header group for target-related configurations.
    /// </summary>
    private static readonly CollapsingHeaderGroup _targetHeader = new(new Dictionary<Func<string>, Action>
    {
    { UiString.ConfigWindow_Target_Config.GetDescription, DrawTargetConfig },
    { UiString.ConfigWindow_List_Hostile.GetDescription, DrawTargetHostile },
    });

    /// <summary>
    /// Draws the target configuration items.
    /// </summary>
    private static void DrawTargetConfig()
    {
        _allSearchable.DrawItems(Configs.TargetConfig);
    }

    private static void DrawTargetHostile()
    {
        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add Hostile"))
        {
            Service.Config.TargetingTypes.Add(TargetingType.Big);
        }
        ImGui.SameLine();
        ImGui.TextWrapped(UiString.ConfigWindow_Param_HostileDesc.GetDescription());

        for (int i = 0; i < Service.Config.TargetingTypes.Count; i++)
        {
            TargetingType targetType = Service.Config.TargetingTypes[i];
            string key = $"TargetingTypePopup_{i}";

            void Delete()
            {
                Service.Config.TargetingTypes.RemoveAt(i);
            }

            void Up()
            {
                Service.Config.TargetingTypes.RemoveAt(i);
                Service.Config.TargetingTypes.Insert(Math.Max(0, i - 1), targetType);
            }

            void Down()
            {
                Service.Config.TargetingTypes.RemoveAt(i);
                Service.Config.TargetingTypes.Insert(Math.Min(Service.Config.TargetingTypes.Count - 1, i + 1), targetType);
            }

            ImGuiHelper.DrawHotKeysPopup(key, string.Empty,
                (UiString.ConfigWindow_List_Remove.GetDescription(), Delete, new[] { "Delete" }),
                (UiString.ConfigWindow_Actions_MoveUp.GetDescription(), Up, new[] { "↑" }),
                (UiString.ConfigWindow_Actions_MoveDown.GetDescription(), Down, new[] { "↓" }));

            string[] names = Enum.GetNames(typeof(TargetingType));
            int targetingType = (int)Service.Config.TargetingTypes[i];
            string text = UiString.ConfigWindow_Param_HostileCondition.GetDescription();
            ImGui.SetNextItemWidth(ImGui.CalcTextSize(text).X + (30 * Scale));
            if (ImGui.Combo(text + "##HostileCondition" + i, ref targetingType, names, names.Length))
            {
                Service.Config.TargetingTypes[i] = (TargetingType)targetingType;
            }

            ImGuiHelper.ExecuteHotKeysPopup(key, string.Empty, string.Empty, true,
                (Delete, new[] { VirtualKey.DELETE }),
                (Up, new[] { VirtualKey.UP }),
                (Down, new[] { VirtualKey.DOWN }));
        }
    }
    #endregion

    #region Extra
    private static void DrawExtra()
    {
        ImGui.TextWrapped(UiString.ConfigWindow_Extra_Description.GetDescription());
        _extraHeader?.Draw();
    }

    private static readonly CollapsingHeaderGroup _extraHeader = new(new Dictionary<Func<string>, Action>
    {
    { UiString.ConfigWindow_EventItem.GetDescription, DrawEventTab },
    { UiString.ConfigWindow_Internal.GetDescription, DrawInternalTab },
    {
        UiString.ConfigWindow_Extra_Others.GetDescription,
        () => _allSearchable.DrawItems(Configs.Extra)
    },
    });
    private static void DrawInternalTab()
    {
        ImGui.Text($"Configs/Backups location: {Svc.PluginInterface.ConfigFile.Directory}");
        
        if (ImGui.Button("Backup Configs"))
        {
            Service.Config.Backup();
        }
        
        if (ImGui.Button("Restore Configs"))
        {
            Service.Config.Restore();
        }
    }

    private static void DrawEventTab()
    {
        if (ImGui.Button(UiString.ConfigWindow_Events_AddEvent.GetDescription()))
        {
            Service.Config.Events.Add(new ActionEventInfo());
        }
        ImGui.SameLine();

        ImGui.TextWrapped(UiString.ConfigWindow_Events_Description.GetDescription());

        ImGui.Text(UiString.ConfigWindow_Events_DutyStart.GetDescription());
        ImGui.SameLine();
        Service.Config.DutyStart.DisplayMacro();

        ImGui.Text(UiString.ConfigWindow_Events_DutyEnd.GetDescription());
        ImGui.SameLine();
        Service.Config.DutyEnd.DisplayMacro();

        ImGui.Separator();

        for (int i = 0; i < Service.Config.Events.Count; i++)
        {
            ActionEventInfo eve = Service.Config.Events[i];
            eve.DisplayEvent();

            ImGui.SameLine();

            if (ImGui.Button($"{UiString.ConfigWindow_Events_RemoveEvent.GetDescription()}##RemoveEvent{eve.GetHashCode()}"))
            {
                Service.Config.Events.RemoveAt(i);
                i--; // Adjust index after removal
            }
            ImGui.Separator();
        }
    }
    #endregion
}
