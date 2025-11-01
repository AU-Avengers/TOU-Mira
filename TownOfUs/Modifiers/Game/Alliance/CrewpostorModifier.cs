using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class CrewpostorModifier : AllianceGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Crewpostor";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription");
    }

    public override string Symbol => "*";
    public override float IntroSize => 4f;
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override bool CrewContinuesGame => true;
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

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public static bool CrewpostorVisibilityFlag(PlayerControl player)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isImp = PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode;
        
        return player.Data != null && !player.Data.Disconnected && player.HasModifier<CrewpostorModifier>() && isImp;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && !role.Player.HasModifier<ToBecomeTraitorModifier>();
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.ImpostorsByKill || reason is GameOverReason.ImpostorsBySabotage || reason is GameOverReason.ImpostorsByVote;
    }
}