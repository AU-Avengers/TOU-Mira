using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class AftermathModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Aftermath";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Aftermath;
    public override string GetDescription() => "Your killer will be forced to use their abilities!";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
    public string GetAdvancedDescription()
    {
        return
            "After you die, your killer will be forced to use their abilities, targetting your body or targetting themselves.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}
