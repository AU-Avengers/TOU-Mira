using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Assets;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class SniperRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public override bool IsAffectedByComms => false;
    public string RoleName => TouLocale.Get(TouNames.Sniper, "Sniper");
    public string RoleDescription => "Pick off your target anywhere on your screen.";
    public string RoleLongDescription => "The Sniper is an Impostor that can shoot any player visible on their screen. Limited uses.";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public int ShotsRemaining { get; set; }
    public float Cooldown { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Grenadier,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        sb.AppendLine(CultureInfo.InvariantCulture, $"<b>Shots remaining: {ShotsRemaining}</b>");
        return sb;
    }

    public string GetAdvancedDescription()
    {
        return $"{RoleName} can kill any player on their screen using a sniper shot. " +
               "Limited shots and a cooldown after firing." +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Snipe",
            "Shoot a player anywhere on your screen. Limited uses.",
            TouAssets.KillSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        base.Initialize(player);
        var sniperOpts = OptionGroupSingleton<SniperOptions>.Instance;
        ShotsRemaining = sniperOpts.MaxShots;
        Cooldown = sniperOpts.ShootCooldown;
        CustomButtonSingleton<SniperShootButton>.Instance.SetActive(true, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        base.Deinitialize(targetPlayer);
        CustomButtonSingleton<SniperShootButton>.Instance.SetActive(false, this);
    }

    [MethodRpc((uint)TownOfUsRpc.SniperShot, SendImmediately = true)]
    public static void RpcSniperShot(PlayerControl sniper, PlayerControl target)
    {
        if (sniper.Data.Role is not SniperRole role)
        {
            return;
        }

        if (role.ShotsRemaining <= 0)
        {
            return;
        }

        var sniperOpts = OptionGroupSingleton<SniperOptions>.Instance;
        role.ShotsRemaining--;

        if (target != null && !target.Data.IsDead)
        {
            sniper.RpcCustomMurder(target, resetKillTimer: false, teleportMurderer: false, playKillSound: false);
        }

        if (sniperOpts.RevealSniper)
        {
            TouAudio.PlaySound(TouAudio.GrenadeSound);
        }

        if (role.ShotsRemaining <= 0)
        {
            CustomButtonSingleton<SniperShootButton>.Instance.SetUses(0);
            CustomButtonSingleton<SniperShootButton>.Instance.SetEnabled(false);
        }

        CustomButtonSingleton<SniperShootButton>.Instance.SetTimer(role.Cooldown);
    }
}

