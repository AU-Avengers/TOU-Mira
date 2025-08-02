using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class TiebreakerModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Tiebreaker, "Tiebreaker");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Tiebreaker;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public string GetAdvancedDescription()
    {
        return
            "Your vote allows you to break ties.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Your vote breaks ties";
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerAmount != 0 ? 1 : 0;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerChance;
    }
}