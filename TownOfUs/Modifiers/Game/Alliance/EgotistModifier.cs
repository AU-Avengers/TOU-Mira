using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class EgotistModifier : AllianceGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Egotist,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Egotist.LoadAsset(),
            "TouMira.Modifier.Alliance.Egotist", 1.45f));
    public bool LeaveMessageSent { get; set; }
    public bool HasSurvived { get; set; } = true;
    public static float CooldownReduction { get; set; }
    public static float SpeedMultiplier { get; set; } = 1f;
    public override string IdPart => "Egotist";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public string ShortName => MiraLocaleManager.Get($"TouModifier{IdPart}ShortName");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription")
            .Replace("<symbol>", "<color=#669966>#</color>") + MiscUtils.AppendOptionsText(GetType());
    }

    public override string Symbol => "#";
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
        CooldownReduction = 0f;
        SpeedMultiplier = 1f;
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }

        if (Player.HasModifier<ToBecomeTraitorModifier>())
        {
            Player.RemoveModifier<ToBecomeTraitorModifier>();
        }
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance.Value;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public static bool EgoVisibilityFlag(PlayerControl player)
    {
        return player.Data != null && !player.Data.Disconnected &&
               (PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling));
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && !role.Player.HasModifier<ToBecomeTraitorModifier>() &&
            (role is not ILoyalCrewmate loyalCrew || loyalCrew.CanBeEgotist);
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return (!OptionGroupSingleton<EgotistOptions>.Instance.EgotistMustSurvive.Value || HasSurvived || !Player.HasDied()) && !(reason is GameOverReason.CrewmatesByVote or GameOverReason.CrewmatesByTask
            or GameOverReason.ImpostorDisconnect || reason == CustomGameOver.GameOverReason<DrawGameOver>());
    }
}