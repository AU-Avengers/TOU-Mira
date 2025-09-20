using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public class NeutralGhostRole(IntPtr cppPtr) : RoleBehaviour(cppPtr), ITownOfUsRole
{
    private Minigame _hauntMenu = null!;

    public override bool IsDead => true;
    public override bool IsAffectedByComms => false;

    public void Awake()
    {
        var crewGhost = RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).Cast<CrewmateGhostRole>();
        _hauntMenu = crewGhost.HauntMenu;
        Ability = crewGhost.Ability;
    }

    public virtual string RoleName => Player != null ? Player.GetRoleWhenAlive().GetRoleName() : "Neutral Ghost";
    public virtual string RoleDescription => Player != null ? Player.GetRoleWhenAlive().Blurb : string.Empty;
    public virtual string RoleLongDescription => Player != null ? Player.GetRoleWhenAlive().BlurbLong : string.Empty;
    public virtual Color RoleColor => Player != null ? Player.GetRoleWhenAlive().TeamColor : TownOfUsColors.Neutral;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public virtual RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        HideSettings = true,
        RoleHintType = Player != null && Player.GetRoleWhenAlive() is ICustomRole custom
            ? custom.Configuration.RoleHintType
            : RoleHintType.None
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        if (Player.GetRoleWhenAlive() is ITownOfUsRole touRole)
        {
            stringB = ITownOfUsRole.SetDeadTabText(touRole);
            if (touRole.MetWinCon)
            {
                stringB.Append("<b>You have already won.</b>");
            }
            else
            {
                stringB.Append("<b>You are dead.</b>");
            }
        }
        else
        {
            stringB.Append("<b>You are dead.</b>");
        }

        return stringB;
    }

    public virtual bool WinConditionMet()
    {
        var role = Player.GetRoleWhenAlive();

        return role is ITownOfUsRole tRole && tRole.WinConditionMet();
    }

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    public override bool CanUse(IUsable console)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(console, Player))
        {
            return false;
        }

        var console2 = console.TryCast<Console>()!;
        return console2 == null || console2.AllowImpostor;
    }

    // reimplement haunt minigame
    public override void UseAbility()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return;
        }

        if (Minigame.Instance)
        {
            if (Minigame.Instance.TryCast<HauntMenuMinigame>())
            {
                Minigame.Instance.Close();
            }

            return;
        }

        var minigame = Instantiate(_hauntMenu, HudManager.Instance.AbilityButton.transform, false);
        minigame.transform.SetLocalZ(-5f);
        minigame.Begin(null);
        HudManager.Instance.AbilityButton.SetDisabled();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var role = Player.GetRoleWhenAlive();

        var win = role.DidWin(gameOverReason);

        Logger<TownOfUsPlugin>.Message($"NeutralGhostRole.DidWin - role: {role.GetRoleName()} DidWin: {win}");

        // Yes, this is bad, but we don't want to break the end game screen to allow other mods to still work with tou mira - Atony
        if (role is JesterRole && win && OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.EndsGame)
        {
            Logger<TownOfUsPlugin>.Info($"Jester - Player: {Player.Data.PlayerName}");
            Player.Data.IsDead = false;
        }

        return win;
    }
}