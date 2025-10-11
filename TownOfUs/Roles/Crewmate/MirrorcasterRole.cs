using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MirrorcasterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Protected { get; set; }
    public int UnleashesAvailable { get; set; }
    public string UnleashString { get; set; }
    [HideFromIl2Cpp] public RoleBehaviour? ContainedRole { get; set; }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not MirrorcasterRole)
        {
            return;
        }

        if (Protected != null && Protected.HasDied())
        {
            Clear();
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string LocaleKey => "Mirrorcaster";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}MagicMirror", "Magic Mirror"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MagicMirrorWikiDescription"),
                    TouCrewAssets.MagicMirrorSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Unleash", "Unleash"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}UnleashWikiDescription"),
                    TouCrewAssets.UnleashSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Mirrorcaster;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Scientist),
        Icon = TouRoleIcons.Mirrorcaster
    };

    public bool IsPowerCrew =>
        UnleashesAvailable > 0 ||
        ModifierUtils.GetActiveModifiers<MagicMirrorModifier>()
            .Any(); // Always disable end game checks if there is an Unleash available

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Protected != null)
        {
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Protecting: </b>{Color.white.ToTextColor()}{Protected.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public void Clear()
    {
        SetProtectedPlayer(null);
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

    public void SetProtectedPlayer(PlayerControl? player)
    {
        Protected?.RemoveModifier<MagicMirrorModifier>();

        Protected = (player?.HasDied() == true) ? null : player;

        Protected?.AddModifier<MagicMirrorModifier>(Player);
    }

    public static void DangerAnim()
    {
        Coroutines.Start(MiscUtils.CoFlash(new Color32(144, 162, 195, 255)));
    }

    [MethodRpc((uint)TownOfUsRpc.MagicMirror)]
    public static void RpcMagicMirror(PlayerControl mc, PlayerControl target)
    {
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcMagicMirror - Invalid mirrorcaster");
            return;
        }

        role?.SetProtectedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearMagicMirror)]
    public static void RpcClearMagicMirror(PlayerControl mc)
    {
        ClearMagicMirror(mc);
    }

    [MethodRpc((uint)TownOfUsRpc.MirrorcasterUnleash)]
    public static void RpcMirrorcasterUnleash(PlayerControl mc)
    {
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Logger<TownOfUsPlugin>.Error("ClearMagicMirror - Invalid mirrorcaster");
            return;
        }

        role.UnleashesAvailable--;
    }

    public static void ClearMagicMirror(PlayerControl mc)
    {
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Logger<TownOfUsPlugin>.Error("ClearMagicMirror - Invalid mirrorcaster");
            return;
        }

        role?.SetProtectedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.MagicMirrorAttacked)]
    public static void RpcMagicMirrorAttacked(PlayerControl mirrorcaster, PlayerControl source,
        PlayerControl protectedPlayer)
    {
        if (mirrorcaster.Data.Role is not MirrorcasterRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcMagicMirrorAttacked - Invalid mirrorcaster");
            return;
        }

        role.SetProtectedPlayer(null);
        role.UnleashesAvailable++;

        var cod = "Killed";
        var killerRole = source.GetRoleWhenAlive();
        var checkForCod = true;
        if (killerRole is MirrorcasterRole mirrorcaster2)
        {
            role.ContainedRole = mirrorcaster2.ContainedRole;
            cod = mirrorcaster2.UnleashString;
            checkForCod = cod == string.Empty || mirrorcaster2.ContainedRole == null;
            mirrorcaster2.ContainedRole = null;
            mirrorcaster2.UnleashString = string.Empty;
        }

        if (checkForCod)
        {
            switch (killerRole)
            {
                case SheriffRole:
                    cod = "Shot";
                    break;
                case DeputyRole:
                    cod = "Blasted";
                    break;
                case GlitchRole:
                    cod = "Bugged";
                    break;
                case JuggernautRole:
                    cod = "Destroyed";
                    break;
                case SoulCollectorRole:
                    cod = "Reaped";
                    break;
                case VampireRole:
                    cod = "Bitten";
                    break;
                case WerewolfRole:
                    cod = "Rampaged";
                    break;
            }

            role.ContainedRole = killerRole;
        }

        role.UnleashString = cod;

        var opt = OptionGroupSingleton<MirrorcasterOptions>.Instance;
        var attackInfo = opt.AttackInformationGiven;
        if (mirrorcaster.AmOwner)
        {
            CustomButtonSingleton<MirrorcasterMagicMirrorButton>.Instance.ResetCooldownAndOrEffect();
            CustomButtonSingleton<MirrorcasterUnleashButton>.Instance.ResetCooldownAndOrEffect();
            DangerAnim();
            var text =
                $"{protectedPlayer.Data.PlayerName} was attacked! You can now unleash the attack onto another player!";
            switch (attackInfo)
            {
                case MirrorAttackInfo.Role:
                    if (role.ContainedRole != null)
                    {
                        text =
                            $"{protectedPlayer.Data.PlayerName} was attacked by the {role.ContainedRole.GetRoleName()}! You can now unleash the attack onto another player!";
                    }
                    break;
                case MirrorAttackInfo.Faction:
                    var faction = TouLocale.Get("CrewmateKeyword");
                    if (source.IsNeutral())
                    {
                        faction = TouLocale.Get("NeutralKeyword");
                    }
                    else if (source.IsImpostor())
                    {
                        faction = TouLocale.Get("ImpKeyword");
                    }
                    text =
                        $"{protectedPlayer.Data.PlayerName} was attacked by a {MiscUtils.GetColoredFactionString(faction)}! You can now unleash the attack onto another player!";
                    break;
                case MirrorAttackInfo.Subalignment:
                    if (role.ContainedRole != null)
                    {
                        text =
                            $"{protectedPlayer.Data.PlayerName} was attacked by a {MiscUtils.GetParsedRoleAlignment(role.ContainedRole, true)}! You can now unleash the attack onto another player!";
                    }
                    break;
            }
            var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Mirrorcaster.LoadAsset());
            notif1.AdjustNotification();
        }
        else if (opt.WhoGetsNotification is MirrorOption.MirrorcasterAndKiller && source.AmOwner)
        {
            DangerAnim();
        }
    }
}