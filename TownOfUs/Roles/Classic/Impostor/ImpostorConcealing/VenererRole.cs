using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class VenererRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<HunterRole>());
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Venerer";

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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Venerer.LoadAsset(), "TouMira.Role.Impostor.Venerer", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Venerer
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage", "Camouflage"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage.WikiDescription"),
                    TouImpAssets.CamouflageSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint", "Sprint"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint.WikiDescription"),
                    TouImpAssets.SprintSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze", "Freeze"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze.WikiDescription"),
                    TouImpAssets.FreezeSprite)
            ];
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        CustomButtonSingleton<VenererAbilityButton>.Instance.UpdateAbility(VenererAbility.None);
    }
}

public enum VenererAbility
{
    None,
    Camouflage,
    Sprint,
    Freeze
}