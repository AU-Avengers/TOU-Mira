using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class PoliticianRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, ILoyalCrewmate
{
    public bool CanBeTraitor => false;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => true;
    public override bool IsAffectedByComms => false;

    public bool CanCampaign { get; set; } = true;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Politician";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Campaign", "Campaign"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Campaign.WikiDescription"),
                    TouCrewAssets.CampaignButtonSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}RevealWiki", "Reveal"),
                    MiraLocaleManager.Get(OptionGroupSingleton<PoliticianOptions>.Instance.PreventCampaign
                        ? $"TownOfUsMira.Role.{IdPart}RevealWikiDescriptionPunished"
                        : $"TownOfUsMira.Role.{IdPart}Reveal.WikiDescription"),
                    TouAssets.RevealCleanSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Politician;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public bool IsPowerCrew => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Politician.LoadAsset(), "TouMira.Role.Crewmate.Politician", 1.45f),
        Icon = TouRoleIcons.Politician,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.PoliticianIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            stringB.AppendLine(TownOfUsPlugin.Culture,
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianEgotistTabInfo")}</b>");
        }

        return stringB;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        CanCampaign = true;
    }

    public void AttemptReveal()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        // All living crewmates excluding the Politician
        var aliveCrew = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && x.IsCrewmate() && x.Data.Role is not PoliticianRole).ToList();
        // All living crewmates excluding the Politician that are campaigned
        var aliveCampaigned = aliveCrew.Count(x => x.HasModifier<PoliticianCampaignedModifier>(y => y.Politician.AmOwner));
        var hasMajority =
            aliveCampaigned >= (aliveCrew.Count / 2f);
        if (aliveCrew.Count == 0)
        {
            hasMajority = true; // if all crew are dead, politician can reveal
        }

        if (hasMajority)
        {
            Player.RpcChangeRole(RoleId.Get<MayorRole>());
        }
        else
        {
            var text = MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianFailedRevealCanCampaign");
            if (OptionGroupSingleton<PoliticianOptions>.Instance.PreventCampaign)
            {
                CanCampaign = false;
                text = MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianFailedRevealCannotCampaign");
            }

            var title = $"<color=#{TownOfUsColors.Mayor.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.PoliticianFeedbackText")}</color>";
            MiscUtils.AddFakeChat(Player.Data, title, text, false, true);
        }
    }
}