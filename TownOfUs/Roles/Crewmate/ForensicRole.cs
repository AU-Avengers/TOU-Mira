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

public sealed class ForensicRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public CrimeSceneComponent? InvestigatingScene { get; set; }

    [HideFromIl2Cpp] public List<byte> InvestigatedPlayers { get; init; } = new();

    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Forensic";
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
        Icon = TouRoleIcons.Forensic,
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
            if (scene == null || scene.gameObject == null || !scene.gameObject)
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

    public void ExaminePlayer(PlayerControl player)
    {
        var text = TouLocale.GetParsed("TouRoleForensicAtScene").Replace("<player>", $"{TownOfUsColors.Detective.ToTextColor()}{player.Data.PlayerName}</color>");
        if (InvestigatedPlayers.Contains(player.PlayerId) && InvestigatingScene != null && InvestigatingScene.DeadPlayer != null)
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.red));

            var deadPlayer = InvestigatingScene.DeadPlayer;
            text = text.Replace("<deadPlayer>", deadPlayer.Data.PlayerName);
        }
        else
        {
            Coroutines.Start(MiscUtils.CoFlash(Color.green));
            text = TouLocale.GetParsed("TouRoleForensicNotAtScene").Replace("<player>", $"{TownOfUsColors.Detective.ToTextColor()}{player.Data.PlayerName}</color>");
        }
        var notif1 = Helpers.CreateAndShowNotification(text,
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Forensic.LoadAsset());
        notif1.AdjustNotification();
    }

    public void Report(byte deadPlayerId)
    {
        var areReportsEnabled = OptionGroupSingleton<ForensicOptions>.Instance.ForensicReportOn;

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

        var reportMsg = BodyReport.ParseForensicReport(br);

        if (string.IsNullOrWhiteSpace(reportMsg))
        {
            return;
        }

        // Send the message through chat only visible to the forensic
        var title = $"<color=#{TownOfUsColors.Detective.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleForensicMessageTitle")}</color>";
        var reported = Player;
        if (br.Body != null)
        {
            reported = br.Body;
        }

        MiscUtils.AddFakeChat(reported.Data, title, reportMsg, false, true);
    }
}