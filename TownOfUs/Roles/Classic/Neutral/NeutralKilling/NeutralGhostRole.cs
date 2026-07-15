using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public class NeutralGhostRole : RoleBehaviour, ITownOfUsRole
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = TouLocale.GetParsed("NeutralBasicDeadTaskHeader");
        orCreateTask.name = "NeutralRoleText";
    }
    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }
    private Minigame _hauntMenu = null!;
    public bool IsDraftable => false;
    public override bool IsDead => true;
    public override bool IsAffectedByComms => false;

    public void Awake()
    {
        var crewGhost = (RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost) as CrewmateGhostRole)!;
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

    public override void AppendTaskHint(System.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    public override bool CanUse(IUsable console)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(console, Player))
        {
            return false;
        }

        var console2 = (console as Console)!;
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
            if (Minigame.Instance is HauntMenuMinigame)
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

        Message($"NeutralGhostRole.DidWin - role: {role.GetRoleName()} DidWin: {win}");

        // Yes, this is bad, but we don't want to break the end game screen to allow other mods to still work with tou mira - Atony
        if (role is JesterRole && win &&
            OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.EndsGame)
        {
            Info($"Jester - Player: {Player.Data.PlayerName}");
            Player.Data.IsDead = false;
        }

        return win;
    }
}