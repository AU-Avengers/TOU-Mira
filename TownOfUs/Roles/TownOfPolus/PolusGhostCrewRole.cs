using Il2CppSystem.Text;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.GameModes;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus;

public abstract class PolusGhostCrewRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), ITownOfUsRole
{
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => TouRoleGroups.TownOfPolusCrewmate;
    public virtual string LocaleKey => "Crewmate";
    public virtual string RoleName => Player != null ? Player.GetRoleWhenAlive().GetRoleName() : TouLocale.Get("CrewmateKeyword");
    public virtual string RoleDescription => Player != null ? Player.GetRoleWhenAlive().Blurb : TouLocale.GetParsed("TownOfPolusRoleCrewDescriptionDead");

    public virtual string RoleLongDescription
    {
        get
        {
            if (Player == null)
            {
                return TouLocale.GetParsed("TownOfPolusRoleCrewDescriptionDead");
            }

            var role = Player.GetRoleWhenAlive();
            return role is PolusBaseCrewRole polusRole ? polusRole.RoleDescriptionDead : role.BlurbLong;
        }
    }
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{RoleColor.ToTextColor()}{TouLocale.GetParsed("TownOfPolusRoleTabText").Replace("<roleName>", RoleName).Replace("<description>", "<color=#FF0000>" + RoleLongDescription + "</color>")}</color>";
        orCreateTask.name = "TownOfPolusRoleText";
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public virtual Color RoleColor => Player != null ? Player.GetRoleWhenAlive().TeamColor : Palette.CrewmateBlue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.Crewmate;

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        HideSettings = true,
        MaxRoleCount = 0,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanModifyChance = false,
        ShowInFreeplay = false,
        AssociatedGameMode = typeof(TownOfPolusMode),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconCrewmate.LoadAsset(), "TownOfPolus.Role.Crewmate.Crewmate", 1.45f),
        Icon = PolusGgAssets.IconCrewmate
    };
    public override bool IsDead => true; // needed because we inherit from RoleBehaviour
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