﻿using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class DiseasedModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Diseased";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Diseased;
    public override string GetDescription() => "Increase your killer's kill cooldown.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
    public string GetAdvancedDescription()
    {
        return
            $"After you die, your killer's kill cooldown is multiplied by a factor of {OptionGroupSingleton<DiseasedOptions>.Instance.CooldownMultiplier}x.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}
