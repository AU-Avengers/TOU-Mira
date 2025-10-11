using System.Globalization;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class WardenRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    public PlayerControl? Fortified { get; set; }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not WardenRole)
        {
            return;
        }

        if (Fortified != null && Fortified.HasDied())
        {
            Clear();
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string LocaleKey => "Warden";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Fortify", "Fortify"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}FortifyWikiDescription"),
                    TouCrewAssets.FortifySprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Warden;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.SpyIntroSound,
        Icon = TouRoleIcons.Warden
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Fortified != null)
        {
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Fortified: </b>{Color.white.ToTextColor()}{Fortified.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public void Clear()
    {
        SetFortifiedPlayer(null);
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void SetFortifiedPlayer(PlayerControl? player)
    {
        Fortified?.RemoveModifier<WardenFortifiedModifier>();

        Fortified = player;

        Fortified?.AddModifier<WardenFortifiedModifier>(Player);
    }

    [MethodRpc((uint)TownOfUsRpc.WardenFortify)]
    public static void RpcWardenFortify(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearWardenFortify)]
    public static void RpcClearWardenFortify(PlayerControl player)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcClearWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.WardenNotify)]
    public static void RpcWardenNotify(PlayerControl player, PlayerControl source, PlayerControl target)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcWardenNotify - Invalid warden");
            return;
        }

        // Logger<TownOfUsPlugin>.Error("RpcWardenNotify");
        if (player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Warden));
        }

        if (source.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Warden));
        }
    }
}