using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
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

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        Icon = TouRoleIcons.Herbalist,
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Expose", "Expose"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ExposeWikiDescription"),
            TouImpAssets.BlackmailSprite),
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Confuse", "Confuse"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ConfuseWikiDescription"),
            TouImpAssets.HypnotiseButtonSprite),
        /*new(TouLocale.GetParsed($"TouRole{LocaleKey}Glamour", "Glamour"),
            TouLocale.GetParsed($"TouRole{LocaleKey}GlamourWikiDescription"),
            TouImpAssets.FlashSprite),*/
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Protect", "Protect"),
            TouLocale.GetParsed($"TouRole{LocaleKey}ProtectWikiDescription"),
            TouCrewAssets.BarrierSprite)
    ];
}

public enum HerbAbilities
{
    Kill,
    Expose,
    Confuse,
    // Glamour,
    Protect
}