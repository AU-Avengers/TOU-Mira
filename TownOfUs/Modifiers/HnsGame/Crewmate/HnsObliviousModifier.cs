using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsObliviousModifier : HnsGameModifier
{
    public override string LocaleKey => "Oblivious";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Bait;
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }


    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.ObliviousChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.ObliviousAmount;
    }
}