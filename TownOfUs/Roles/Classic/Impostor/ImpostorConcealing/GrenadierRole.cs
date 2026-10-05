using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class GrenadierRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Grenadier";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Grenadier.LoadAsset(), "TouMira.Role.Impostor.Grenadier", 1.45f),
        Icon = TouRoleIcons.Grenadier,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = OptionGroupSingleton<GrenadierOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Flash", "Flash"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Flash.WikiDescription"),
                    TouImpAssets.FlashSprite)
            ];
        }
    }
}