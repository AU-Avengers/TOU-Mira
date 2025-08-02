using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class ScoutModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Scout, "Scout");
    public override string IntroInfo => "You can also see farther in light but very low in dark.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Scout;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateVisibility;

    public string GetAdvancedDescription()
    {
        return
            "While you can see twice as far as a regular crewmate, your vision falters when lights are off.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Your vision is higher when lights are on, but very low when lights are off.";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && Instance.Type != MapType.Fungle;
    }
}