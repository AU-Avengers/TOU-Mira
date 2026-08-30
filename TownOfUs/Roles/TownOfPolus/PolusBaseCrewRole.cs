using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Patches.Stubs;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus;

[MiraIgnore]
public abstract class PolusBaseCrewRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => TouRoleGroups.TownOfPolusCrewmate;
    public string IdPrefix => "TownOfUsMira.TownOfPolus.Role";
    public virtual string IdPart => "Crewmate";
    public virtual string RoleName => MiraLocaleManager.Get("CrewmateKeyword");
    public virtual string RoleDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.CrewDescription");
    public virtual string RoleDescriptionDead => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.CrewDescriptionDead");
    public virtual string RoleLongDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.CrewDescription");
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.TabText").Replace("<roleName>", RoleName).Replace("<description>", RoleLongDescription)}</color>";
        orCreateTask.name = "TownOfUsMira.TownOfPolus.Role.Text";
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public virtual Color RoleColor => Palette.CrewmateBlue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.Crewmate;

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostCrewRole>(),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconCrewmate.LoadAsset(), "TownOfPolus.Role.Crewmate.Crewmate", 1.45f),
        Icon = PolusGgAssets.IconCrewmate
    };
    public override bool IsDead => false; // needed because we inherit from RoleBehaviour
    public override bool IsAffectedByComms => false;

#pragma warning disable S927 // Parameter names should match base declaration and other partial definitions
#pragma warning disable CA1725 // Parameter names should match base declaration
    public override bool CanUse(IUsable usable)
#pragma warning restore CA1725 // Parameter names should match base declaration
#pragma warning restore S927 // Parameter names should match base declaration and other partial definitions
    {
        return GameManager.Instance.LogicUsables.CanUse(usable, Player);
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }
}