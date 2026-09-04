using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class DiseasedModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Diseased,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Diseased.LoadAsset(),
            "TouMira.Modifier.Crewmate.Diseased", 1.45f));
    public override string IdPart => "Diseased";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription").Replace("<cooldownMultiplier>",
            $"{OptionGroupSingleton<DiseasedOptions>.Instance.CooldownMultiplier}");
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Diseased;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}