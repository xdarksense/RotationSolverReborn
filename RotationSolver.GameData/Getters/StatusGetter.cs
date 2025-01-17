using Lumina.Excel.GeneratedSheets;
using System.Text;

namespace RotationSolver.GameData.Getters;

/// <summary>
/// Class for getting and processing Status Excel rows.
/// </summary>
internal class StatusGetter(Lumina.GameData gameData)
    : ExcelRowGetter<Status>(gameData)
{
    private readonly HashSet<string> _addedNames = new();

    /// <summary>
    /// Called before creating the list of items. Clears the added names set.
    /// </summary>
    protected override void BeforeCreating()
    {
        _addedNames.Clear();
        base.BeforeCreating();
    }

    /// <summary>
    /// Determines whether the specified status should be added to the list.
    /// </summary>
    /// <param name="item">The status item to check.</param>
    /// <returns>True if the status should be added; otherwise, false.</returns>
    protected override bool AddToList(Status item)
    {
        var name = item.Name.RawString;
        if (string.IsNullOrEmpty(name))
        {
            // Allow statuses without a name
            return true;
        }

        // Perform usual checks for statuses with a name
        return item.ClassJobCategory.Row != 0 &&
               name.All(char.IsAscii) &&
               item.Icon != 0;
    }

    /// <summary>
    /// Converts the specified status to its code representation.
    /// </summary>
    /// <param name="item">The status item to convert.</param>
    /// <returns>The code representation of the status.</returns>
    protected override string ToCode(Status item)
    {
        var name = item.Name.RawString;
        if (string.IsNullOrEmpty(name))
        {
            name = $"UnnamedStatus_{item.RowId}";
        }
        else
        {
            name = name.ToPascalCase();
        }

        if (!_addedNames.Add(name))
        {
            name += "_" + item.RowId.ToString();
        }

        var desc = item.Description.RawString;
        var jobs = item.ClassJobCategory.Value?.Name.RawString;
        jobs = string.IsNullOrEmpty(jobs) ? string.Empty : $" ({jobs})";

        var cate = item.StatusCategory switch
        {
            1 => " ↑",
            2 => " ↓",
            _ => string.Empty,
        };

        var sb = new StringBuilder();
        if (!name.StartsWith("UnnamedStatus"))
        {
            sb.AppendLine($"""
        /// <summary>
        /// <see href="https://garlandtools.org/db/#status/{item.RowId}"><strong>{item.Name.RawString.Replace("&", "and")}</strong></see>{cate}{jobs}
        /// <para>{desc.Replace("\n", "</para>\n/// <para>")}</para>
        /// </summary>
        """);
        }
        else
        {
            sb.AppendLine($"""
        /// <summary>
        /// <para>{desc.Replace("\n", "</para>\n/// <para>")}</para>
        /// </summary>
        """);
        }
        sb.AppendLine($"{name} = {item.RowId},");

        return sb.ToString();
    }
}