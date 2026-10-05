using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Text;
using MiraAPI.Roles;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus.Impostor;

public class PolusSwooperRole(IntPtr cppPtr) : PolusBaseImpRole(cppPtr), IWikiDiscoverable
{
    public override string IdPart => "Swooper";
    public override string RoleName => MiraLocaleManager.Get($"TownOfUsMira.TownOfPolus.Role.{IdPart}");
    public override string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.TownOfPolus.Role.{IdPart}.IntroBlurb");
    public override string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.TownOfPolus.Role.{IdPart}.TabDescription");
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.TownOfPolus;

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.TownOfPolus.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public override Color RoleColor => TownOfUsColors.PolusSwooper;
    public override CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostImpRole>(),
        FreeplayFolder = "Town of Polus",
        CanUseVent = false,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconImpostor.LoadAsset(), "TownOfPolus.Role.Impostor.Swooper", 1.45f),
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