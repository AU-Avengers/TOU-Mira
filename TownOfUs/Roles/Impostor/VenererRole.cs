using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class VenererRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Venerer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Venerer
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Camouflage", "Camouflage"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}CamouflageWikiDescription"),
                    TouImpAssets.CamouflageSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Sprint", "Sprint"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}SprintWikiDescription"),
                    TouImpAssets.SprintSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Freeze", "Freeze"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}FreezeWikiDescription"),
                    TouImpAssets.FreezeSprite)
            };
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