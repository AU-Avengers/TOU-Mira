using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class HerbalistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Herbalist";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not HerbalistRole || Player.HasDied() || !Player.AmOwner ||
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
    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        OptionsScreenshot = TouBanners.PlaceholderRoleBanner,
        Icon = TouRoleIcons.Herbalist,
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Expose", "Expose"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ExposeWikiDescription"),
            TouImpAssets.HerbExposeSprite),
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Confuse", "Confuse"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ConfuseWikiDescription"),
            TouImpAssets.HerbConfuseSprite),
        /*new(TouLocale.GetParsed($"TouRole{LocaleKey}Glamour", "Glamour"),
            TouLocale.GetParsed($"TouRole{LocaleKey}GlamourWikiDescription"),
            TouImpAssets.FlashSprite),*/
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Protect", "Protect"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ProtectWikiDescription"),
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

        if (PlayerControl.LocalPlayer.PlayerId == source.PlayerId ||
            (PlayerControl.LocalPlayer.PlayerId == cleric.PlayerId &&
             OptionGroupSingleton<HerbalistOptions>.Instance.AttackNotif))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Cleric));
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