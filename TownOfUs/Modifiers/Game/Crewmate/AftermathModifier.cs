using MiraAPI.GameOptions;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class AftermathModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Aftermath,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Aftermath.LoadAsset(),
            "TouMira.Modifier.Crewmate.Aftermath", 1.45f));
    public override string IdPart => "Aftermath";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription");
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Aftermath;

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
}