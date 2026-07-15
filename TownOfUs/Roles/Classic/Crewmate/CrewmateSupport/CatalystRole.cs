using System.Text;
using MiraAPI.Roles;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class CatalystRole : CrewmateRole, ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Catalyst";
    public string RoleName => "Catalyst";
    public string RoleDescription => "Overdrive!";
    public string RoleLongDescription => "Overcharge the crewmates by lowering cooldowns.";
    public Color RoleColor => TownOfUsColors.Catalyst;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Catalyst.LoadAsset(), "TouMira.Role.Crewmate.Catalyst", 1.45f),
        Icon = TouRoleIcons.Catalyst,
        OptionsScreenshot = TouBanners.PlaceholderRoleBanner,
    };

    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        
        stringB.AppendLine($"\n<size=40%><b>This is an Experimental role, subject to change.</b></size>");

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Support role that can make people ability cooldown decrease faster." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Overcharge",
            $"Make crew member's ability cooldown decrease faster. This effects lasts until the next meeting.",
            TouCrewAssets.OverchargeSprite),
    ];
}