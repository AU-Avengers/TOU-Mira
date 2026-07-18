using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class TiebreakerModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Tiebreaker,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Tiebreaker.LoadAsset(),
            "TouMira.Modifier.Universal.Tiebreaker", 1.45f));
    public override string LocaleKey => "Tiebreaker";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Tiebreaker;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerChance;
    }
}