using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class MysticRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Mystic";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Mystic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Mystic.LoadAsset(), "TouMira.Role.Crewmate.Mystic", 1.45f),
        Icon = TouRoleIcons.Mystic,
        OptionsScreenshot = TouBanners.MysticRoleBanner,
        IntroSound = TouAudio.MediumIntroSound
    };


}