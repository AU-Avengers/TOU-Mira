using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class EclipsalRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Eclipsal";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Eclipsal.LoadAsset(), "TouMira.Role.Impostor.Eclipsal", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Eclipsal
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Blind", "Blind"),
                    MiraLocaleManager.Get($"TouRole{IdPart}BlindWikiDescription"),
                    TouImpAssets.BlindSprite)
            ];
        }
    }
}