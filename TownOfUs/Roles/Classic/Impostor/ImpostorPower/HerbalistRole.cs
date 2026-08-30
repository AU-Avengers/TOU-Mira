using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class HerbalistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Herbalist";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not HerbalistRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        var herbs = CustomButtonSingleton<HerbalistAbilityHerbButton>.Instance;
        var herbsActive = herbs.Button!.isActiveAndEnabled;
        var kill = CustomButtonSingleton<HerbalistAbilityKillButton>.Instance;
        var killActive = kill.Button!.isActiveAndEnabled;
        if (!herbs.TimerPaused && herbsActive && !killActive)
        {
            kill.UpdateCooldownHandler(Player);
        }
        else if (!kill.TimerPaused && !herbsActive && killActive)
        {
            herbs.UpdateCooldownHandler(Player);
        }

        herbs.UpdateMiniAbilityCooldown(kill.Timer);
        kill.UpdateMiniAbilityCooldown(herbs.Timer);

    }
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            var herbs = CustomButtonSingleton<HerbalistAbilityHerbButton>.Instance;
            var opts = OptionGroupSingleton<HerbalistOptions>.Instance;
            if (herbs.ExposeUsesLeft == -2)
            {
                herbs.ExposeUsesLeft = (int)opts.MaxExposeUses.Value == 0 ? -1 : (int)opts.MaxExposeUses.Value;
            }
            if (herbs.ConfuseUsesLeft == -2)
            {
                herbs.ConfuseUsesLeft = (int)opts.MaxConfuseUses.Value == 0 ? -1 : (int)opts.MaxConfuseUses.Value;
            }
            if (herbs.ProtectUsesLeft == -2)
            {
                herbs.ProtectUsesLeft = (int)opts.MaxProtectUses.Value == 0 ? -1 : (int)opts.MaxProtectUses.Value;
            }
        }
    }

    public void LobbyStart()
    {
        var herbs = CustomButtonSingleton<HerbalistAbilityHerbButton>.Instance;
        herbs.ExposeUsesLeft = -2;
        herbs.ConfuseUsesLeft = -2;
        herbs.ProtectUsesLeft = -2;
    }
    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Herbalist.LoadAsset(), "TouMira.Role.Impostor.Herbalist", 1.45f),
        UseVanillaKillButton = false,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Herbalist,
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Expose", "Expose"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Expose.WikiDescription"),
            TouImpAssets.HerbExposeSprite),
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Confuse", "Confuse"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Confuse.WikiDescription"),
            TouImpAssets.HerbConfuseSprite),
        /*new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Glamour", "Glamour"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Glamour.WikiDescription"),
            TouImpAssets.FlashSprite),*/
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Protect", "Protect"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Protect.WikiDescription"),
            TouImpAssets.HerbProtectSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.HerbalistBarrierAttacked)]
    public static void RpcHerbalistBarrierAttacked(PlayerControl cleric, PlayerControl source, PlayerControl shielded)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(cleric);
            return;
        }
        if (cleric.Data.Role is not HerbalistRole)
        {
            Error("RpcHerbalistBarrierAttacked - Invalid herbalist");
            return;
        }

        if (source.AmOwner ||
            (cleric.AmOwner &&
             OptionGroupSingleton<HerbalistOptions>.Instance.AttackNotif))
        {
            Coroutines.Start(MiscUtils.CoFlash(OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields && !cleric.AmOwner ? TownOfUsColors.NeutralWiki : TownOfUsColors.Cleric));
        }
    }
}

public enum HerbAbilities
{
    Kill,
    Expose,
    Confuse,
    // Glamour, // Scrapped because otherwise this role will be too clunky
    Protect
}