using System.Text;
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
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MirrorcasterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public void InitialSetup()
    {
        TmpSpriteUtils.CreateSpriteAsset(TouCrewAssets.MagicMirrorSprite.LoadAsset(),
            "TouMira.Role.Crewmate.Mirrorcaster.Ui.MagicMirror", 1.45f);
    }
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public PlayerControl? Protected { get; set; }
    public bool IsProtecting { get; set; }
    public int UnleashesAvailable { get; set; }
    [HideFromIl2Cpp] public RoleBehaviour? ContainedRole { get; set; }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not MirrorcasterRole)
        {
            return;
        }

        var dced = IsProtecting && Protected == null;
        if (Protected != null && Protected.HasDied() || dced)
        {
            Clear(dced);
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Mirrorcaster";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}MagicMirror", "Magic Mirror"),
                    MiraLocaleManager.Get($"TouRole{IdPart}MagicMirrorWikiDescription"),
                    TouCrewAssets.MagicMirrorSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Unleash", "Unleash"),
                    MiraLocaleManager.Get($"TouRole{IdPart}UnleashWikiDescription"),
                    TouCrewAssets.UnleashSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Mirrorcaster;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Mirrorcaster.LoadAsset(), "TouMira.Role.Crewmate.Mirrorcaster", 1.45f),
        IntroSound = TouAudio.MirrorcasterIntro,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        Icon = TouRoleIcons.Mirrorcaster
    };

    public bool IsPowerCrew =>
        UnleashesAvailable > 0 ||
        ModifierUtils.GetActiveModifiers<MagicMirrorModifier>()
            .HasAny(); // Always disable end game checks if there is an Unleash available

    public static string ProtectionString = MiraLocaleManager.Get("TouRoleMirrorcasterTabProtecting");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProtectionString = MiraLocaleManager.Get("TouRoleMirrorcasterTabProtecting");
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Protected != null)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b><sprite name=\"TouMira.Role.Crewmate.Mirrorcaster.Ui.MagicMirror\">{ProtectionString.Replace("<player>", Protected.Data.PlayerName)}</b>");
        }

        return stringB;
    }

    public void Clear(bool playerLeft = false)
    {
        if (playerLeft)
        {
            IsProtecting = false;
            Protected = null;
            if (Player.AmOwner)
            {
                var button = CustomButtonSingleton<MirrorcasterMagicMirrorButton>.Instance;
                button.TargetWasValid = false;
                button.ResetCooldownAndOrEffect();
            }
        }
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
        IsProtecting = false;
        if (Protected == player && player != null)
        {
            if (player.TryGetModifier<MagicMirrorModifier>(out var mod2))
            {
                player.RemoveModifier(mod2);
            }

            return;
        }

        if (Protected?.TryGetModifier<MagicMirrorModifier>(out var mod) == true)
        {
            Protected.RemoveModifier(mod);
        }

        Protected = (player?.HasDied() == true) ? null : player;
        if (Protected != null)
        {
            IsProtecting = true;
            Protected.AddModifier<MagicMirrorModifier>(Player);
        }
    }

    public static void DangerAnim(bool localMirrorcaster = false)
    {
        Coroutines.Start(MiscUtils.CoFlash(OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields && !localMirrorcaster ? TownOfUsColors.NeutralWiki : new Color32(144, 162, 195, 255)));
        if (localMirrorcaster)
        {
            TouAudio.PlaySound(TouAudio.MirrorcasterShatter);
            HudManager.Instance.StartCoroutine(HudManager.Instance.PlayerCam.CoShakeScreen(0.4f, 3f));
        }
    }

    [MethodRpc((uint)TownOfUsRpc.MagicMirror)]
    public static void RpcMagicMirror(PlayerControl mc, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(mc);
            return;
        }
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Error("RpcMagicMirror - Invalid mirrorcaster");
            return;
        }

        role?.SetProtectedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearMagicMirror)]
    public static void RpcClearMagicMirror(PlayerControl mc)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(mc);
            return;
        }
        ClearMagicMirror(mc);
    }

    [MethodRpc((uint)TownOfUsRpc.MirrorcasterUnleash)]
    public static void RpcMirrorcasterUnleash(PlayerControl mc)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(mc);
            return;
        }
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Error("ClearMagicMirror - Invalid mirrorcaster");
            return;
        }

        role.UnleashesAvailable--;
    }

    public static void ClearMagicMirror(PlayerControl mc)
    {
        if (mc.Data.Role is not MirrorcasterRole role)
        {
            Error("ClearMagicMirror - Invalid mirrorcaster");
            return;
        }

        role?.SetProtectedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.MagicMirrorAttacked)]
    public static void RpcMagicMirrorAttacked(PlayerControl source, PlayerControl mirrorcaster,
        PlayerControl protectedPlayer)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        if (mirrorcaster.Data.Role is not MirrorcasterRole role)
        {
            Error("RpcMagicMirrorAttacked - Invalid mirrorcaster");
            return;
        }

        role.SetProtectedPlayer(null);
        role.UnleashesAvailable++;

        var killerRole = source.GetRoleWhenAlive();
        if (killerRole is MirrorcasterRole mirrorcaster2)
        {
            role.ContainedRole = mirrorcaster2.ContainedRole;
            mirrorcaster2.ContainedRole = null;
        }

        if (source.Data.Role is IGhostRole)
        {
            killerRole = source.Data.Role;
        }

        role.ContainedRole = killerRole;

        var opt = OptionGroupSingleton<MirrorcasterOptions>.Instance;
        var attackInfo = (MirrorAttackInfo)opt.AttackInformationGiven.Value;
        if (mirrorcaster.AmOwner)
        {
            CustomButtonSingleton<MirrorcasterMagicMirrorButton>.Instance.ResetCooldownAndOrEffect();
            CustomButtonSingleton<MirrorcasterUnleashButton>.Instance.ResetCooldownAndOrEffect();
            DangerAnim(true);
            var text = MiraLocaleManager.Get("TouRoleMirrorcasterAttackedMessageWithoutType")
                .Replace("<player>", protectedPlayer.Data.PlayerName);
            switch (attackInfo)
            {
                case MirrorAttackInfo.Role:
                    if (role.ContainedRole != null)
                    {
                        text = MiraLocaleManager.Get("TouRoleMirrorcasterAttackedMessageWithType")
                            .Replace("<player>", protectedPlayer.Data.PlayerName)
                            .Replace("<attackerRole>", role.ContainedRole.GetRoleName());
                    }
                    break;
                case MirrorAttackInfo.Faction:
                    var faction = MiraLocaleManager.Get("CrewmateKeyword");
                    if (source.IsNeutral())
                    {
                        faction = MiraLocaleManager.Get("NeutralKeyword");
                    }
                    else if (source.IsImpostor())
                    {
                        faction = MiraLocaleManager.Get("ImpKeyword");
                    }
                    text = MiraLocaleManager.Get("TouRoleMirrorcasterAttackedMessageWithFaction")
                        .Replace("<player>", protectedPlayer.Data.PlayerName)
                        .Replace("<faction>", MiscUtils.GetColoredFactionString(faction));
                    break;
                case MirrorAttackInfo.Subalignment:
                    if (role.ContainedRole != null)
                    {
                        text = MiraLocaleManager.Get("TouRoleMirrorcasterAttackedMessageWithSubalignment")
                            .Replace("<player>", protectedPlayer.Data.PlayerName)
                            .Replace("<subalignment>", MiscUtils.GetParsedRoleAlignment(role.ContainedRole, true));
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