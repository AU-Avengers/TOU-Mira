namespace TownOfUs.Interfaces;

/// <summary>
/// Allows an option group to compress noisy option blocks in the wiki "Options" section.
/// Implementers can hide specific option keys and replace them with one or more summary lines.
/// </summary>
public interface IWikiOptionsSummaryProvider
{
    /// <summary>
    /// Option string keys (StringNames) to omit from the wiki options list (e.g., a large set of related options).
    /// </summary>
    IReadOnlySet<StringNames> WikiHiddenOptionKeys { get; }

    /// <summary>
    /// Summary lines to insert when the first hidden option would have appeared.
    /// Lines should already be formatted as "Title: Value".
    /// </summary>
    IEnumerable<string> GetWikiOptionSummaryLines();
}