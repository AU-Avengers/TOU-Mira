﻿using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class EgotistModifier : AllianceGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Egotist";
    public override string IntroInfo => $"Your Ego is Thriving...";
    public override string Symbol => "#";
    public override float IntroSize => 4f;
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override bool CrewContinuesGame => false;
    public override ModifierFaction FactionType => ModifierFaction.CrewmateAlliance;
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Egotist;
    public override string GetDescription() => "<size=130%>Defy the crew,</size>\n<size=125%>win with killers.</size>";
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
    public override int GetAmountPerGame() => 1;
    
    public int Priority { get; set; } = 5;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public static bool EgoVisibilityFlag(PlayerControl player)
    {
        return player.HasModifier<EgotistModifier>() && (PlayerControl.LocalPlayer.IsImpostor() || player.Is(RoleAlignment.NeutralKilling));
    }

    public string GetAdvancedDescription()
    {
        return $"The Egotist is a Crewmate Alliance modifier (signified by <color=#669966>#</color>). As the Egotist, you can only win if Crewmates lose, even when dead. If no crewmates remain after a meeting ends, you will leave in victory, but the game will continue.";
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
    public override bool? DidWin(GameOverReason reason)
    {
        return !(reason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask or GameOverReason.ImpostorDisconnect);
    }
}
