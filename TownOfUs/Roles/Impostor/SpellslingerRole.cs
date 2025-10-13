using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Components;
using TownOfUs.Options;

namespace TownOfUs.Roles.Impostor;

public sealed class SpellslingerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Spellslinger";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");
    public static bool SabotageTriggered { get; internal set; }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Hex", "Hex"),
            TouLocale.GetParsed($"TouRole{LocaleKey}HexWikiDescription"),
            TouImpAssets.HexSprite),
        new(TouLocale.GetParsed($"TouRole{LocaleKey}HexBomb", "Hex Bomb"),
            TouLocale.GetParsed($"TouRole{LocaleKey}HexBombWikiDescription"),
            TouImpAssets.HexSprite)
    ];

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spellslinger,
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

        if (Player == null)
        {
            return;
        }
        var sabotage = ShipStatus.Instance.Systems[(SystemTypes)HexBombSabotageSystem.SabotageId]
            .Cast<HexBombSabotageSystem>();
        if (!sabotage.IsActive)
        {
            return;
        }

        var text = TouLocale.GetParsed("TouRoleSpellslingerGlobalWarning").Replace("<role>", $"#{RoleName.ToLowerInvariant().Replace(" ", "-")}");

        reportBuilder.Append(TownOfUsPlugin.Culture,
            $"{text.Replace("<time>", $"{(int)sabotage.TimeRemaining + 1}")}");

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title =
                $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleSpellslingerMessageTitle")}</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report, false, true);
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !DeathHandlerModifier.IsFullyDead(x)).ToList();

        var hexed = alivePlayers
            .Where(p => p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        var unhexedNonImpostors = alivePlayers
            .Where(p => !p.IsImpostor() && !p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (hexed.Count > 0)
        {
            sb.Append("\n<b>Hexed Players:</b>");
            foreach (var player in hexed)
            {
                var color = player.IsImpostor() ? "red" : "white";
                sb.Append(TownOfUsPlugin.Culture, $"\n<color={color}><size=75%>{player.Data.PlayerName}</size></color>");
            }
        }

        sb.Append(TownOfUsPlugin.Culture, $"\n\n<b>Players Left to Hex: {unhexedNonImpostors.Count}</b>");
        // foreach (var player in unhexedNonImpostors)
        // {
        //     sb.Append(TownOfUsPlugin.Culture, $"\n{player.Data.PlayerName}");
        // }

        return sb;
    }

    public static bool EveryoneHexed()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => p.Data.Role is not SpellslingerRole && !p.HasDied() && (!p.IsImpostor() || OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode))
            .All(p => p.HasModifier<SpellslingerHexedModifier>());
    }

}