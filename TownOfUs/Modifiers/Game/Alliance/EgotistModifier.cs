using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.GameOver;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class EgotistModifier : AllianceGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.Egotist, "Egotist");
    public override string IntroInfo => "Your Ego is Thriving...";
    public override string Symbol => "#";
    public override float IntroSize => 4f;
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override bool CrewContinuesGame => false;
    public override ModifierFaction FactionType => ModifierFaction.CrewmateAlliance;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Egotist;

    public int Priority { get; set; } = 5;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];
    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
        
        if (Player.HasModifier<TraitorCacheModifier>())
        {
            Player.RemoveModifier<TraitorCacheModifier>();
        }
    }

    public string GetAdvancedDescription()
    {
        return
            "The Egotist is a Crewmate Alliance modifier (signified by <color=#669966>#</color>). As the Egotist, you can only win if Crewmates lose, even when dead. If no crewmates remain after a meeting ends, you will leave in victory, but the game will continue.";
    }

    public override string GetDescription()
    {
        return "<size=130%>Defy the crew,</size>\n<size=125%>win with killers.</size>";
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public static bool EgoVisibilityFlag(PlayerControl player)
    {
        return player.HasModifier<EgotistModifier>() &&
               (PlayerControl.LocalPlayer.IsImpostor() || player.Is(RoleAlignment.NeutralKilling));
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && !role.Player.HasModifier<ToBecomeTraitorModifier>();
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return !(reason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask
            or GameOverReason.ImpostorDisconnect || reason == CustomGameOver.GameOverReason<DrawGameOver>());
    }
}