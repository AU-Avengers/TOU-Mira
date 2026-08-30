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
                new(MiraLocaleManager.Get($"TouRole{IdPart}Swoop", "Swoop"),
                    MiraLocaleManager.Get($"TouRole{IdPart}SwoopWikiDescription"),
                    TouImpAssets.SwoopSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Unswoop", "Unswoop"),
                    MiraLocaleManager.Get($"TouRole{IdPart}UnswoopWikiDescription"),
                    TouImpAssets.UnswoopSprite)
            ];
        }
    }
}