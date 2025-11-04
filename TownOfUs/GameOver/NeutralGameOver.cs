using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.GameOver;

public sealed class NeutralGameOver : CustomGameOver
{
    private Color _roleColor;
    private RoleBehaviour _role;
    private bool _soloWin = true;

    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        if (winners is not [{ Role: RoleBehaviour role and ITownOfUsRole tRole }])
        {
            return false;
        }

        var mainRole = role;

        Error(
            $"VerifyCondition - mainRole: '{mainRole.GetRoleName()}', IsDead: '{role.IsDead}'");

        if (role.IsDead && role is not PhantomTouRole or HaunterRole)
        {
            mainRole = role.Player.GetRoleWhenAlive();

            Error($"VerifyCondition - RoleWhenAlive: '{mainRole?.GetRoleName()}'");
        }

        _role = mainRole!;
        if (PlayerControl.AllPlayerControls.ToArray().Any(x => x != role.Player && x.GetRoleWhenAlive() == mainRole))
        {
            _soloWin = false;
        }
        _roleColor = mainRole!.TeamColor;

        return tRole.WinConditionMet();
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, _roleColor);

        var text = Object.Instantiate(endGameManager.WinText);
        var winText = _soloWin ? TouLocale.GetParsed("SoloWin") : TouLocale.GetParsed("TeamWin");
        winText = winText.Replace("<role>", _role.GetRoleName());
        text.text = $"{winText}!";
        text.color = _roleColor;
        GameHistory.WinningFaction = $"<color=#{_roleColor.ToHtmlStringRGBA()}>{TouLocale.GetParsed("TeamWin").Replace("<role>", _role.GetRoleName())}</color>";

        var pos = endGameManager.WinText.transform.localPosition;
        pos.y = 1.5f;
        pos += Vector3.down * 0.15f;
        text.transform.localScale = new Vector3(1f, 1f, 1f);

        text.transform.position = pos;
        text.text = $"<size=4>{text.text}</size>";
    }
}