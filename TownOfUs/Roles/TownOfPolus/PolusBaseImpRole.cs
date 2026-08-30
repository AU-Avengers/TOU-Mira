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
    public string IdPrefix => "TownOfUsMira.TownOfPolus.Role";
    public virtual string IdPart => "Impostor";
    public virtual string RoleName => MiraLocaleManager.Get("ImpostorKeyword");
    public virtual string RoleDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.ImpDescription");
    public virtual string RoleDescriptionDead => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.ImpDescriptionDead");
    public virtual string RoleLongDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.ImpDescription");

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        var text =
            $"{RoleColor.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.TabText").Replace("<roleName>", RoleName).Replace("<description>", RoleLongDescription)}</color>" +
            "\n<color=#FFFFFF>" + MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.FakeTaskTabText") + "</color>";
        orCreateTask.Text = text;
        orCreateTask.name = "TownOfUsMira.TownOfPolus.Role.Text";
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

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }
}