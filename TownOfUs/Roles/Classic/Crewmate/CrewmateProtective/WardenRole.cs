using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class WardenRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Fortified { get; set; }
    public bool IsProtecting { get; set; }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not WardenRole)
        {
            return;
        }

        var dced = IsProtecting && Fortified == null;
        if (Fortified != null && Fortified.HasDied() || dced)
        {
            Clear(dced);
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Warden";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

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
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Fortify", "Fortify"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Fortify.WikiDescription"),
                    TouCrewAssets.FortifySprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Warden;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Warden.LoadAsset(), "TouMira.Role.Crewmate.Warden", 1.45f),
        IntroSound = TouAudio.SpyIntroSound,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        Icon = TouRoleIcons.Warden
    };

    public static string ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.WardenTabProtecting");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.WardenTabProtecting");
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Fortified != null)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{ProtectionString.Replace("<player>", Fortified.Data.PlayerName)}</b>");
        }

        return stringB;
    }

    public void Clear(bool playerLeft = false)
    {
        if (playerLeft)
        {
            IsProtecting = false;
            Fortified = null;
            if (Player.AmOwner)
            {
                var button = CustomButtonSingleton<WardenFortifyButton>.Instance;
                button.ResetCooldownAndOrEffect();
            }
        }
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
        IsProtecting = false;
        Fortified?.RemoveModifier<WardenFortifiedModifier>();

        Fortified = player;
        if (Fortified != null)
        {
            IsProtecting = true;
            Fortified.AddModifier<WardenFortifiedModifier>(Player);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.WardenFortify)]
    public static void RpcWardenFortify(PlayerControl player, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not WardenRole)
        {
            Error("RpcWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearWardenFortify)]
    public static void RpcClearWardenFortify(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not WardenRole)
        {
            Error("RpcClearWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.WardenNotify)]
    public static void RpcWardenNotify(PlayerControl player, PlayerControl source, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not WardenRole)
        {
            Error("RpcWardenNotify - Invalid warden");
            return;
        }

        // Error("RpcWardenNotify");
        if (source.AmOwner || player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields && !player.AmOwner ? TownOfUsColors.NeutralWiki : TownOfUsColors.Warden));
        }
    }
}