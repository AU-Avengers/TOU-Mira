using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class DivaniAssets
{
    internal const string ShortPath = "TownOfUs.Resources.DivaniSpecial";

    public static LoadableAsset<Sprite> DuelistIcon { get; } =
        new LoadableResourceAsset($"{ShortPath}.Duelist.png", 200);

    public static LoadableAsset<Sprite> DuelistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelistBanner.png");

    public static LoadableAudioResourceAsset DuelistIntroSound { get; } =
        new ($"{ShortPath}.DuelistIntro.wav");

    public static LoadableAsset<Sprite> DemolitionistIcon { get; } =
        new LoadableResourceAsset($"{ShortPath}.Demolitionist.png", 200);

    public static LoadableAsset<Sprite> DuelStrikeButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelStrikeButton.png");

    public static LoadableAsset<Sprite> DuelistDuelButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelistDuel.png");

    public static LoadableAsset<Sprite> DemolitionistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistBanner.png");

    public static LoadableAsset<Sprite> DemolitionistDefuseButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistDefuse.png");

    public static LoadableAudioResourceAsset DemolitionistExplosionSound { get; } =
        new ($"{ShortPath}.DemolitionistExplosion.wav");

    public static LoadableAudioResourceAsset DemolitionistIntroSound { get; } =
        new ($"{ShortPath}.DemolitionistIntro.wav");

    public static LoadableAsset<Sprite> DemolitionistPlantButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistPlant.png");

    public static LoadableAsset<Sprite> DemolitionistVentButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistVent.png");
}
