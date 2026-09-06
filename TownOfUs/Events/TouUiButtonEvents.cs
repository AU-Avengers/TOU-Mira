using MiraAPI;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using TownOfUs.Patches;

namespace TownOfUs.Events;

public static class UiResetEvents
{
    [RegisterEvent]
    public static void ResetButtonParents(UiButtonResetEvent @event)
    {
        var wikiButton = HudManagerPatches.WikiButton;
        var zoomButton = HudManagerPatches.ZoomButton;
        if (wikiButton)
        {
            wikiButton.transform.SetParent(null);
        }
        if (zoomButton)
        {
            zoomButton.transform.SetParent(null);
        }
    }

    [RegisterEvent(-900)]
    public static void PlaceWikiButton(UiButtonPostResetEvent @event)
    {
        var wikiButton = HudManagerPatches.WikiButton;
        if (!wikiButton)
        {
            return;
        }
        var firstRow = @event.MainTopUiRow;
        var secondRow = @event.SecondTopUiRow;
        var opts = LocalSettingsTabSingleton<MiraApiSettings>.Instance;
        wikiButton.transform.SetParent(opts.WikiOnBottomRow.Value ? secondRow.transform : firstRow.transform);
    }

    [RegisterEvent(-850)]
    public static void PlaceZoomButton(UiButtonPostResetEvent @event)
    {
        var zoomButton = HudManagerPatches.ZoomButton;
        if (!zoomButton)
        {
            return;
        }
        var firstRow = @event.MainTopUiRow;
        var secondRow = @event.SecondTopUiRow;
        var opts = LocalSettingsTabSingleton<TouLocalTabButtons>.Instance;
        zoomButton.transform.SetParent(opts.ZoomOnBottomRow.Value ? secondRow.transform : firstRow.transform);
    }
}
