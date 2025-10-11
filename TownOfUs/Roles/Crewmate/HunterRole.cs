using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class HunterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public PlayerControl? LastVoted { get; set; }

    [HideFromIl2Cpp] public List<PlayerControl> CaughtPlayers { get; } = [];

    public DoomableType DoomHintType => DoomableType.Hunter;
    public string LocaleKey => "Hunter";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Stalk", "Stalk"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}StalkWikiDescription")
                        .Replace("<hunterMaxStalkUsages>",
                            $"{(int)OptionGroupSingleton<HunterOptions>.Instance.StalkUses}"),
                    TouCrewAssets.StalkButtonSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Hunter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public bool IsPowerCrew =>
        CaughtPlayers.Any(x => !x.HasDied()); // Disable end game checks if a Hunter has alive targets

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Hunter,
        IntroSound = TouAudio.OtherIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var stalkedPlayer = ModifierUtils.GetPlayersWithModifier<HunterStalkedModifier>(x => x.Hunter.AmOwner)
            .FirstOrDefault();
        var stalked = stalkedPlayer != null && !stalkedPlayer.HasDied() ? stalkedPlayer.Data.PlayerName : "Nobody";
        stringB.AppendLine(CultureInfo.InvariantCulture, $"{TouLocale.Get("TouRoleHunterStalking")}: <b>{stalked}</b>");
        if (CaughtPlayers.Count != 0)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture,
                $"<b>{TouLocale.Get("TouRoleHunterCaughtPlayersText")}</b>");
        }

        foreach (var player in CaughtPlayers)
        {
            var newText = $"<b><size=80%>{player.Data.PlayerName}</size></b>";
            stringB.AppendLine(CultureInfo.InvariantCulture, $"{newText}");
        }

        return stringB;
    }

    [MethodRpc((uint)TownOfUsRpc.CatchPlayer, SendImmediately = true)]
    public static void RpcCatchPlayer(PlayerControl hunter, PlayerControl source)
    {
        if (hunter.Data.Role is not HunterRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcCatchPlayer - Invalid hunter");
            return;
        }

        if (!role.CaughtPlayers.Contains(source))
        {
            role.CaughtPlayers.Add(source);

            if (hunter.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Hunter));

                CustomButtonSingleton<HunterStalkButton>.Instance.ResetCooldownAndOrEffect();
            }
        }
    }

    public static void Retribution(PlayerControl hunter, PlayerControl target)
    {
        if (hunter.Data.Role is not HunterRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCatchPlayer - Invalid hunter");
            return;
        }

        if (hunter.AmOwner)
        {
            hunter.RpcCustomMurder(target, resetKillTimer: false, createDeadBody: false, teleportMurderer: false,
                showKillAnim: false, playKillSound: false);
        }

        // this sound normally plays on the source only
            SoundManager.Instance.PlaySound(hunter.KillSfx, false, 0.8f);

        // this kill animations normally plays on the target only
        HudManager.Instance.KillOverlay.ShowKillAnimation(hunter.Data, target.Data);
    }
}