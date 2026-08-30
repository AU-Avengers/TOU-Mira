using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class SwooperRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Swooper";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Swooper.LoadAsset(), "TouMira.Role.Impostor.Swooper", 1.45f),
        CanUseVent = (SwooperVent)OptionGroupSingleton<SwooperOptions>.Instance.CanVent.Value is not SwooperVent.Never,
        Icon = TouRoleIcons.Swooper,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.PhantomIntroSound
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swoop", "Swoop"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swoop.WikiDescription"),
                    TouImpAssets.SwoopSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Unswoop", "Unswoop"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Unswoop.WikiDescription"),
                    TouImpAssets.UnswoopSprite)
            ];
        }
    }
}