using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers;
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
        if (stalkedPlayer != null && !stalkedPlayer.HasDied() && !CaughtPlayers.Contains(stalkedPlayer))
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{TouLocale.Get("TouRoleHunterStalking")}: <b>{stalkedPlayer.Data.PlayerName}</b>");
        }
        if (CaughtPlayers.Count != 0)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture,
                $"<b>{TouLocale.Get("TouRoleHunterCaughtPlayersText")}</b>");
        }

        foreach (var player in CaughtPlayers)
        {
            var newText = $"<b><size=80%>{player.Data.PlayerName}</size></b>";
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{newText}");
        }

        return stringB;
    }

    [MethodRpc((uint)TownOfUsRpc.CatchPlayer)]
    public static void RpcCatchPlayer(PlayerControl hunter, PlayerControl source, bool playerInteraction)
    {
        if (hunter.Data.Role is not HunterRole role)
        {
            Error("RpcCatchPlayer - Invalid hunter");
            return;
        }

        if (!role.CaughtPlayers.Contains(source))
        {
            role.CaughtPlayers.Add(source);

            if (hunter.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Hunter));

                CustomButtonSingleton<HunterStalkButton>.Instance.ResetCooldownAndOrEffect();
                var text = playerInteraction && OptionGroupSingleton<HunterOptions>.Instance.SeesTypeOfInteraction.Value
                    ? TouLocale.GetParsed("TouRoleHunterCaughtInteractionNotif")
                    : TouLocale.GetParsed("TouRoleHunterCaughtAbilityNotif");

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{text.Replace("<player>", source.Data.PlayerName)}</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hunter.LoadAsset());
                notif1.AdjustNotification();
            }
        }

        if (source.TryGetModifier<HunterStalkedModifier>(out var modifier))
        {
            source.RemoveModifier(modifier);
        }
    }

    public static void Retribution(PlayerControl hunter, PlayerControl target)
    {
        if (hunter.Data.Role is not HunterRole)
        {
            Error("Retribution - Invalid hunter");
            return;
        }

        if (target.HasModifier<InvulnerabilityModifier>())
        {
            Error("Retribution - Target cannot be killed!");
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