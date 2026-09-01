using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class WarlockRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<VeteranRole>());
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string IdPart => "Warlock";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    [HideFromIl2Cpp]
    public bool IsModifierApplicable(BaseModifier modifier)
    {
        return modifier is not OverclockerModifier;
    }

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Warlock.LoadAsset(), "TouMira.Role.Impostor.Warlock", 1.45f),
        UseVanillaKillButton = false,
        IntroSound = TouAudio.WarlockIntroSound,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Warlock
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}BurstKill", "Burst Kill"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Kill.WikiDescription"),
                    TouAssets.KillSprite)
            ];
        }
    }
}