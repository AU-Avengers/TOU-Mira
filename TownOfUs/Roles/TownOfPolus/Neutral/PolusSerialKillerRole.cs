using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Text;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus.Neutral;

public class PolusSerialKillerRole(IntPtr cppPtr) : PolusBaseNeutRole(cppPtr), IWikiDiscoverable
{
    public override string LocaleKey => "SerialKiller";
    public override string RoleName => TouLocale.Get($"TownOfPolusRole{LocaleKey}");
    public override string RoleDescription => TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}IntroBlurb");
    public override string RoleLongDescription => TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}TabDescription");
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.TownOfPolus;
    public int KillCount;

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public override Color RoleColor => TownOfUsColors.PolusSerialKiller;
    public override CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostNeutRole>(),
        FreeplayFolder = "Town of Polus",
        CanUseVent = false,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconSerialKiller.LoadAsset(), "TownOfPolus.Role.Neutral.SerialKiller", 1.45f),
        Icon = PolusGgAssets.IconSerialKiller
    };
    public override bool WinConditionMet()
    {
        var juggCount = CustomRoleUtils.GetActiveRolesOfType<PolusSerialKillerRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > juggCount)
        {
            return false;
        }

        return juggCount >= Helpers.GetAlivePlayers().Count - juggCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool IsDead => false; // needed because we inherit from RoleBehaviour
    public override bool IsAffectedByComms => false;

    public override bool CanUse(IUsable console)
    {
        return GameManager.Instance.LogicUsables.CanUse(console, Player);
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }
}