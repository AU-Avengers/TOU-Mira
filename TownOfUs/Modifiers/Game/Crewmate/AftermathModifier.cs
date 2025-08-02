using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class AftermathModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Aftermath, "Aftermath");
    public override string IntroInfo => "You will also trigger your killer's abilities upon death.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Aftermath;

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public string GetAdvancedDescription()
    {
        return
            "After you die, your killer will be forced to use their abilities, targetting your body or targetting themselves.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Your killer will be forced to use their abilities!";
    }

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