using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class TorchModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Torch, "Torch");
    public override string IntroInfo => "You can also see without lights on.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Torch;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateVisibility;

    public string GetAdvancedDescription()
    {
        return
            "The lights being off do not affect your vision.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Your vision wont get reduced\nwhen the lights are sabotaged.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && Instance.Type != MapType.Fungle;
    }
}