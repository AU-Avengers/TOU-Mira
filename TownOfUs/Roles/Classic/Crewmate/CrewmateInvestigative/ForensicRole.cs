using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Achievements;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ForensicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IBodyReportChat
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public CrimeSceneComponent? InvestigatingScene { get; set; }

    [HideFromIl2Cpp] public List<byte> InvestigatedPlayers { get; init; } = [];

    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Forensic";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Inspect", "Inspect"),
                    MiraLocaleManager.Get($"MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Inspect.WikiDescription"),
                    TouCrewAssets.InspectSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Examine", "Examine"),
                    MiraLocaleManager.Get($"MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Examine.WikiDescription"),
                    TouCrewAssets.ExamineSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Forensic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Forensic, "TouMira.Role.Crewmate.Forensic", 1.45f),
        Icon = TouRoleIcons.Forensic,
        OptionsScreenshot = TouBanners.ForensicRoleBanner,
        IntroSound = TouAudio.QuestionSound
    };

    public void LobbyStart()
    {
        InvestigatingScene = null;
        InvestigatedPlayers.Clear();

        CrimeSceneComponent.Clear();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (CrimeSceneComponent._crimeScenes.Count == 0)
        {
            return;
        }

        foreach (var scene in CrimeSceneComponent._crimeScenes)
        {
            if (scene?.gameObject == null || !scene.gameObject)
            {
                continue;
            }

            scene.gameObject.SetActive(false);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        InvestigatingScene = null;
        InvestigatedPlayers.Clear();
    }

    public void ExaminePlayer(PlayerControl player, bool firstAttempt)
    {
        var text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicAtScene").Replace("<player>", $"{TownOfUsColors.Forensic.ToTextColor()}{player.Data.PlayerName}</color>");
        if (InvestigatedPlayers.Contains(player.PlayerId) && InvestigatingScene?.DeadPlayer is { Data.PlayerName: var deadName } )
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));

            text = text.Replace("<deadPlayer>", deadName);
            if (firstAttempt)
            {
                TouAchievements.BloodyHands.Unlock();
            }
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.green));
            text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicNotAtScene").Replace("<player>", $"{TownOfUsColors.Forensic.ToTextColor()}{player.Data.PlayerName}</color>");
        }
        var notif1 = Helpers.CreateAndShowNotification(text,
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Forensic);
        notif1.AdjustNotification();
    }

    public bool ReportEnabled => OptionGroupSingleton<ForensicOptions>.Instance.ForensicReportOn;

    public string BodyReportTitle => $"<color=#{TownOfUsColors.Forensic.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.ForensicMessageTitle")}</color>";

    public string ParseReport(BodyReport br)
    {
        string text;
        if (br.Killer is not { } killer)
        {
            text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyError");
        }
        else if (br.KillAge > OptionGroupSingleton<ForensicOptions>.Instance.ForensicFactionDuration * 1000 &&
            OptionGroupSingleton<ForensicOptions>.Instance.ForensicFactionDuration > 0)
        {
            text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyOld");
        }
        else if (killer.PlayerId == br.Body!.PlayerId)
        {
            text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodySuicide");
        }
        else
        {
            TouAchievements.RightOnTime.Unlock();

            if (br.KillAge < OptionGroupSingleton<ForensicOptions>.Instance.ForensicRoleDuration * 1000)
            {
                // if the killer died, they would still appear correctly here
                var role = killer.GetModifierOfType<ICachedRole>()?.CachedRole
                    ?? killer.GetRoleWhenAlive();

                text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyKillerRole").Replace("<role>",
                    MiscUtils.GetReliableHyperlinkText(role));
            }

            else if (killer.IsNeutral())
            {
                text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyKillerNeutral");
            }

            else if (killer.IsCrewmate())
            {
                text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyKillerCrewmate");
            }
            else
            {
                text = MiraLocaleManager.Get("TownOfUsMira.Role.ForensicBodyKillerImpostor");
            }
        }

        text = text.Replace("<time>", Math.Round(br.KillAge / 1000).ToString(TownOfUsPlugin.Culture));

        return text;
    }
}