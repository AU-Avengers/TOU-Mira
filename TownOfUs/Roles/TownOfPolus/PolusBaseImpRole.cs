using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Patches.Stubs;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus;

[MiraIgnore]
public abstract class PolusBaseImpRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole
{
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => TouRoleGroups.TownOfPolusImpostor;
    public virtual string LocaleKey => "Impostor";
    public virtual string RoleName => TouLocale.Get("ImpostorKeyword");
    public virtual string RoleDescription => TouLocale.GetParsed("TownOfPolusRoleImpDescription");
    public virtual string RoleDescriptionDead => TouLocale.GetParsed("TownOfPolusRoleImpDescriptionDead");
    public virtual string RoleLongDescription => TouLocale.GetParsed("TownOfPolusRoleImpDescription");

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        var text =
            $"{RoleColor.ToTextColor()}{TouLocale.GetParsed("TownOfPolusRoleTabText").Replace("<roleName>", RoleName).Replace("<description>", RoleLongDescription)}</color>" +
            "\n<color=#FFFFFF>" + TouLocale.GetParsed("TownOfPolusRoleFakeTaskTabText") + "</color>";
        orCreateTask.Text = text;
        orCreateTask.name = "TownOfPolusRoleText";
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public virtual Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.Impostor;

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostImpRole>(),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconImpostor.LoadAsset(), "TownOfPolus.Role.Impostor.Impostor", 1.45f),
        Icon = PolusGgAssets.IconImpostor
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