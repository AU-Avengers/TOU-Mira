using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class DrunkModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Drunk,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Drunk.LoadAsset(),
            "TouMira.Modifier.Universal.Drunk", 1.45f));
    public override string IdPart => "Drunk";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Drunk;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription");
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.DrunkAmount;
    }
}