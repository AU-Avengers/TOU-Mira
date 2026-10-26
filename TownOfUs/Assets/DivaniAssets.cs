using UnityEngine;

namespace TownOfUs.Assets;

public static class DivaniAssets
{
    internal const string ShortPath = "TownOfUs.Resources.DivaniSpecial";

    public static LoadableAsset<Sprite> DuelistIcon { get; } =
        new LoadableResourceAsset($"{ShortPath}.Duelist.png", TownOfUsPlugin.TouAssembly, 200);

    public static LoadableAsset<Sprite> DuelistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelistBanner.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAudioResourceAsset DuelistIntroSound { get; } =
        new ($"{ShortPath}.DuelistIntro.wav", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DemolitionistIcon { get; } =
        new LoadableResourceAsset($"{ShortPath}.Demolitionist.png", TownOfUsPlugin.TouAssembly, 200);

    public static LoadableAsset<Sprite> DuelStrikeButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelStrikeButton.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DuelistDuelButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DuelistDuel.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DemolitionistBanner { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistBanner.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DemolitionistDefuseButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistDefuse.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAudioResourceAsset DemolitionistExplosionSound { get; } =
        new ($"{ShortPath}.DemolitionistExplosion.wav", TownOfUsPlugin.TouAssembly);

    public static LoadableAudioResourceAsset DemolitionistIntroSound { get; } =
        new ($"{ShortPath}.DemolitionistIntro.wav", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DemolitionistPlantButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistPlant.png", TownOfUsPlugin.TouAssembly);

    public static LoadableAsset<Sprite> DemolitionistVentButton { get; } =
        new LoadableResourceAsset($"{ShortPath}.DemolitionistVent.png", TownOfUsPlugin.TouAssembly);
}
