using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class DetectiveTouRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public CrimeSceneComponent? InvestigatingScene { get; set; }

    [HideFromIl2Cpp] public List<byte> InvestigatedPlayers { get; init; } = new();

    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Detective";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Inspect", "Inspect"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}InspectWikiDescription"),
                    TouCrewAssets.InspectSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Examine", "Examine"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ExamineWikiDescription"),
                    TouCrewAssets.ExamineSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Detective;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Detective,
        IntroSound = TouAudio.QuestionSound
    };

    public void LobbyStart()
    {
        InvestigatingScene = null;
        InvestigatedPlayers.Clear();

        CrimeSceneComponent.Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        InvestigatingScene = null;
        InvestigatedPlayers.Clear();
    }

    public void ExaminePlayer(PlayerControl player)
    {
        if (InvestigatedPlayers.Contains(player.PlayerId))
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));

            var deadPlayer = InvestigatingScene?.DeadPlayer!;

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Detective.ToTextColor()}{player.Data.PlayerName} was at the scene of {deadPlayer.Data.PlayerName}'s death!\nThey might be the killer or a witness.</b></color>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Detective.LoadAsset());
            notif1.AdjustNotification();
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.green));
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Detective.ToTextColor()}{player.Data.PlayerName} was not at the scene of the crime.</b></color>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Detective.LoadAsset());
            notif1.AdjustNotification();
        }
    }

    public void Report(byte deadPlayerId)
    {
        var areReportsEnabled = OptionGroupSingleton<DetectiveOptions>.Instance.DetectiveReportOn;

        if (!areReportsEnabled)
        {
            return;
        }

        var matches = GameHistory.KilledPlayers.Where(x => x.VictimId == deadPlayerId).ToArray();

        DeadPlayer? killer = null;

        if (matches.Length > 0)
        {
            killer = matches[0];
        }

        if (killer == null)
        {
            return;
        }

        var br = new BodyReport
        {
            Killer = MiscUtils.PlayerById(killer.KillerId),
            Reporter = Player,
            Body = MiscUtils.PlayerById(killer.VictimId),
            KillAge = (float)(DateTime.UtcNow - killer.KillTime).TotalMilliseconds
        };

        var reportMsg = BodyReport.ParseDetectiveReport(br);

        if (string.IsNullOrWhiteSpace(reportMsg))
        {
            return;
        }

        // Send the message through chat only visible to the detective
        var title = $"<color=#{TownOfUsColors.Detective.ToHtmlStringRGBA()}>Detective Report</color>";
        var reported = Player;
        if (br.Body != null)
        {
            reported = br.Body;
        }

        MiscUtils.AddFakeChat(reported.Data, title, reportMsg, false, true);
    }
}