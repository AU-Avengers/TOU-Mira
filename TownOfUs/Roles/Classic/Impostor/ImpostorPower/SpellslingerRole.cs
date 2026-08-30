using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Roles.Impostor;

public sealed class SpellslingerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ClericRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Spellslinger";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");
    public static bool SabotageTriggered { get; internal set; }

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Hex", "Hex"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Hex.WikiDescription"),
            TouImpAssets.HexSprite),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}HexBomb", "Hex Bomb"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}HexBomb.WikiDescription"),
            TouImpAssets.HexBombSprite)
    ];

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Spellslinger.LoadAsset(), "TouMira.Role.Impostor.Spellslinger", 1.45f),
        Icon = TouRoleIcons.Spellslinger,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        MaxRoleCount = 1,
        IntroSound = TouAudio.ArsoIgniteSound,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        HexBombSabotageSystem.BombFinished = false;
        SabotageTriggered = false;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        HexBombSabotageSystem.BombFinished = false;
        SabotageTriggered = false;
    }
    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        if (SabotageTriggered)
        {
            GenerateReport();
        }
        SabotageTriggered = false;
    }
    private void GenerateReport()
    {
        var reportBuilder = new StringBuilder();

        if (!Player)
        {
            return;
        }
        var sabotage = ShipStatus.Instance.Systems[(SystemTypes)HexBombSabotageSystem.SabotageId]
            .Cast<HexBombSabotageSystem>();
        if (!sabotage.IsActive)
        {
            return;
        }

        var text = MiraLocaleManager.Get("TownOfUsMira.Role.SpellslingerGlobalWarning").Replace("<role>", $"#{RoleName.ToLowerInvariant().Replace(" ", "-")}");

        reportBuilder.Append(TownOfUsPlugin.Culture,
            $"{text.Replace("<time>", $"{(int)sabotage.TimeRemaining + 1}")}");

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title =
                $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.SpellslingerMessageTitle")}</color>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, report, false, true);
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !GameHistory.IsFullyDead(x)).ToList();

        var hexed = alivePlayers
            .Where(p => p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        var unhexedNonImpostors = alivePlayers
            .Where(p => !p.IsImpostorAligned() && !p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (EveryoneHexed())
        {
            stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{MiraLocaleManager.Get("TownOfUsMira.Role.SpellslingerTabHexFinished")}</b>");
        }
        else
        {
            if (hexed.Count > 0)
            {
                stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{MiraLocaleManager.Get("TownOfUsMira.Role.SpellslingerTabHexedInfo")}</b>");
                foreach (var player in hexed)
                {
                    var color = player.IsImpostorAligned() ? "red" : "white";
                    stringB.Append(TownOfUsPlugin.Culture, $"\n<color={color}><size=75%>{player.Data.PlayerName}</size></color>");
                }
            }

            stringB.Append(TownOfUsPlugin.Culture, $"\n\n<b>{MiraLocaleManager.Get("TownOfUsMira.Role.SpellslingerTabHexCounter").Replace("<count>", $"{unhexedNonImpostors.Count}")}</b>");
        }

        return stringB;
    }

    public static bool EveryoneHexed()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => p.Data.Role is not SpellslingerRole && !p.HasDied() && (!p.IsImpostorAligned() || OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode))
            .All(p => p.HasModifier<SpellslingerHexedModifier>());
    }

}