using RotationSolver.Basic.Configuration;
using RotationSolver.Data;
using RotationSolver.UI.SearchableSettings;

namespace RotationSolver.UI.SearchableConfigs;

internal class AutoHealCheckBox(PropertyInfo property, params ISearchable[] otherChildren)
    : CheckBoxSearchCondition(property, otherChildren.Union(new ISearchable[]
        {
            _healthAreaAbility,
            _healthAreaAbilityHot,
            _healthAreaSpell ,
            _healthAreaSpellHot ,
            _healthSingleAbility ,
            _healthSingleAbilityHot ,
            _healthSingleSpell ,
            _healthSingleSpellHot ,
        }).ToArray())
{
    private readonly ISearchable[] _otherChildren = otherChildren;

    private static readonly DragFloatSearch
        _healthAreaAbility = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthAreaAbility))!),
        _healthAreaAbilityHot = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthAreaAbilityHot))!),
        _healthAreaSpell = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthAreaSpell))!),
        _healthAreaSpellHot = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthAreaSpellHot))!),
        _healthSingleAbility = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthSingleAbility))!),
        _healthSingleAbilityHot = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthSingleAbilityHot))!),
        _healthSingleSpell = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthSingleSpell))!),
        _healthSingleSpellHot = new(typeof(Configs).GetRuntimeProperty(nameof(Configs.HealthSingleSpellHot))!);

    protected override void DrawChildren()
    {
        foreach (var child in _otherChildren)
        {
            child.Draw();
        }

        if (ImGui.BeginTable("Healing things", 3, ImGuiTableFlags.Borders
            | ImGuiTableFlags.Resizable
            | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableNextColumn();
            ImGui.TableHeader("");

            ImGui.TableNextColumn();
            ImGui.TableHeader(UiString.NormalTargets.GetDescription());

            ImGui.TableNextColumn();
            ImGui.TableHeader(UiString.HotTargets.GetDescription());

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(UiString.HpAoe0Gcd.GetDescription());

            ImGui.TableNextColumn();

            _healthAreaAbility?.Draw();

            ImGui.TableNextColumn();

            _healthAreaAbilityHot?.Draw();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(UiString.HpAoeGcd.GetDescription());

            ImGui.TableNextColumn();

            _healthAreaSpell?.Draw();


            ImGui.TableNextColumn();

            _healthAreaSpellHot?.Draw();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(UiString.HpSingle0Gcd.GetDescription());

            ImGui.TableNextColumn();

            _healthSingleAbility?.Draw();

            ImGui.TableNextColumn();

            _healthSingleAbilityHot?.Draw();

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(UiString.HpSingleGcd.GetDescription());

            ImGui.TableNextColumn();

            _healthSingleSpell?.Draw();

            ImGui.TableNextColumn();

            _healthSingleSpellHot?.Draw();

            ImGui.EndTable();
        }
    }
}
