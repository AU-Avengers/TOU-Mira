using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Roles;
using TownOfUs.GameModes;

namespace TownOfUs.Roles.TownOfPolus.Impostor;

public class PolusImpostorRole(IntPtr cppPtr) : PolusBaseImpRole(cppPtr)
{
    public override CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        HideSettings = true,
        MaxRoleCount = 0,
        DefaultRoleCount = 0,
        DefaultChance = 0,
        CanModifyChance = false,
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