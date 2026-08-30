using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Patches.Stubs;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus;

[MiraIgnore]
public abstract class PolusBaseNeutRole(IntPtr cppPtr) : RoleBehaviour(cppPtr), ITownOfUsRole
{
    RoleOptionsGroup ICustomRole.RoleOptionsGroup => TouRoleGroups.TownOfPolusNeutral;
    public string IdPrefix => "TownOfUsMira.TownOfPolus.Role";
    public virtual string IdPart => "Neutral";
    public virtual string RoleName => MiraLocaleManager.Get("NeutralKeyword");
    public virtual string RoleDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.NeutDescription");
    public virtual string RoleDescriptionDead => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.NeutDescriptionDead");
    public virtual string RoleLongDescription => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.NeutDescription");

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

    public virtual Color RoleColor => TownOfUsColors.Neutral;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.Neutral;
    public virtual bool WinConditionMet()
    {
        return false;
    }

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostNeutRole>(),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconNeutralAlign.LoadAsset(), "TownOfPolus.Role.Neutral.Neutral", 1.45f),
        Icon = PolusGgAssets.IconNeutralAlign
    };
    public override bool IsDead => false; // needed because we inherit from RoleBehaviour
    public override bool IsAffectedByComms => false;

    public override bool CanUse(IUsable console)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(console, Player))
        {
            return false;
        }

        var console2 = console.TryCast<Console>()!;
        return console2 == null || console2.AllowImpostor;
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }
}