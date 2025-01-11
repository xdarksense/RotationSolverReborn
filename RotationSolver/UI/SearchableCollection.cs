using RotationSolver.Basic.Configuration;
using RotationSolver.UI.SearchableConfigs;
using RotationSolver.UI.SearchableSettings;
using System.Collections.Concurrent;

namespace RotationSolver.UI;

internal readonly record struct SearchPair(UIAttribute Attribute, ISearchable Searchable);

internal class SearchableCollection
{
    private readonly List<SearchPair> _items;
    private static readonly char[] _splitChar = { ' ', ',', '、', '.', '。' };
    private const int MaxResultLength = 20;

    public SearchableCollection()
    {
        var properties = typeof(Configs).GetRuntimeProperties().ToArray();
        var pairs = new List<SearchPair>(properties.Length);
        var parents = new Dictionary<string, CheckBoxSearch>(properties.Length);
        var attributes = new ConcurrentDictionary<PropertyInfo, UIAttribute>();

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
        var filteredItems = new Dictionary<byte, List<SearchPair>>();

        foreach (var item in _items)
        {
            if (item.Attribute.Filter == filter)
            {
                if (!filteredItems.ContainsKey(item.Attribute.Section))
                {
                    filteredItems[item.Attribute.Section] = new List<SearchPair>();
                }
                filteredItems[item.Attribute.Section].Add(item);
            }
        }

        foreach (var grp in filteredItems)
        {
            if (!isFirst)
            {
                ImGui.Separator();
            }

            foreach (var item in grp.Value.OrderBy(i => i.Attribute.Order))
            {
                item.Searchable.Draw();
            }

            isFirst = false;
        }
    }

    public ISearchable[] SearchItems(string searchingText)
    {
        if (string.IsNullOrEmpty(searchingText)) return Array.Empty<ISearchable>();

        var results = new HashSet<ISearchable>();
        var finalResults = new List<ISearchable>(MaxResultLength);

        foreach (var pair in _items)
        {
            foreach (var searchable in GetChildren(pair.Searchable))
            {
                var parent = GetParent(searchable);
                if (results.Contains(parent)) continue;

                if (Similarity(searchable.SearchingKeys, searchingText) > 0)
                {
                    results.Add(parent);
                    finalResults.Add(parent);
                    if (finalResults.Count >= MaxResultLength) break;
                }
            }
        }

        return finalResults.ToArray();
    }

    private static ISearchable? CreateSearchable(PropertyInfo property) => property.Name switch
    {
        nameof(Configs.AutoHeal) => new AutoHealCheckBox(property),
        _ when property.PropertyType.IsEnum => new EnumSearch(property),
        _ when property.PropertyType == typeof(bool) => new CheckBoxSearchNoCondition(property),
        _ when property.PropertyType == typeof(ConditionBoolean) => new CheckBoxSearchCondition(property),
        _ when property.PropertyType == typeof(float) => new DragFloatSearch(property),
        _ when property.PropertyType == typeof(int) => new DragIntSearch(property),
        _ when property.PropertyType == typeof(Vector2) => new DragFloatRangeSearch(property),
        _ when property.PropertyType == typeof(Vector2Int) => new DragIntRangeSearch(property),
        _ when property.PropertyType == typeof(Vector4) => new ColorEditSearch(property),
        _ => null
    };

    private static IEnumerable<ISearchable> GetChildren(ISearchable searchable)
    {
        yield return searchable;

        if (searchable is CheckBoxSearch c && c.Children != null)
        {
            foreach (var child in c.Children)
            {
                foreach (var grandChild in GetChildren(child))
                {
                    yield return grandChild;
                }
            }
        }
    }

    private static ISearchable GetParent(ISearchable searchable) => searchable.Parent == null ? searchable : GetParent(searchable.Parent);

    public static float Similarity(string text, string key)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var chars = text.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);
        var keys = key.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);

        var startWithCount = 0;
        var containCount = 0;

        foreach (var c in chars)
        {
            foreach (var k in keys)
            {
                if (c.StartsWith(k, StringComparison.OrdinalIgnoreCase))
                {
                    startWithCount++;
                }
                else if (c.Contains(k, StringComparison.OrdinalIgnoreCase))
                {
                    containCount++;
                }
            }
        }

        return startWithCount * 3 + containCount;
    }
}