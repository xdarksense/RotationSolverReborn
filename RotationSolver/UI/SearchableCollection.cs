using RotationSolver.Basic.Configuration;
using RotationSolver.UI.SearchableConfigs;
using RotationSolver.UI.SearchableSettings;

namespace RotationSolver.UI;

internal readonly record struct SearchPair(UIAttribute Attribute, ISearchable Searchable);

internal class SearchableCollection
{
    private readonly List<SearchPair> _items;
    private static readonly char[] _splitChar = { ' ', ',', '、', '.', '。' };
    private const int MaxResultLength = 20;

    public SearchableCollection()
    {
        var properties = typeof(Configs).GetRuntimeProperties();
        var pairs = new List<SearchPair>(properties.Count());
        var parents = new Dictionary<string, CheckBoxSearch>(properties.Count());

        foreach (var property in properties)
        {
            var ui = property.GetCustomAttribute<UIAttribute>();
            if (ui == null) continue;

            var item = CreateSearchable(property);
            if (item == null) continue;

            item.PvEFilter = new(ui.PvEFilter);
            item.PvPFilter = new(ui.PvPFilter);

            pairs.Add(new(ui, item));

            if (item is CheckBoxSearch search)
            {
                parents[property.Name] = search;
            }
        }

        _items = new List<SearchPair>(pairs.Count);

        foreach (var pair in pairs)
        {
            var parentName = pair.Attribute.Parent;
            if (string.IsNullOrEmpty(parentName) || !parents.TryGetValue(parentName, out var parent))
            {
                _items.Add(pair);
            }
            else
            {
                parent.AddChild(pair.Searchable);
            }
        }
    }

    public void DrawItems(string filter)
    {
        bool isFirst = true;
        var filteredItems = _items.Where(i => i.Attribute.Filter == filter)
                                  .GroupBy(i => i.Attribute.Section);

        foreach (var grp in filteredItems)
        {
            if (!isFirst)
            {
                ImGui.Separator();
            }

            foreach (var item in grp.OrderBy(i => i.Attribute.Order))
            {
                item.Searchable.Draw();
            }

            isFirst = false;
        }
    }

    public ISearchable[] SearchItems(string searchingText)
    {
        if (string.IsNullOrEmpty(searchingText)) return Array.Empty<ISearchable>();

        var results = new ISearchable[MaxResultLength];
        var enumerator = _items.Select(i => i.Searchable)
                               .SelectMany(GetChildren)
                               .OrderByDescending(i => Similarity(i.SearchingKeys, searchingText))
                               .Select(GetParent)
                               .GetEnumerator();

        int index = 0;
        while (enumerator.MoveNext() && index < MaxResultLength)
        {
            if (results.Contains(enumerator.Current)) continue;
            results[index++] = enumerator.Current;
        }

        return results;
    }

    private static ISearchable? CreateSearchable(PropertyInfo property)
    {
        var type = property.PropertyType;

        return property.Name switch
        {
            nameof(Configs.AutoHeal) => new AutoHealCheckBox(property),
            _ when type.IsEnum => new EnumSearch(property),
            _ when type == typeof(bool) => new CheckBoxSearchNoCondition(property),
            _ when type == typeof(ConditionBoolean) => new CheckBoxSearchCondition(property),
            _ when type == typeof(float) => new DragFloatSearch(property),
            _ when type == typeof(int) => new DragIntSearch(property),
            _ when type == typeof(Vector2) => new DragFloatRangeSearch(property),
            _ when type == typeof(Vector2Int) => new DragIntRangeSearch(property),
            _ when type == typeof(Vector4) => new ColorEditSearch(property),
            _ => null
        };
    }

    private static IEnumerable<ISearchable> GetChildren(ISearchable searchable)
    {
        var myself = new ISearchable[] { searchable };
        if (searchable is CheckBoxSearch c && c.Children != null)
        {
            return c.Children.SelectMany(GetChildren).Union(myself);
        }
        return myself;
    }

    private static ISearchable GetParent(ISearchable searchable)
    {
        return searchable.Parent == null ? searchable : GetParent(searchable.Parent);
    }

    public static float Similarity(string text, string key)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var chars = text.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);
        var keys = key.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);

        var startWithCount = chars.Count(i => keys.Any(k => i.StartsWith(k, StringComparison.OrdinalIgnoreCase)));
        var containCount = chars.Count(i => keys.Any(k => i.Contains(k, StringComparison.OrdinalIgnoreCase)));

        return startWithCount * 3 + containCount;
    }
}