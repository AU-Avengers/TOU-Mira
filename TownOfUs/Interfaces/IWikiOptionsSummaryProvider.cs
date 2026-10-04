using MiraAPI.GameOptions;

namespace TownOfUs.Interfaces;

/// <summary>
/// Allows an option group to compress noisy option blocks in the wiki "Options" section.
/// Implementers can hide specific option keys and replace them with one or more summary lines.
/// </summary>
public interface IWikiOptionsSummaryProvider : ISummarizedOptions
{
}