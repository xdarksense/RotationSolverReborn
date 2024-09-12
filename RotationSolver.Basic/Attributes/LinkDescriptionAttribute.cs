namespace RotationSolver.Basic.Attributes;

/// <summary>
/// The link to an image or web page about your rotation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class LinkDescriptionAttribute : Attribute
{
    /// <summary>
    /// The link description.
    /// </summary>
    public LinkDescription LinkDescription { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="url">The URL of the link.</param>
    /// <param name="description">The description of the link.</param>
    /// <exception cref="ArgumentException">Thrown when the URL is null or empty.</exception>
    public LinkDescriptionAttribute(string url, string description = "")
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        LinkDescription = new LinkDescription { Url = url, Description = description };
    }
}

/// <summary>
/// Link description itself.
/// </summary>
public readonly record struct LinkDescription
{
    /// <summary>
    /// The description of the link.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// The URL of the link.
    /// </summary>
    public string Url { get; init; }
}