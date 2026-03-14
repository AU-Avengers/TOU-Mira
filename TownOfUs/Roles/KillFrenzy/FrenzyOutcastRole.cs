using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyOutcastRole(IntPtr cppPtr) : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.KillFrenzy;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;

    [HideFromIl2Cpp]
    Func<bool> ICustomRole.VisibleInSettings => () => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyOutcastRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount)
        {
            return false;
        }

        return wwCount >= Helpers.GetAlivePlayers().Count - wwCount;
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
    public override bool IsAffectedByComms => false;

    public string LocaleKey => "Outcast";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Neutral;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        CanUseVent = true,
        Icon = TouRoleIcons.Neutral,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    public bool HasImpostorVision => true;
}