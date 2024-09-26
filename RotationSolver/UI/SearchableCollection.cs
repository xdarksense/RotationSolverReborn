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
        // Retrieve properties from the Configs class
        var properties = typeof(Configs).GetRuntimeProperties().ToArray();
        var pairs = new List<SearchPair>(properties.Length);
        var parents = new Dictionary<string, CheckBoxSearch>(properties.Length);

        // Cache attributes to avoid repeated reflection calls
        var attributes = new ConcurrentDictionary<PropertyInfo, UIAttribute>();

        // Iterate over each property
        foreach (var property in properties)
        {
            // Get the UIAttribute for the property
            var ui = property.GetCustomAttribute<UIAttribute>();
            if (ui == null) continue;

            // Create an ISearchable instance for the property
            var item = CreateSearchable(property);
            if (item == null) continue;

            // Set PvE and PvP filters
            item.PvEFilter = new(ui.PvEFilter);
            item.PvPFilter = new(ui.PvPFilter);

            // Add the SearchPair to the list
            pairs.Add(new(ui, item));

            // If the item is a CheckBoxSearch, add it to the parents dictionary
            if (item is CheckBoxSearch search)
            {
                parents[property.Name] = search;
            }
        }

        // Initialize the _items list
        _items = new List<SearchPair>(pairs.Count);

        // Organize items based on parent-child relationships
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

        // Filter and group items based on the provided filter
        var filteredItems = _items.Where(i => i.Attribute.Filter == filter)
                                  .GroupBy(i => i.Attribute.Section);

        foreach (var grp in filteredItems)
        {
            // Add a separator between groups
            if (!isFirst)
            {
                ImGui.Separator();
            }

            // Draw each item in the group, ordered by Attribute.Order
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

        var results = new HashSet<ISearchable>();
        var finalResults = new List<ISearchable>(MaxResultLength);

        foreach (var searchable in _items.Select(i => i.Searchable).SelectMany(GetChildren))
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

        return finalResults.ToArray();
    }

    private static ISearchable? CreateSearchable(PropertyInfo property)
    {
        var type = property.PropertyType;

        // Create an instance of ISearchable based on the property type
        return property.Name switch
        {
            // Special case for AutoHeal property
            nameof(Configs.AutoHeal) => new AutoHealCheckBox(property),

            // Handle enum properties
            _ when type.IsEnum => new EnumSearch(property),

            // Handle boolean properties without conditions
            _ when type == typeof(bool) => new CheckBoxSearchNoCondition(property),

            // Handle ConditionBoolean properties
            _ when type == typeof(ConditionBoolean) => new CheckBoxSearchCondition(property),

            // Handle float properties
            _ when type == typeof(float) => new DragFloatSearch(property),

            // Handle int properties
            _ when type == typeof(int) => new DragIntSearch(property),

            // Handle Vector2 properties
            _ when type == typeof(Vector2) => new DragFloatRangeSearch(property),

            // Handle Vector2Int properties
            _ when type == typeof(Vector2Int) => new DragIntRangeSearch(property),

            // Handle Vector4 properties
            _ when type == typeof(Vector4) => new ColorEditSearch(property),

            // Return null for unsupported property types
            _ => null
        };
    }

    private static IEnumerable<ISearchable> GetChildren(ISearchable searchable)
    {
        // Include the current searchable item
        yield return searchable;

        // If the searchable item is a CheckBoxSearch and has children, recursively get all children
        if (searchable is CheckBoxSearch c && c.Children != null)
        {
            foreach (var child in c.Children.SelectMany(GetChildren))
            {
                yield return child;
            }
        }
    }

    private static ISearchable GetParent(ISearchable searchable)
    {
        // Recursively get the top-most parent
        return searchable.Parent == null ? searchable : GetParent(searchable.Parent);
    }

    public static float Similarity(string text, string key)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        // Split the text and key into words using the specified delimiters
        var chars = text.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);
        var keys = key.Split(_splitChar, StringSplitOptions.RemoveEmptyEntries);

        // Count the number of words that start with or contain the key
        var startWithCount = chars.Count(i => keys.Any(k => i.StartsWith(k, StringComparison.OrdinalIgnoreCase)));
        var containCount = chars.Count(i => keys.Any(k => i.Contains(k, StringComparison.OrdinalIgnoreCase)));

        // Calculate the similarity score
        return startWithCount * 3 + containCount;
    }
}